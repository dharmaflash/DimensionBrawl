using System;
using System.Collections.Generic;
using DimensionBrawl.Presentation;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    public enum BossLaserSummonPatternState
    {
        Inactive = 0,
        WaitingForAdvance = 1,
        Telegraph = 2,
        Active = 3,
        Recovery = 4,
        Reposition = 5,
        Retarget = 6
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(SummonFrontlineProxy))]
    public sealed class BossLaserSummonPattern : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int TintColorId = Shader.PropertyToID("_TintColor");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly Renderer[] EmptyRenderers = Array.Empty<Renderer>();
        private static readonly ParticleSystem[] EmptyParticles = Array.Empty<ParticleSystem>();
        private static readonly CombatVfxCueVisual[] EmptyCueVisuals = Array.Empty<CombatVfxCueVisual>();
        private static readonly LineRenderer[] EmptyLineRenderers = Array.Empty<LineRenderer>();
        private const string LaserSustainLoopAudioName = "BossLaserSustainLoopAudio";

        [SerializeField] private SummonFrontlineProxy proxy;
        [SerializeField] private CombatHealth sourceHealth;
        [SerializeField] private Transform target;
        [SerializeField] private CombatHealth targetHealth;
        [SerializeField] private Transform laserOrigin;
        [SerializeField] private LayerMask targetLayers = Physics.DefaultRaycastLayers;

        [Header("Cadence")]
        [SerializeField, Min(0.05f)] private float telegraphSeconds = 0.78f;
        [SerializeField, Min(0f)] private float aimLockSeconds = 0.2f;
        [SerializeField, Min(0.05f)] private float activeSeconds = 0.92f;
        [SerializeField, Min(0f)] private float recoverySeconds = 0.42f;
        [SerializeField, Min(0.05f)] private float repositionSeconds = 0.62f;
        [SerializeField, Min(0f)] private float retargetSettleSeconds = 0.18f;
        [SerializeField, Min(0f)] private float aimTurnSpeedDegrees = 720f;

        [Header("Laser")]
        [SerializeField, Min(0.1f)] private float laserLength = 22f;
        [SerializeField, Min(0.01f)] private float hitRadius = 0.62f;
        [SerializeField, Min(0f)] private float targetHeightOffset = 1.05f;
        [SerializeField, Min(0f)] private float damagePerSecond = 58f;
        [SerializeField, Min(0.05f)] private float damageIntervalSeconds = 0.12f;
        [SerializeField, Min(0f)] private float tierDamageBonus = 0.12f;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float desiredDistanceFromTarget = 4.2f;
        [SerializeField, Min(0f)] private float strafeDistance = 1.45f;
        [SerializeField, Min(0f)] private float repositionMoveSpeed = 3.8f;

        [Header("Presentation")]
        [SerializeField] private LineRenderer telegraphLine;
        [SerializeField] private GameObject telegraphVfxPrefab;
        [SerializeField] private Color telegraphStartColor = new Color(1f, 0.18f, 0.08f, 0.26f);
        [SerializeField] private Color telegraphEndColor = new Color(1f, 0.28f, 0.12f, 0.96f);
        [SerializeField, Min(0.001f)] private float telegraphStartWidth = 0.045f;
        [SerializeField, Min(0.001f)] private float telegraphEndWidth = 0.115f;
        [SerializeField, Min(0.01f)] private float telegraphVfxWidthScale = 0.72f;
        [SerializeField, Min(0.01f)] private float telegraphVfxLengthScale = 1.12f;
        [SerializeField, Min(0f)] private float telegraphVfxPulseScale = 0.08f;
        [SerializeField, Min(0f)] private float telegraphVfxPulseSpeed = 18f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioSource laserSustainLoopAudioSource;
        [SerializeField] private AudioClip telegraphSfx;
        [SerializeField] private AudioClip laserFireSfx;
        [SerializeField] private AudioClip laserSustainLoopSfx;
        [SerializeField] private AudioClip laserEndSfx;
        [SerializeField, Range(0f, 1f)] private float telegraphSfxVolume = 0.72f;
        [SerializeField, Range(0f, 1f)] private float laserFireSfxVolume = 0.9f;
        [SerializeField, Range(0f, 1f)] private float laserSustainLoopSfxVolume = 0.56f;
        [SerializeField, Range(0f, 1f)] private float laserEndSfxVolume = 0.52f;

        private readonly Collider[] hitBuffer = new Collider[32];
        private readonly List<CombatHealth> uniqueTargets = new List<CombatHealth>(8);
        private SummonAttackBeamPresenter beamPresenter;
        private MaterialPropertyBlock telegraphPropertyBlock;
        private GameObject telegraphVfxInstance;
        private Transform telegraphVfxTransform;
        private Renderer[] telegraphVfxRenderers = EmptyRenderers;
        private ParticleSystem[] telegraphVfxParticles = EmptyParticles;
        private CombatVfxCueVisual[] telegraphVfxCueVisuals = EmptyCueVisuals;
        private LineRenderer[] telegraphVfxLineRenderers = EmptyLineRenderers;
        private float[] telegraphLineBaseWidths = Array.Empty<float>();
        private float[] telegraphLineBaseLengths = Array.Empty<float>();
        private float[] telegraphLineUvOffsets = Array.Empty<float>();
        private BossLaserSummonPatternState state;
        private DamageTeam sourceTeam = DamageTeam.Enemy;
        private Vector3 lockedDirection = Vector3.back;
        private float stateTimer;
        private float nextDamageTime;
        private int cycleIndex;
        private int totalDamageTickCount;
        private bool laserSustainLoopPlaying;

        public BossLaserSummonPatternState CurrentState => state;
        public bool IsLaserActive => state == BossLaserSummonPatternState.Active;
        public int TotalDamageTickCount => totalDamageTickCount;
        public float TelegraphProgress01 => state == BossLaserSummonPatternState.Telegraph
            ? Mathf.Clamp01(stateTimer / Mathf.Max(0.05f, telegraphSeconds))
            : 0f;
        public event Action<BossLaserSummonPattern> LaserFired;
        public event Action<BossLaserSummonPattern, CombatHealth, Vector3, Vector3> DamageApplied;

        private void Awake()
        {
            ResolveReferences();
            HideTelegraphVisual();
        }

        private void OnEnable()
        {
            ResolveReferences();
            EnterInactive();
        }

        private void OnDisable()
        {
            HideTelegraphVisual();
            ClearBeamPresenter();
            StopLaserSustainLoop(playEndSfx: false);
            state = BossLaserSummonPatternState.Inactive;
        }

        private void OnDestroy()
        {
            if (telegraphVfxInstance != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(telegraphVfxInstance);
                }
                else
                {
                    DestroyImmediate(telegraphVfxInstance);
                }

                telegraphVfxInstance = null;
            }
        }

        private void Update()
        {
            ResolveReferences();
            if (proxy == null || !proxy.IsActive)
            {
                EnterInactive();
                return;
            }

            float deltaTime = Time.deltaTime * CombatTimeDilationReceiver.ResolveTimeScale(this);
            if (deltaTime <= 0f)
            {
                return;
            }

            switch (state)
            {
                case BossLaserSummonPatternState.Inactive:
                    EnterWaitingForAdvance();
                    break;
                case BossLaserSummonPatternState.WaitingForAdvance:
                    UpdateWaitingForAdvance();
                    break;
                case BossLaserSummonPatternState.Telegraph:
                    UpdateTelegraph(deltaTime);
                    break;
                case BossLaserSummonPatternState.Active:
                    UpdateActive(deltaTime);
                    break;
                case BossLaserSummonPatternState.Recovery:
                    UpdateRecovery(deltaTime);
                    break;
                case BossLaserSummonPatternState.Reposition:
                    UpdateReposition(deltaTime);
                    break;
                case BossLaserSummonPatternState.Retarget:
                    UpdateRetarget(deltaTime);
                    break;
            }
        }

        public void ConfigurePattern(
            Transform newTarget,
            DamageTeam newSourceTeam,
            float newDamagePerSecond,
            float newDamageIntervalSeconds,
            float newMoveSpeed)
        {
            target = newTarget;
            targetHealth = target != null ? target.GetComponentInParent<CombatHealth>() : null;
            sourceTeam = newSourceTeam;
            damagePerSecond = Mathf.Max(0f, newDamagePerSecond);
            damageIntervalSeconds = Mathf.Max(0.05f, newDamageIntervalSeconds);
            if (newMoveSpeed > 0f)
            {
                repositionMoveSpeed = newMoveSpeed;
            }
        }

        private void UpdateWaitingForAdvance()
        {
            HideTelegraphVisual();
            if (proxy.IsAdvancing || proxy.AdvanceProgress01 < 0.96f)
            {
                return;
            }

            EnterTelegraph();
        }

        private void UpdateTelegraph(float deltaTime)
        {
            stateTimer += deltaTime;
            proxy.RequestAdvanceHold(0.08f);

            float lockStartTime = Mathf.Max(0f, telegraphSeconds - aimLockSeconds);
            if (stateTimer <= lockStartTime)
            {
                lockedDirection = ResolveTargetDirection();
            }

            FaceLockedDirection(deltaTime);
            float progress = Mathf.Clamp01(stateTimer / Mathf.Max(0.05f, telegraphSeconds));
            ShowTelegraphVisual(progress);

            if (stateTimer >= telegraphSeconds)
            {
                EnterActive();
            }
        }

        private void UpdateActive(float deltaTime)
        {
            stateTimer += deltaTime;
            proxy.RequestAdvanceHold(0.08f);
            FaceLockedDirection(deltaTime);
            HideTelegraphVisual();
            SyncBeamPresenter();

            if (Time.time >= nextDamageTime)
            {
                ApplyLaserDamage(Mathf.Max(0.05f, damageIntervalSeconds));
                nextDamageTime = Time.time + Mathf.Max(0.05f, damageIntervalSeconds);
            }

            if (stateTimer >= activeSeconds)
            {
                EnterRecovery();
            }
        }

        private void UpdateRecovery(float deltaTime)
        {
            stateTimer += deltaTime;
            proxy.RequestAdvanceHold(0.08f);
            HideTelegraphVisual();
            if (stateTimer >= recoverySeconds)
            {
                EnterReposition();
            }
        }

        private void UpdateReposition(float deltaTime)
        {
            stateTimer += deltaTime;
            HideTelegraphVisual();
            bool hasAdvance = proxy != null && proxy.AdvanceDistance > 0.05f;
            bool advanceComplete = !hasAdvance || proxy.AdvanceProgress01 >= 0.98f;
            bool timedOut = stateTimer >= repositionSeconds + Mathf.Max(0.18f, retargetSettleSeconds);
            if (advanceComplete || timedOut)
            {
                EnterRetarget();
            }
        }

        private void UpdateRetarget(float deltaTime)
        {
            stateTimer += deltaTime;
            proxy.RequestAdvanceHold(0.08f);
            HideTelegraphVisual();
            lockedDirection = ResolveTargetDirection();
            FaceLockedDirection(deltaTime);
            if (stateTimer >= retargetSettleSeconds)
            {
                EnterTelegraph();
            }
        }

        private void EnterInactive()
        {
            if (state == BossLaserSummonPatternState.Inactive)
            {
                HideTelegraphVisual();
                ClearBeamPresenter();
                return;
            }

            state = BossLaserSummonPatternState.Inactive;
            stateTimer = 0f;
            HideTelegraphVisual();
            ClearBeamPresenter();
            StopLaserSustainLoop(playEndSfx: false);
        }

        private void EnterWaitingForAdvance()
        {
            state = BossLaserSummonPatternState.WaitingForAdvance;
            stateTimer = 0f;
            HideTelegraphVisual();
            ClearBeamPresenter();
            StopLaserSustainLoop(playEndSfx: false);
        }

        private void EnterTelegraph()
        {
            state = BossLaserSummonPatternState.Telegraph;
            stateTimer = 0f;
            ClearBeamPresenter();
            StopLaserSustainLoop(playEndSfx: false);
            lockedDirection = ResolveTargetDirection();
            ShowTelegraphVisual(0f);
            PlaySfx(telegraphSfx, telegraphSfxVolume);
        }

        private void EnterActive()
        {
            state = BossLaserSummonPatternState.Active;
            stateTimer = 0f;
            nextDamageTime = 0f;
            proxy.NotifyAttackPerformed(activeSeconds + 0.05f);
            LaserFired?.Invoke(this);
            PlaySfx(laserFireSfx, laserFireSfxVolume);
            StartLaserSustainLoop();
            HideTelegraphVisual();
            SyncBeamPresenter();
        }

        private void EnterRecovery()
        {
            state = BossLaserSummonPatternState.Recovery;
            stateTimer = 0f;
            StopLaserSustainLoop(playEndSfx: true);
            HideTelegraphVisual();
            ClearBeamPresenter();
        }

        private void EnterReposition()
        {
            state = BossLaserSummonPatternState.Reposition;
            stateTimer = 0f;
            ClearBeamPresenter();
            StopLaserSustainLoop(playEndSfx: false);
            cycleIndex++;
            proxy.BeginAdvanceTo(
                ResolveRepositionTarget(),
                repositionSeconds,
                repositionMoveSpeed);
        }

        private void EnterRetarget()
        {
            state = BossLaserSummonPatternState.Retarget;
            stateTimer = 0f;
            ClearBeamPresenter();
            HideTelegraphVisual();
            StopLaserSustainLoop(playEndSfx: false);
            lockedDirection = ResolveTargetDirection();
        }

        private void ApplyLaserDamage(float interval)
        {
            Vector3 origin = ResolveLaserOrigin();
            Vector3 end = origin + lockedDirection * Mathf.Max(0.1f, laserLength);
            int hitCount = Physics.OverlapCapsuleNonAlloc(
                origin,
                end,
                Mathf.Max(0.01f, hitRadius),
                hitBuffer,
                targetLayers,
                QueryTriggerInteraction.Collide);

            uniqueTargets.Clear();
            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = hitBuffer[i];
                hitBuffer[i] = null;
                if (hit == null)
                {
                    continue;
                }

                CombatHealth health = hit.GetComponentInParent<CombatHealth>();
                if (health == null
                    || health == sourceHealth
                    || !health.IsAlive
                    || !CombatTeamUtility.AreHostile(sourceTeam, health.Team)
                    || uniqueTargets.Contains(health))
                {
                    continue;
                }

                uniqueTargets.Add(health);
                Vector3 point = ClosestPointOnSegment(origin, end, health.transform.position);
                float tierScale = 1f + Mathf.Max(0, proxy.ActiveTier - 1) * tierDamageBonus;
                DamageInfo damageInfo = new DamageInfo(
                    sourceHealth,
                    sourceTeam,
                    damagePerSecond * interval * tierScale,
                    point,
                    lockedDirection,
                    0f,
                    DamageResponsePolicy.FlashOnly,
                    CombatControlLockPolicy.None);
                if (health.TryApplyDamage(damageInfo))
                {
                    totalDamageTickCount++;
                    DamageApplied?.Invoke(this, health, point, lockedDirection);
                }
            }
        }

        private Vector3 ResolveRepositionTarget()
        {
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = ResolveTargetPoint();
            Vector3 away = Vector3.ProjectOnPlane(currentPosition - targetPosition, Vector3.up);
            if (away.sqrMagnitude <= 0.0001f)
            {
                away = -Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            }

            if (away.sqrMagnitude <= 0.0001f)
            {
                away = Vector3.back;
            }

            away.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, away).normalized;
            float side = cycleIndex % 2 == 0 ? 1f : -1f;
            Vector3 desired = targetPosition
                + away * Mathf.Max(0f, desiredDistanceFromTarget)
                + right * (side * Mathf.Max(0f, strafeDistance));
            desired.y = currentPosition.y;
            return desired;
        }

        private Vector3 ResolveTargetPoint()
        {
            if (target != null)
            {
                return target.position + Vector3.up * Mathf.Max(0f, targetHeightOffset);
            }

            if (targetHealth != null)
            {
                return targetHealth.transform.position + Vector3.up * Mathf.Max(0f, targetHeightOffset);
            }

            return transform.position + transform.forward * Mathf.Max(1f, desiredDistanceFromTarget);
        }

        private Vector3 ResolveTargetDirection()
        {
            Vector3 origin = ResolveLaserOrigin();
            Vector3 direction = ResolveTargetPoint() - origin;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            }

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.back;
            }

            return direction.normalized;
        }

        private Vector3 ResolveLaserOrigin()
        {
            Transform origin = laserOrigin != null
                ? laserOrigin
                : proxy != null
                    ? proxy.ProjectileOrigin
                    : transform;
            return origin.position;
        }

        private void FaceLockedDirection(float deltaTime)
        {
            Vector3 planarDirection = Vector3.ProjectOnPlane(lockedDirection, Vector3.up);
            if (planarDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(planarDirection.normalized, Vector3.up);
            transform.rotation = aimTurnSpeedDegrees > 0f && deltaTime > 0f
                ? Quaternion.RotateTowards(transform.rotation, targetRotation, aimTurnSpeedDegrees * deltaTime)
                : targetRotation;
        }

        private void ShowTelegraphVisual(float progress)
        {
            HideLegacyTelegraphLine();
            EnsureTelegraphVfx();
            if (telegraphVfxInstance == null || telegraphVfxTransform == null)
            {
                return;
            }

            Vector3 origin = ResolveLaserOrigin();
            float length = Mathf.Max(0.1f, laserLength);
            Vector3 direction = lockedDirection.sqrMagnitude > 0.0001f
                ? lockedDirection.normalized
                : Vector3.forward;

            if (!telegraphVfxInstance.activeSelf)
            {
                telegraphVfxInstance.SetActive(true);
                RestartTelegraphParticles();
            }

            progress = Mathf.Clamp01(progress);
            telegraphVfxTransform.position = origin;
            telegraphVfxTransform.rotation = Quaternion.LookRotation(direction, ResolveStableUp(direction));

            float parentScale = ResolveParentUniformScale(telegraphVfxTransform);
            float pulse = 1f
                + Mathf.Sin(Time.time * Mathf.Max(0f, telegraphVfxPulseSpeed))
                * Mathf.Max(0f, telegraphVfxPulseScale)
                * Mathf.Lerp(0.35f, 1f, progress);
            float legacyWidthRatio = Mathf.Clamp(
                Mathf.Max(0.001f, telegraphStartWidth) / Mathf.Max(0.001f, telegraphEndWidth),
                0.25f,
                1f);
            float width = Mathf.Max(0.01f, telegraphVfxWidthScale)
                * Mathf.Lerp(legacyWidthRatio, 1f, Mathf.SmoothStep(0f, 1f, progress));

            float worldLength = Mathf.Max(0.01f, length * telegraphVfxLengthScale);
            if (telegraphVfxLineRenderers != null && telegraphVfxLineRenderers.Length > 0)
            {
                telegraphVfxTransform.localScale = new Vector3(
                    1f / parentScale,
                    1f / parentScale,
                    ResolveTelegraphLineRootScale(worldLength, parentScale));
                ApplyTelegraphLineRenderers(width, pulse, worldLength, progress);
            }
            else
            {
                telegraphVfxTransform.localScale = new Vector3(
                    Mathf.Max(0.01f, width * pulse) / parentScale,
                    1f / parentScale,
                    worldLength / parentScale);
            }

            ApplyTelegraphVfxColor(progress);
        }

        private void HideTelegraphVisual()
        {
            HideLegacyTelegraphLine();
            if (telegraphVfxInstance != null && telegraphVfxInstance.activeSelf)
            {
                StopTelegraphParticles();
                telegraphVfxInstance.SetActive(false);
            }
        }

        private void SyncBeamPresenter()
        {
            ResolveReferences();
            if (beamPresenter == null)
            {
                return;
            }

            Vector3 origin = ResolveLaserOrigin();
            Vector3 end = origin + lockedDirection * Mathf.Max(0.1f, laserLength);
            beamPresenter.SetWorldBeamEndpoints(origin, end);
        }

        private void ClearBeamPresenter()
        {
            ResolveReferences();
            beamPresenter?.ClearWorldBeamEndpoints();
        }

        private void EnsureTelegraphVfx()
        {
            if (telegraphVfxInstance != null || telegraphVfxPrefab == null)
            {
                return;
            }

            telegraphVfxInstance = Instantiate(telegraphVfxPrefab, transform);
            telegraphVfxInstance.name = "BossLaserTelegraphVfx";
            telegraphVfxTransform = telegraphVfxInstance.transform;
            telegraphVfxCueVisuals = telegraphVfxInstance.GetComponentsInChildren<CombatVfxCueVisual>(includeInactive: true);
            for (int i = 0; i < telegraphVfxCueVisuals.Length; i++)
            {
                CombatVfxCueVisual cueVisual = telegraphVfxCueVisuals[i];
                if (cueVisual != null)
                {
                    cueVisual.enabled = false;
                }
            }

            telegraphVfxRenderers = telegraphVfxInstance.GetComponentsInChildren<Renderer>(includeInactive: true);
            for (int i = 0; i < telegraphVfxRenderers.Length; i++)
            {
                Renderer renderer = telegraphVfxRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.enabled = true;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            telegraphVfxParticles = telegraphVfxInstance.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
            telegraphVfxLineRenderers = telegraphVfxInstance.GetComponentsInChildren<LineRenderer>(includeInactive: true);
            CaptureTelegraphLineRendererDefaults();
            telegraphVfxInstance.SetActive(false);
        }

        private void CaptureTelegraphLineRendererDefaults()
        {
            if (telegraphVfxLineRenderers == null || telegraphVfxLineRenderers.Length == 0)
            {
                telegraphLineBaseWidths = Array.Empty<float>();
                telegraphLineBaseLengths = Array.Empty<float>();
                telegraphLineUvOffsets = Array.Empty<float>();
                return;
            }

            telegraphLineBaseWidths = new float[telegraphVfxLineRenderers.Length];
            telegraphLineBaseLengths = new float[telegraphVfxLineRenderers.Length];
            telegraphLineUvOffsets = new float[telegraphVfxLineRenderers.Length];
            for (int i = 0; i < telegraphVfxLineRenderers.Length; i++)
            {
                LineRenderer lineRenderer = telegraphVfxLineRenderers[i];
                if (lineRenderer == null)
                {
                    telegraphLineBaseWidths[i] = 1f;
                    telegraphLineBaseLengths[i] = 10f;
                    continue;
                }

                lineRenderer.useWorldSpace = false;
                lineRenderer.textureMode = LineTextureMode.Tile;
                lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lineRenderer.receiveShadows = false;
                telegraphLineBaseWidths[i] = Mathf.Max(0.01f, lineRenderer.widthMultiplier);
                telegraphLineBaseLengths[i] = ResolveLineRendererLength(lineRenderer);
                telegraphLineUvOffsets[i] = UnityEngine.Random.Range(0f, 5f);
            }
        }

        private float ResolveTelegraphLineRootScale(float worldLength, float parentScale)
        {
            float authoredLength = 0f;
            for (int i = 0; i < telegraphLineBaseLengths.Length; i++)
            {
                authoredLength = Mathf.Max(authoredLength, telegraphLineBaseLengths[i]);
            }

            return worldLength / Mathf.Max(0.01f, authoredLength * parentScale);
        }

        private void ApplyTelegraphLineRenderers(float width, float pulse, float worldLength, float progress)
        {
            if (telegraphVfxLineRenderers == null)
            {
                return;
            }

            Color color = Color.Lerp(telegraphStartColor, telegraphEndColor, Mathf.SmoothStep(0f, 1f, progress));
            float textureScale = Mathf.Max(0.01f, worldLength * 0.12f);
            float widthScale = Mathf.Max(0.04f, width * 0.34f * pulse);
            for (int i = 0; i < telegraphVfxLineRenderers.Length; i++)
            {
                LineRenderer lineRenderer = telegraphVfxLineRenderers[i];
                if (lineRenderer == null)
                {
                    continue;
                }

                float baseWidth = i < telegraphLineBaseWidths.Length ? telegraphLineBaseWidths[i] : 1f;
                float baseLength = i < telegraphLineBaseLengths.Length ? telegraphLineBaseLengths[i] : 10f;
                lineRenderer.enabled = true;
                lineRenderer.startColor = color;
                lineRenderer.endColor = new Color(color.r, color.g, color.b, color.a * 0.45f);
                lineRenderer.widthMultiplier = baseWidth * widthScale;
                if (lineRenderer.positionCount < 2)
                {
                    lineRenderer.positionCount = 2;
                }

                lineRenderer.SetPosition(0, Vector3.zero);
                lineRenderer.SetPosition(1, new Vector3(0f, 0f, Mathf.Max(0.01f, baseLength)));
                ApplyTelegraphLineMaterialPlayback(lineRenderer, i, textureScale);
            }
        }

        private void ApplyTelegraphLineMaterialPlayback(LineRenderer lineRenderer, int lineIndex, float textureScale)
        {
            Material material = lineRenderer != null ? lineRenderer.material : null;
            if (material == null)
            {
                return;
            }

            Vector2 tiling = new Vector2(textureScale, 1f);
            SetTextureScaleIfPresent(material, "_BaseMap", tiling);
            SetTextureScaleIfPresent(material, "_MainTex", tiling);
            SetTextureScaleIfPresent(material, "_MainTexture", tiling);

            float initialOffset = lineIndex < telegraphLineUvOffsets.Length ? telegraphLineUvOffsets[lineIndex] : 0f;
            Vector2 offset = new Vector2(Time.time * -5.5f + initialOffset, 0f);
            if (material.HasProperty("_Offset"))
            {
                material.SetVector("_Offset", offset);
            }

            SetTextureOffsetIfPresent(material, "_BaseMap", offset);
            SetTextureOffsetIfPresent(material, "_MainTex", offset);
            SetTextureOffsetIfPresent(material, "_MainTexture", offset);
        }

        private static float ResolveLineRendererLength(LineRenderer lineRenderer)
        {
            if (lineRenderer == null || lineRenderer.positionCount < 2)
            {
                return 10f;
            }

            Vector3 start = lineRenderer.GetPosition(0);
            Vector3 end = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
            return Mathf.Max(0.01f, Vector3.Distance(start, end));
        }

        private void ApplyTelegraphVfxColor(float progress)
        {
            if (telegraphVfxRenderers == null || telegraphVfxRenderers.Length == 0)
            {
                return;
            }

            telegraphPropertyBlock ??= new MaterialPropertyBlock();
            float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress));
            Color color = Color.Lerp(telegraphStartColor, telegraphEndColor, eased);
            color.a *= Mathf.Lerp(0.78f, 1f, eased);
            Color emission = color * Mathf.Lerp(1.15f, 2.2f, eased);
            emission.a = color.a;
            for (int i = 0; i < telegraphVfxRenderers.Length; i++)
            {
                Renderer renderer = telegraphVfxRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(telegraphPropertyBlock);
                telegraphPropertyBlock.SetColor(BaseColorId, color);
                telegraphPropertyBlock.SetColor(ColorId, color);
                telegraphPropertyBlock.SetColor(TintColorId, color);
                telegraphPropertyBlock.SetColor(EmissionColorId, emission);
                renderer.SetPropertyBlock(telegraphPropertyBlock);
            }
        }

        private void RestartTelegraphParticles()
        {
            if (telegraphVfxParticles == null)
            {
                return;
            }

            for (int i = 0; i < telegraphVfxParticles.Length; i++)
            {
                ParticleSystem particle = telegraphVfxParticles[i];
                if (particle == null)
                {
                    continue;
                }

                particle.Clear(withChildren: true);
                particle.Play(withChildren: true);
            }
        }

        private void StopTelegraphParticles()
        {
            if (telegraphVfxParticles == null)
            {
                return;
            }

            for (int i = 0; i < telegraphVfxParticles.Length; i++)
            {
                ParticleSystem particle = telegraphVfxParticles[i];
                if (particle == null)
                {
                    continue;
                }

                particle.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void HideLegacyTelegraphLine()
        {
            if (telegraphLine != null)
            {
                telegraphLine.gameObject.SetActive(false);
            }
        }

        private void StartLaserSustainLoop()
        {
            if (laserSustainLoopSfx == null || laserSustainLoopSfxVolume <= 0f)
            {
                laserSustainLoopPlaying = false;
                return;
            }

            AudioSource source = ResolveLaserSustainLoopAudioSource();
            if (source == null)
            {
                laserSustainLoopPlaying = false;
                return;
            }

            source.clip = laserSustainLoopSfx;
            source.playOnAwake = false;
            source.loop = true;
            source.volume = Mathf.Clamp01(laserSustainLoopSfxVolume);
            source.pitch = 1f;
            source.spatialBlend = 1f;
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1.6f;
            source.maxDistance = 20f;
            source.priority = 132;
            source.Stop();
            source.Play();
            laserSustainLoopPlaying = true;
        }

        private void StopLaserSustainLoop(bool playEndSfx)
        {
            bool shouldPlayEnd = playEndSfx && laserSustainLoopPlaying;
            laserSustainLoopPlaying = false;
            if (laserSustainLoopAudioSource != null)
            {
                laserSustainLoopAudioSource.Stop();
            }

            if (shouldPlayEnd)
            {
                PlaySfx(laserEndSfx, laserEndSfxVolume);
            }
        }

        private AudioSource ResolveLaserSustainLoopAudioSource()
        {
            if (laserSustainLoopAudioSource != null)
            {
                return laserSustainLoopAudioSource;
            }

            Transform child = transform.Find(LaserSustainLoopAudioName);
            if (child != null)
            {
                laserSustainLoopAudioSource = child.GetComponent<AudioSource>();
                if (laserSustainLoopAudioSource != null)
                {
                    return laserSustainLoopAudioSource;
                }
            }

            if (!Application.isPlaying)
            {
                return null;
            }

            GameObject audioObject = new GameObject(LaserSustainLoopAudioName);
            audioObject.transform.SetParent(transform, worldPositionStays: false);
            audioObject.transform.localPosition = Vector3.zero;
            laserSustainLoopAudioSource = audioObject.AddComponent<AudioSource>();
            return laserSustainLoopAudioSource;
        }

        private void PlaySfx(AudioClip clip, float volume)
        {
            if (clip == null || volume <= 0f)
            {
                return;
            }

            if (audioSource != null)
            {
                audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
                return;
            }

            AudioSource.PlayClipAtPoint(clip, ResolveLaserOrigin(), Mathf.Clamp01(volume));
        }

        private static Vector3 ResolveStableUp(Vector3 forward)
        {
            Vector3 normalized = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
            return Mathf.Abs(Vector3.Dot(normalized, Vector3.up)) > 0.96f
                ? Vector3.forward
                : Vector3.up;
        }

        private static float ResolveParentUniformScale(Transform child)
        {
            if (child == null || child.parent == null)
            {
                return 1f;
            }

            Vector3 scale = child.parent.lossyScale;
            return Mathf.Max(
                0.01f,
                (Mathf.Abs(scale.x) + Mathf.Abs(scale.y) + Mathf.Abs(scale.z)) / 3f);
        }

        private static void SetTextureScaleIfPresent(Material material, string propertyName, Vector2 scale)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetTextureScale(propertyName, scale);
            }
        }

        private static void SetTextureOffsetIfPresent(Material material, string propertyName, Vector2 offset)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetTextureOffset(propertyName, offset);
            }
        }

        private void ResolveReferences()
        {
            if (proxy == null)
            {
                proxy = GetComponent<SummonFrontlineProxy>();
            }

            if (sourceHealth == null)
            {
                sourceHealth = proxy != null ? proxy.Health : GetComponent<CombatHealth>();
            }

            if (laserOrigin == null && proxy != null)
            {
                laserOrigin = proxy.ProjectileOrigin;
            }

            if (beamPresenter == null)
            {
                beamPresenter = GetComponent<SummonAttackBeamPresenter>();
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (laserSustainLoopAudioSource == null)
            {
                Transform loopAudio = transform.Find(LaserSustainLoopAudioName);
                if (loopAudio != null)
                {
                    laserSustainLoopAudioSource = loopAudio.GetComponent<AudioSource>();
                }
            }

            if (targetHealth == null && target != null)
            {
                targetHealth = target.GetComponentInParent<CombatHealth>();
            }
        }

        private static Vector3 ClosestPointOnSegment(Vector3 start, Vector3 end, Vector3 point)
        {
            Vector3 segment = end - start;
            float lengthSqr = segment.sqrMagnitude;
            if (lengthSqr <= 0.0001f)
            {
                return start;
            }

            float t = Vector3.Dot(point - start, segment) / lengthSqr;
            return start + segment * Mathf.Clamp01(t);
        }
    }
}
