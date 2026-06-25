using System.Collections.Generic;
using UnityEngine;

namespace IsekaiBrawl.Gameplay
{
    public enum ProjectilePerspectivePreset
    {
        Balanced = 0,
        PathFirst = 1,
        StrikeZone = 2
    }

    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyProjectile : MonoBehaviour
    {
        [System.Serializable]
        private struct PerspectiveBaseTuning
        {
            public float impactPlaneForwardOffset;
            public float telegraphLeadTime;
            public float pathRevealLeadTime;
            public float imminentCueLeadTime;
            public float cuePulseDuration;
            public float farScaleMultiplier;
            public float nearScaleMultiplier;
            public float nearLengthMultiplier;
            public float nearTrailWidthMultiplier;
            public float nearTrailTimeMultiplier;
        }

        private readonly struct PerspectivePresetModifiers
        {
            public PerspectivePresetModifiers(
                float telegraphLeadScale,
                float pathLeadScale,
                float imminentLeadScale,
                float cuePulseScale,
                float farScaleScale,
                float nearScaleScale)
            {
                TelegraphLeadScale = telegraphLeadScale;
                PathLeadScale = pathLeadScale;
                ImminentLeadScale = imminentLeadScale;
                CuePulseScale = cuePulseScale;
                FarScaleScale = farScaleScale;
                NearScaleScale = nearScaleScale;
            }

            public float TelegraphLeadScale { get; }
            public float PathLeadScale { get; }
            public float ImminentLeadScale { get; }
            public float CuePulseScale { get; }
            public float FarScaleScale { get; }
            public float NearScaleScale { get; }
        }

        private static readonly List<EnemyProjectile> ActiveProjectiles = new();
        private static readonly PerspectivePresetModifiers[] PerspectivePresetTable =
        {
            new(1f, 1f, 1f, 1f, 1f, 1f),
            new(0.78f, 1.7f, 1.06f, 1f, 0.9f, 0.88f),
            new(1.12f, 0.92f, 1.15f, 1.08f, 1.06f, 1.14f)
        };
        private static ProjectilePerspectivePreset currentPerspectivePreset = ProjectilePerspectivePreset.Balanced;

        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float maxLifetime = 6f;
        [SerializeField] private float threatRadius = 1.15f;
        [SerializeField] private PerspectiveBaseTuning perspectiveBaseTuning = new()
        {
            impactPlaneForwardOffset = 0.05f,
            telegraphLeadTime = 0.58f,
            pathRevealLeadTime = 1.08f,
            imminentCueLeadTime = 0.24f,
            cuePulseDuration = 0.18f,
            farScaleMultiplier = 0.38f,
            nearScaleMultiplier = 3.1f,
            nearLengthMultiplier = 1.8f,
            nearTrailWidthMultiplier = 1.7f,
            nearTrailTimeMultiplier = 1.18f
        };

        private Vector3 moveDirection = Vector3.back;
        private float damage = 10f;
        private float lifetime;
        private bool wasDodged;
        private PlayerController targetPlayer;
        private JustDodgeDetector targetDodgeDetector;
        private Transform threatIndicator;
        private Renderer threatIndicatorRenderer;
        private Material threatIndicatorMaterial;
        private Transform threatPathIndicator;
        private Renderer threatPathIndicatorRenderer;
        private Material threatPathIndicatorMaterial;
        private Vector3 baseScale = Vector3.one;
        private Vector3 prototypeBaseScale = Vector3.one;
        private Color baseProjectileColor = new(1f, 0.45f, 0.22f, 1f);
        private Renderer projectileRenderer;
        private TrailRenderer projectileTrail;
        private SphereCollider sphereCollider;
        private float defaultMoveSpeed;
        private float defaultThreatRadius;
        private float baseColliderWorldRadius = 0.26f;
        private float currentThreatWidthMultiplier = 1f;
        private float initialDistanceToPlayerPlane = 1f;
        private bool cuePulseRequested;
        private float cuePulseExpiresAt;
        private bool imminentCueTriggered;
        private bool registeredWithDetector;
        private bool resolvedAgainstPlayer;
        private bool hasLastThreatSnapshot;
        private PerspectiveThreatSnapshot lastThreatSnapshot;
        private float impactPlaneForwardOffset;
        private float telegraphLeadTime;
        private float pathRevealLeadTime;
        private float imminentCueLeadTime;
        private float cuePulseDuration;
        private float farScaleMultiplier;
        private float nearScaleMultiplier;
        private float profileFarScaleMultiplier;
        private float profileNearScaleMultiplier;
        private float nearLengthMultiplier;
        private float nearTrailWidthMultiplier;
        private float nearTrailTimeMultiplier;
        private float trailBaseStartWidth;
        private float trailBaseEndWidth;
        private float trailBaseTime;
        private System.Action<EnemyProjectile> resolvedCallback;
        private bool resolutionReported;

        public bool WasDodged => wasDodged;
        public static ProjectilePerspectivePreset CurrentPerspectivePreset => currentPerspectivePreset;

        private void OnEnable()
        {
            if (!ActiveProjectiles.Contains(this))
            {
                ActiveProjectiles.Add(this);
            }

            ApplyPerspectivePreset(currentPerspectivePreset);
        }

        private void OnDisable()
        {
            ActiveProjectiles.Remove(this);
            NotifyResolved();
            if (threatIndicator != null)
            {
                Object.Destroy(threatIndicator.gameObject);
            }

            if (threatPathIndicator != null)
            {
                Object.Destroy(threatPathIndicator.gameObject);
            }
        }

        private void Awake()
        {
            EnsurePerspectiveBaseTuning();

            projectileRenderer = GetComponent<Renderer>();
            projectileTrail = GetComponent<TrailRenderer>();
            sphereCollider = GetComponent<SphereCollider>();

            Rigidbody rigidbodyComponent = GetComponent<Rigidbody>();
            rigidbodyComponent.useGravity = false;
            rigidbodyComponent.isKinematic = true;

            Collider colliderComponent = GetComponent<Collider>();
            colliderComponent.isTrigger = true;

            float enforcedBaseSize = Mathf.Clamp(transform.localScale.x, 0.18f, 0.38f);
            prototypeBaseScale = new Vector3(enforcedBaseSize * 0.68f, enforcedBaseSize * 0.68f, enforcedBaseSize * 1.12f);
            baseScale = prototypeBaseScale;
            defaultMoveSpeed = moveSpeed;
            defaultThreatRadius = threatRadius;
            profileFarScaleMultiplier = perspectiveBaseTuning.farScaleMultiplier;
            profileNearScaleMultiplier = perspectiveBaseTuning.nearScaleMultiplier;
            ApplyPerspectivePreset(currentPerspectivePreset);
            if (sphereCollider != null)
            {
                baseColliderWorldRadius = sphereCollider.radius * enforcedBaseSize;
            }

            EnsureTrail(baseProjectileColor, 1f);
            ApplyVisualTint(baseProjectileColor);
            RefreshCollisionRadius();
            EnsureThreatIndicator();
        }

        public static bool TrySetPerspectivePreset(ProjectilePerspectivePreset preset)
        {
            if (currentPerspectivePreset == preset)
            {
                return false;
            }

            currentPerspectivePreset = preset;
            for (int index = ActiveProjectiles.Count - 1; index >= 0; index--)
            {
                EnemyProjectile projectile = ActiveProjectiles[index];
                if (projectile == null)
                {
                    continue;
                }

                projectile.ApplyPerspectivePreset(preset);
            }

            return true;
        }

        public static string GetPerspectivePresetLabel(ProjectilePerspectivePreset preset)
        {
            return preset switch
            {
                ProjectilePerspectivePreset.PathFirst => "PATH FIRST",
                ProjectilePerspectivePreset.StrikeZone => "STRIKE ZONE",
                _ => "BALANCED"
            };
        }

        private void Update()
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
            transform.Rotate(Vector3.forward, 260f * Time.deltaTime, Space.Self);
            lifetime += Time.deltaTime;

            UpdatePerspectivePresentation();
            EvaluatePlayerImpactResolution();

            if (lifetime >= maxLifetime || transform.position.z < -1f)
            {
                DestroyProjectile(spawnImpact: false);
            }
        }

        public void Initialize(Vector3 direction, float projectileDamage, PlayerController player = null)
        {
            Initialize(direction, projectileDamage, player, ProjectileProfile.Default, null);
        }

        public void Initialize(Vector3 direction, float projectileDamage, PlayerController player, ProjectileProfile profile)
        {
            Initialize(direction, projectileDamage, player, profile, null);
        }

        public void Initialize(
            Vector3 direction,
            float projectileDamage,
            PlayerController player,
            ProjectileProfile profile,
            System.Action<EnemyProjectile> onResolved)
        {
            moveDirection = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.back;
            damage = projectileDamage;
            lifetime = 0f;
            wasDodged = false;
            cuePulseRequested = false;
            cuePulseExpiresAt = 0f;
            imminentCueTriggered = false;
            registeredWithDetector = false;
            resolvedAgainstPlayer = false;
            hasLastThreatSnapshot = false;
            lastThreatSnapshot = default;
            resolvedCallback = onResolved;
            resolutionReported = false;
            targetPlayer = player != null ? player : FindFirstObjectByType<PlayerController>();
            targetDodgeDetector = targetPlayer != null ? targetPlayer.GetComponent<JustDodgeDetector>() : null;
            ApplyProfile(profile);
            initialDistanceToPlayerPlane = ResolveDistanceToPlayerPlane();
            if (targetDodgeDetector != null)
            {
                targetDodgeDetector.RegisterIncomingProjectile(this);
                registeredWithDetector = true;
            }
        }

        public bool TryMarkDodged()
        {
            if (wasDodged)
            {
                return false;
            }

            wasDodged = true;
            return true;
        }

        public void ShowImmediateThreatCue(PerspectiveThreatSnapshot snapshot)
        {
            cuePulseRequested = true;
            cuePulseExpiresAt = Time.time + cuePulseDuration;
            BattlePresentationController.Instance?.ShowWorldText(
                snapshot.InterceptPoint + new Vector3(0f, 1.15f, 0f),
                "DODGE",
                Color.Lerp(baseProjectileColor, Color.white, 0.28f),
                3.6f,
                0.42f);
        }

        public void NotifyPerspectiveDodgeSuccess(PerspectiveThreatSnapshot snapshot)
        {
            BattlePresentationController.Instance?.SpawnBurst(
                snapshot.InterceptPoint + Vector3.up * 0.4f,
                new Color(0.42f, 0.95f, 1f, 1f),
                16,
                0.16f,
                2.8f,
                0.08f,
                0.42f);

            if (threatIndicatorMaterial != null)
            {
                threatIndicatorMaterial.color = new Color(0.42f, 0.95f, 1f, 0.72f);
            }
        }

        public bool TryGetPerspectiveThreatSnapshot(PlayerController player, out PerspectiveThreatSnapshot snapshot)
        {
            snapshot = default;
            PlayerController resolvedPlayer = player != null ? player : targetPlayer;
            if (resolvedPlayer == null || Mathf.Abs(moveDirection.z) <= 0.001f || moveSpeed <= 0.001f)
            {
                return false;
            }

            float playerPlaneZ = resolvedPlayer.transform.position.z + impactPlaneForwardOffset;
            float travelDistanceToPlane = (playerPlaneZ - transform.position.z) / moveDirection.z;
            if (travelDistanceToPlane < 0f)
            {
                return false;
            }

            Vector3 interceptPoint = transform.position + (moveDirection * travelDistanceToPlane);
            float timeToPlane = travelDistanceToPlane / moveSpeed;
            float lateralDelta = interceptPoint.x - resolvedPlayer.transform.position.x;
            float normalizedDepthProgress = initialDistanceToPlayerPlane <= 0.001f
                ? 1f
                : Mathf.Clamp01(1f - (travelDistanceToPlane / initialDistanceToPlayerPlane));

            snapshot = new PerspectiveThreatSnapshot(
                timeToPlane,
                travelDistanceToPlane,
                interceptPoint,
                lateralDelta,
                Mathf.Max(0.35f, threatRadius),
                normalizedDepthProgress);
            return true;
        }

        public static int ClearProjectilesInRadius(Vector3 center, float radius)
        {
            float radiusSquared = radius * radius;
            int clearedCount = 0;

            for (int index = ActiveProjectiles.Count - 1; index >= 0; index--)
            {
                EnemyProjectile projectile = ActiveProjectiles[index];
                if (projectile == null)
                {
                    continue;
                }

                if ((projectile.transform.position - center).sqrMagnitude > radiusSquared)
                {
                    continue;
                }

                projectile.DestroyProjectile(spawnImpact: true, new Color(0.45f, 0.95f, 1f, 1f));
                clearedCount++;
            }

            return clearedCount;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (resolvedAgainstPlayer)
            {
                return;
            }

            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null)
            {
                player = other.GetComponentInParent<PlayerController>();
            }

            if (player == null)
            {
                return;
            }

            if (ResolveTravelDistanceToPlayerPlane(player) > 0f)
            {
                return;
            }

            ResolvePlayerImpact(player, transform.position, forceDamageCheck: true);
        }

        private void DestroyProjectile(bool spawnImpact, Color? impactColorOverride = null)
        {
            if (spawnImpact)
            {
                SpawnImpactBurst(transform.position, impactColorOverride ?? new Color(1f, 0.55f, 0.28f, 1f));
            }

            if (threatIndicator != null)
            {
                Object.Destroy(threatIndicator.gameObject);
                threatIndicator = null;
                threatIndicatorRenderer = null;
                threatIndicatorMaterial = null;
            }

            if (threatPathIndicator != null)
            {
                Object.Destroy(threatPathIndicator.gameObject);
                threatPathIndicator = null;
                threatPathIndicatorRenderer = null;
                threatPathIndicatorMaterial = null;
            }

            NotifyResolved();
            Destroy(gameObject);
        }

        private void NotifyResolved()
        {
            if (resolutionReported)
            {
                return;
            }

            resolutionReported = true;
            resolvedCallback?.Invoke(this);
        }

        private static void SpawnImpactBurst(Vector3 position, Color color)
        {
            GameObject effectObject = new("ProjectileImpact");
            effectObject.transform.position = position;

            ParticleSystem particleSystem = effectObject.AddComponent<ParticleSystem>();
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = particleSystem.main;
            main.playOnAwake = false;
            main.duration = 0.25f;
            main.loop = false;
            main.startLifetime = 0.18f;
            main.startSpeed = 2.5f;
            main.startSize = 0.25f;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 14) });

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.08f;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            particleSystem.Play();
            Object.Destroy(effectObject, 0.6f);
        }

        private void EnsureTrail(Color color)
        {
            EnsureTrail(color, 1f);
        }

        private void EnsureTrail(Color color, float widthScale)
        {
            if (projectileTrail == null)
            {
                projectileTrail = GetComponent<TrailRenderer>();
            }

            if (projectileTrail == null)
            {
                projectileTrail = gameObject.AddComponent<TrailRenderer>();
            }

            trailBaseTime = 0.18f;
            trailBaseStartWidth = 0.14f * widthScale;
            trailBaseEndWidth = 0.01f * widthScale;
            projectileTrail.time = trailBaseTime;
            projectileTrail.startWidth = trailBaseStartWidth;
            projectileTrail.endWidth = trailBaseEndWidth;
            projectileTrail.alignment = LineAlignment.View;
            projectileTrail.minVertexDistance = 0.02f;
            projectileTrail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            projectileTrail.receiveShadows = false;
            projectileTrail.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            Gradient gradient = new();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(Color.Lerp(color, Color.white, 0.2f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.9f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            projectileTrail.colorGradient = gradient;
        }

        private void EnsureThreatIndicator()
        {
            if (threatIndicator != null)
            {
                return;
            }

            GameObject indicatorObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            indicatorObject.name = "PerspectiveThreatIndicator";
            Collider indicatorCollider = indicatorObject.GetComponent<Collider>();
            if (indicatorCollider != null)
            {
                Object.Destroy(indicatorCollider);
            }

            threatIndicator = indicatorObject.transform;
            threatIndicatorRenderer = indicatorObject.GetComponent<Renderer>();
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            threatIndicatorMaterial = new Material(shader);
            threatIndicatorMaterial.color = new Color(1f, 0.4f, 0.25f, 0.18f);
            threatIndicatorRenderer.material = threatIndicatorMaterial;
            indicatorObject.SetActive(false);

            GameObject pathObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pathObject.name = "PerspectiveThreatPath";
            Collider pathCollider = pathObject.GetComponent<Collider>();
            if (pathCollider != null)
            {
                Object.Destroy(pathCollider);
            }

            threatPathIndicator = pathObject.transform;
            threatPathIndicatorRenderer = pathObject.GetComponent<Renderer>();
            threatPathIndicatorMaterial = new Material(shader);
            threatPathIndicatorMaterial.color = new Color(1f, 0.45f, 0.28f, 0.16f);
            threatPathIndicatorRenderer.material = threatPathIndicatorMaterial;
            pathObject.SetActive(false);
        }

        private float ResolveDistanceToPlayerPlane()
        {
            if (targetPlayer == null || Mathf.Abs(moveDirection.z) <= 0.001f)
            {
                return 1f;
            }

            float playerPlaneZ = targetPlayer.transform.position.z + impactPlaneForwardOffset;
            float travelDistanceToPlane = (playerPlaneZ - transform.position.z) / moveDirection.z;
            return Mathf.Max(1f, travelDistanceToPlane);
        }

        private void UpdatePerspectivePresentation()
        {
            if (targetPlayer == null)
            {
                targetPlayer = FindFirstObjectByType<PlayerController>();
                targetDodgeDetector = targetPlayer != null ? targetPlayer.GetComponent<JustDodgeDetector>() : null;
                if (targetDodgeDetector != null && !registeredWithDetector)
                {
                    targetDodgeDetector.RegisterIncomingProjectile(this);
                    registeredWithDetector = true;
                }
            }

            if (!TryGetPerspectiveThreatSnapshot(targetPlayer, out PerspectiveThreatSnapshot snapshot))
            {
                if (threatIndicator != null)
                {
                    threatIndicator.gameObject.SetActive(false);
                }

                if (threatPathIndicator != null)
                {
                    threatPathIndicator.gameObject.SetActive(false);
                }

                return;
            }

            float depthScaleFactor = Mathf.SmoothStep(0f, 1f, snapshot.NormalizedDepthProgress);
            UpdateProjectileBodyPresentation(depthScaleFactor);
            hasLastThreatSnapshot = true;
            lastThreatSnapshot = snapshot;
            if (projectileRenderer != null)
            {
                Color nearColor = wasDodged
                    ? new Color(0.42f, 0.95f, 1f, 1f)
                    : Color.Lerp(baseProjectileColor, Color.white, 0.42f);
                projectileRenderer.material.color = Color.Lerp(baseProjectileColor, nearColor, snapshot.NormalizedDepthProgress * 0.82f);
            }

            RefreshCollisionRadius();

            bool shouldShowIndicator = snapshot.TimeToPlane <= telegraphLeadTime && !wasDodged;
            bool shouldShowPathIndicator = snapshot.TimeToPlane <= pathRevealLeadTime && !wasDodged;
            if (threatIndicator == null)
            {
                return;
            }

            if (shouldShowIndicator &&
                !imminentCueTriggered &&
                snapshot.TimeToPlane <= imminentCueLeadTime &&
                snapshot.AbsoluteLateralDelta <= snapshot.ThreatRadius * 1.05f)
            {
                imminentCueTriggered = true;
                cuePulseRequested = true;
                cuePulseExpiresAt = Time.time + (cuePulseDuration * 1.35f);
            }

            threatIndicator.gameObject.SetActive(shouldShowIndicator);
            if (!shouldShowIndicator)
            {
                UpdateThreatPathIndicator(snapshot, shouldShowPathIndicator, depthScaleFactor);
                return;
            }

            threatIndicator.position = new Vector3(snapshot.InterceptPoint.x, 0.05f, snapshot.InterceptPoint.z);
            threatIndicator.rotation = Quaternion.identity;
            threatIndicator.localScale = new Vector3(
                snapshot.ThreatRadius * Mathf.Lerp(2.2f, 4f, depthScaleFactor),
                0.045f,
                Mathf.Lerp(0.7f, 1.35f, depthScaleFactor));

            if (threatIndicatorMaterial != null)
            {
                float pulse = cuePulseRequested && Time.time <= cuePulseExpiresAt
                    ? 0.75f + (Mathf.Sin(Time.unscaledTime * 18f) * 0.2f)
                    : 0.35f + (Mathf.Sin((Time.unscaledTime * 8f) + snapshot.NormalizedDepthProgress) * 0.08f);

                Color color = wasDodged
                    ? new Color(0.42f, 0.95f, 1f, pulse)
                    : Color.Lerp(
                        new Color(baseProjectileColor.r, baseProjectileColor.g, baseProjectileColor.b, pulse),
                        new Color(
                            Mathf.Lerp(baseProjectileColor.r, 1f, 0.24f),
                            Mathf.Lerp(baseProjectileColor.g, 1f, 0.24f),
                            Mathf.Lerp(baseProjectileColor.b, 1f, 0.24f),
                            pulse + 0.08f),
                        1f - Mathf.Clamp01(snapshot.TimeToPlane / telegraphLeadTime));
                threatIndicatorMaterial.color = color;
            }

            UpdateThreatPathIndicator(snapshot, shouldShowPathIndicator, depthScaleFactor);
        }

        private void EnsurePerspectiveBaseTuning()
        {
            bool legacyPrototypeValues =
                Mathf.Abs(perspectiveBaseTuning.impactPlaneForwardOffset - 0.15f) <= 0.001f &&
                Mathf.Abs(perspectiveBaseTuning.telegraphLeadTime - 0.95f) <= 0.001f &&
                Mathf.Abs(perspectiveBaseTuning.pathRevealLeadTime - 0.62f) <= 0.001f &&
                Mathf.Abs(perspectiveBaseTuning.imminentCueLeadTime - 0.26f) <= 0.001f &&
                Mathf.Abs(perspectiveBaseTuning.cuePulseDuration - 0.18f) <= 0.001f &&
                Mathf.Abs(perspectiveBaseTuning.farScaleMultiplier - 0.28f) <= 0.001f &&
                Mathf.Abs(perspectiveBaseTuning.nearScaleMultiplier - 2.35f) <= 0.001f &&
                perspectiveBaseTuning.nearLengthMultiplier <= 0.001f &&
                perspectiveBaseTuning.nearTrailWidthMultiplier <= 0.001f &&
                perspectiveBaseTuning.nearTrailTimeMultiplier <= 0.001f;
            if (legacyPrototypeValues)
            {
                perspectiveBaseTuning.impactPlaneForwardOffset = 0.05f;
                perspectiveBaseTuning.telegraphLeadTime = 0.58f;
                perspectiveBaseTuning.pathRevealLeadTime = 1.08f;
                perspectiveBaseTuning.imminentCueLeadTime = 0.24f;
                perspectiveBaseTuning.cuePulseDuration = 0.18f;
                perspectiveBaseTuning.farScaleMultiplier = 0.38f;
                perspectiveBaseTuning.nearScaleMultiplier = 3.1f;
                perspectiveBaseTuning.nearLengthMultiplier = 1.8f;
                perspectiveBaseTuning.nearTrailWidthMultiplier = 1.7f;
                perspectiveBaseTuning.nearTrailTimeMultiplier = 1.18f;
            }

            if (perspectiveBaseTuning.impactPlaneForwardOffset <= 0.001f)
            {
                perspectiveBaseTuning.impactPlaneForwardOffset = 0.05f;
            }

            if (perspectiveBaseTuning.telegraphLeadTime <= 0.001f)
            {
                perspectiveBaseTuning.telegraphLeadTime = 0.58f;
            }

            if (perspectiveBaseTuning.pathRevealLeadTime <= 0.001f)
            {
                perspectiveBaseTuning.pathRevealLeadTime = 1.08f;
            }

            if (perspectiveBaseTuning.imminentCueLeadTime <= 0.001f)
            {
                perspectiveBaseTuning.imminentCueLeadTime = 0.24f;
            }

            if (perspectiveBaseTuning.cuePulseDuration <= 0.001f)
            {
                perspectiveBaseTuning.cuePulseDuration = 0.18f;
            }

            if (perspectiveBaseTuning.farScaleMultiplier <= 0.001f)
            {
                perspectiveBaseTuning.farScaleMultiplier = 0.38f;
            }

            if (perspectiveBaseTuning.nearScaleMultiplier <= 0.001f)
            {
                perspectiveBaseTuning.nearScaleMultiplier = 3.1f;
            }

            if (perspectiveBaseTuning.nearLengthMultiplier <= 0.001f)
            {
                perspectiveBaseTuning.nearLengthMultiplier = 1.8f;
            }

            if (perspectiveBaseTuning.nearTrailWidthMultiplier <= 0.001f)
            {
                perspectiveBaseTuning.nearTrailWidthMultiplier = 1.7f;
            }

            if (perspectiveBaseTuning.nearTrailTimeMultiplier <= 0.001f)
            {
                perspectiveBaseTuning.nearTrailTimeMultiplier = 1.18f;
            }
        }

        private void ApplyPerspectivePreset(ProjectilePerspectivePreset preset)
        {
            EnsurePerspectiveBaseTuning();
            PerspectivePresetModifiers modifiers = PerspectivePresetTable[(int)preset];
            impactPlaneForwardOffset = perspectiveBaseTuning.impactPlaneForwardOffset;
            telegraphLeadTime = perspectiveBaseTuning.telegraphLeadTime * modifiers.TelegraphLeadScale;
            pathRevealLeadTime = perspectiveBaseTuning.pathRevealLeadTime * modifiers.PathLeadScale;
            imminentCueLeadTime = perspectiveBaseTuning.imminentCueLeadTime * modifiers.ImminentLeadScale;
            cuePulseDuration = perspectiveBaseTuning.cuePulseDuration * modifiers.CuePulseScale;
            farScaleMultiplier = profileFarScaleMultiplier * modifiers.FarScaleScale;
            nearScaleMultiplier = profileNearScaleMultiplier * modifiers.NearScaleScale;
            nearLengthMultiplier = perspectiveBaseTuning.nearLengthMultiplier;
            nearTrailWidthMultiplier = perspectiveBaseTuning.nearTrailWidthMultiplier;
            nearTrailTimeMultiplier = perspectiveBaseTuning.nearTrailTimeMultiplier;
        }

        private void ApplyProfile(ProjectileProfile profile)
        {
            float baseSizeMultiplier = Mathf.Max(0.5f, profile.BaseSizeMultiplier);
            currentThreatWidthMultiplier = Mathf.Max(0.65f, profile.ThreatWidthMultiplier);
            moveSpeed = defaultMoveSpeed * Mathf.Max(0.55f, profile.SpeedMultiplier);
            threatRadius = defaultThreatRadius * currentThreatWidthMultiplier;
            profileFarScaleMultiplier = profile.FarScaleMultiplier > 0f ? profile.FarScaleMultiplier : perspectiveBaseTuning.farScaleMultiplier;
            profileNearScaleMultiplier = profile.NearScaleMultiplier > 0f ? profile.NearScaleMultiplier : perspectiveBaseTuning.nearScaleMultiplier;
            baseScale = prototypeBaseScale * baseSizeMultiplier;
            baseProjectileColor = profile.ProjectileColor;
            ApplyPerspectivePreset(currentPerspectivePreset);

            float trailWidthScale = Mathf.Lerp(0.7f, 1.4f, Mathf.InverseLerp(0.6f, 1.6f, baseSizeMultiplier));
            EnsureTrail(baseProjectileColor, trailWidthScale);
            ApplyVisualTint(baseProjectileColor);
            RefreshCollisionRadius();
        }

        private void ApplyVisualTint(Color color)
        {
            if (projectileRenderer != null && projectileRenderer.material != null && projectileRenderer.material.HasProperty("_Color"))
            {
                projectileRenderer.material.color = color;
            }
        }

        private void UpdateProjectileBodyPresentation(float depthScaleFactor)
        {
            float widthScale = Mathf.Lerp(farScaleMultiplier, nearScaleMultiplier, depthScaleFactor);
            float lengthScale = widthScale * Mathf.Lerp(1.08f, nearLengthMultiplier, depthScaleFactor);
            transform.localScale = new Vector3(
                baseScale.x * widthScale,
                baseScale.y * widthScale,
                baseScale.z * lengthScale);

            if (projectileTrail == null)
            {
                return;
            }

            float trailScale = Mathf.Lerp(1f, nearTrailWidthMultiplier, depthScaleFactor);
            projectileTrail.startWidth = trailBaseStartWidth * trailScale;
            projectileTrail.endWidth = trailBaseEndWidth * Mathf.Lerp(1f, 1.18f, depthScaleFactor);
            projectileTrail.time = trailBaseTime * Mathf.Lerp(1f, nearTrailTimeMultiplier, depthScaleFactor);
        }

        private void RefreshCollisionRadius()
        {
            if (sphereCollider == null)
            {
                return;
            }

            float worldScale = Mathf.Max(transform.lossyScale.x, 0.001f);
            sphereCollider.radius = (baseColliderWorldRadius * currentThreatWidthMultiplier) / worldScale;
        }

        private void UpdateThreatPathIndicator(PerspectiveThreatSnapshot snapshot, bool shouldShowIndicator, float depthScaleFactor)
        {
            if (threatPathIndicator == null)
            {
                return;
            }

            threatPathIndicator.gameObject.SetActive(shouldShowIndicator);
            if (!shouldShowIndicator)
            {
                return;
            }

            float laneSegmentLength = Mathf.Clamp(snapshot.DistanceToPlane, 2.6f, 16f);
            threatPathIndicator.position = new Vector3(
                snapshot.InterceptPoint.x,
                0.035f,
                Mathf.Lerp(transform.position.z, snapshot.InterceptPoint.z, 0.5f));
            threatPathIndicator.rotation = Quaternion.identity;
            threatPathIndicator.localScale = new Vector3(
                snapshot.ThreatRadius * Mathf.Lerp(1.3f, 1.85f, depthScaleFactor),
                0.018f,
                laneSegmentLength);

            if (threatPathIndicatorMaterial != null)
            {
                float pathAlpha = 0.18f + (Mathf.Sin((Time.unscaledTime * 7.5f) + depthScaleFactor) * 0.05f);
                threatPathIndicatorMaterial.color = wasDodged
                    ? new Color(0.42f, 0.95f, 1f, 0.16f)
                    : Color.Lerp(
                        new Color(baseProjectileColor.r, baseProjectileColor.g, baseProjectileColor.b, pathAlpha),
                        new Color(
                            Mathf.Lerp(baseProjectileColor.r, 1f, 0.22f),
                            Mathf.Lerp(baseProjectileColor.g, 1f, 0.22f),
                            Mathf.Lerp(baseProjectileColor.b, 1f, 0.22f),
                            pathAlpha + 0.06f),
                        depthScaleFactor);
            }
        }

        private void EvaluatePlayerImpactResolution()
        {
            if (resolvedAgainstPlayer || targetPlayer == null || Mathf.Abs(moveDirection.z) <= 0.001f)
            {
                return;
            }

            float remainingTravelDistance = ResolveTravelDistanceToPlayerPlane(targetPlayer);
            if (remainingTravelDistance > 0f)
            {
                return;
            }

            Vector3 impactPosition = hasLastThreatSnapshot ? lastThreatSnapshot.InterceptPoint : transform.position;
            ResolvePlayerImpact(targetPlayer, impactPosition, forceDamageCheck: false);
        }

        private void ResolvePlayerImpact(PlayerController player, Vector3 impactPosition, bool forceDamageCheck)
        {
            if (resolvedAgainstPlayer)
            {
                return;
            }

            resolvedAgainstPlayer = true;
            bool shouldDamage = !wasDodged;
            if (shouldDamage && !forceDamageCheck && hasLastThreatSnapshot)
            {
                shouldDamage = lastThreatSnapshot.AbsoluteLateralDelta <= lastThreatSnapshot.ThreatRadius;
            }

            if (shouldDamage)
            {
                player.TakeDamage(damage);
            }

            Color impactColor = shouldDamage
                ? Color.Lerp(baseProjectileColor, Color.white, 0.18f)
                : new Color(0.45f, 0.95f, 1f, 1f);
            transform.position = ResolveVisualImpactPosition(player, impactPosition, shouldDamage);
            DestroyProjectile(spawnImpact: true, impactColor);
        }

        private Vector3 ResolveVisualImpactPosition(PlayerController player, Vector3 fallbackImpactPosition, bool shouldDamage)
        {
            if (!shouldDamage || player == null)
            {
                return transform.position;
            }

            Collider playerCollider = player.GetComponent<Collider>();
            if (playerCollider == null)
            {
                return transform.position;
            }

            Vector3 closestPoint = playerCollider.ClosestPoint(transform.position);
            return (closestPoint - transform.position).sqrMagnitude > 0.0001f
                ? closestPoint
                : fallbackImpactPosition;
        }

        private float ResolveTravelDistanceToPlayerPlane(PlayerController player)
        {
            if (player == null || Mathf.Abs(moveDirection.z) <= 0.001f)
            {
                return float.MaxValue;
            }

            float playerPlaneZ = player.transform.position.z + impactPlaneForwardOffset;
            return (playerPlaneZ - transform.position.z) / moveDirection.z;
        }

        public readonly struct PerspectiveThreatSnapshot
        {
            public PerspectiveThreatSnapshot(float timeToPlane, float distanceToPlane, Vector3 interceptPoint, float lateralDelta, float threatRadius, float normalizedDepthProgress)
            {
                TimeToPlane = timeToPlane;
                DistanceToPlane = distanceToPlane;
                InterceptPoint = interceptPoint;
                LateralDelta = lateralDelta;
                ThreatRadius = threatRadius;
                NormalizedDepthProgress = normalizedDepthProgress;
            }

            public float TimeToPlane { get; }
            public float DistanceToPlane { get; }
            public Vector3 InterceptPoint { get; }
            public float LateralDelta { get; }
            public float ThreatRadius { get; }
            public float NormalizedDepthProgress { get; }
            public float AbsoluteLateralDelta => Mathf.Abs(LateralDelta);
        }

        public readonly struct ProjectileProfile
        {
            public static ProjectileProfile Default => new(1f, 1f, 1f, new Color(1f, 0.45f, 0.22f, 1f), -1f, -1f);

            public ProjectileProfile(
                float baseSizeMultiplier,
                float threatWidthMultiplier,
                float speedMultiplier,
                Color projectileColor,
                float farScaleMultiplier,
                float nearScaleMultiplier)
            {
                BaseSizeMultiplier = baseSizeMultiplier;
                ThreatWidthMultiplier = threatWidthMultiplier;
                SpeedMultiplier = speedMultiplier;
                ProjectileColor = projectileColor;
                FarScaleMultiplier = farScaleMultiplier;
                NearScaleMultiplier = nearScaleMultiplier;
            }

            public float BaseSizeMultiplier { get; }
            public float ThreatWidthMultiplier { get; }
            public float SpeedMultiplier { get; }
            public Color ProjectileColor { get; }
            public float FarScaleMultiplier { get; }
            public float NearScaleMultiplier { get; }
        }
    }

    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyLineProjectile : MonoBehaviour
    {
        private static readonly List<EnemyLineProjectile> ActiveProjectiles = new();

        private Vector3 moveDirection = Vector3.back;
        private float moveSpeed = 13f;
        private float damage = 12f;
        private float baseDamage = 16f;
        private float structureDamageMultiplier = 1.15f;
        private float maxLifetime = 5.5f;
        private float shakeDuration = 0.07f;
        private float shakeMagnitude = 0.08f;
        private float lifetime;
        private bool hasHit;
        private Renderer projectileRenderer;
        private TrailRenderer projectileTrail;
        private SphereCollider sphereCollider;
        private Vector3 baseScale = Vector3.one * 0.24f;
        private float baseColliderWorldRadius = 0.16f;
        private float defaultMoveSpeed;

        private void OnEnable()
        {
            if (!ActiveProjectiles.Contains(this))
            {
                ActiveProjectiles.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveProjectiles.Remove(this);
        }

        private void Awake()
        {
            projectileRenderer = GetComponent<Renderer>();
            projectileTrail = GetComponent<TrailRenderer>();
            sphereCollider = GetComponent<SphereCollider>();

            Rigidbody rigidbodyComponent = GetComponent<Rigidbody>();
            rigidbodyComponent.useGravity = false;
            rigidbodyComponent.isKinematic = true;

            Collider colliderComponent = GetComponent<Collider>();
            colliderComponent.isTrigger = true;

            if (sphereCollider != null)
            {
                float enforcedScale = Mathf.Max(transform.localScale.x, 0.1f);
                baseColliderWorldRadius = sphereCollider.radius * enforcedScale;
            }

            defaultMoveSpeed = moveSpeed;
            ApplyProfile(ProjectileProfile.Default);
        }

        private void Update()
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
            transform.Rotate(Vector3.right, 280f * Time.deltaTime, Space.Self);
            lifetime += Time.deltaTime;

            if (lifetime >= maxLifetime)
            {
                Destroy(gameObject);
            }
        }

        public void Initialize(
            Vector3 direction,
            float projectileDamage,
            float projectileBaseDamage,
            float projectileStructureDamageMultiplier,
            float projectileLifetime,
            ProjectileProfile profile)
        {
            moveDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.back;
            damage = projectileDamage;
            baseDamage = projectileBaseDamage;
            structureDamageMultiplier = projectileStructureDamageMultiplier;
            maxLifetime = projectileLifetime;
            lifetime = 0f;
            hasHit = false;
            ApplyProfile(profile);
        }

        public static int ClearProjectilesInRadius(Vector3 center, float radius)
        {
            float radiusSquared = radius * radius;
            int clearedCount = 0;
            for (int index = ActiveProjectiles.Count - 1; index >= 0; index--)
            {
                EnemyLineProjectile projectile = ActiveProjectiles[index];
                if (projectile == null || projectile.hasHit)
                {
                    continue;
                }

                if ((projectile.transform.position - center).sqrMagnitude > radiusSquared)
                {
                    continue;
                }

                projectile.ResolveImpact(projectile.transform.position, new Color(0.45f, 0.95f, 1f, 1f), playShake: false);
                clearedCount++;
            }

            return clearedCount;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasHit)
            {
                return;
            }

            SummonUnit summonUnit = other.GetComponent<SummonUnit>();
            if (summonUnit == null)
            {
                summonUnit = other.GetComponentInParent<SummonUnit>();
            }

            if (summonUnit != null)
            {
                if (!summonUnit.IsPlayerTeam)
                {
                    return;
                }

                summonUnit.TakeDamage(damage);
                ResolveImpact(transform.position, new Color(1f, 0.55f, 0.28f, 1f), playShake: false);
                return;
            }

            BattleStructure structure = other.GetComponent<BattleStructure>();
            if (structure == null)
            {
                structure = other.GetComponentInParent<BattleStructure>();
            }

            if (structure != null)
            {
                structure.TakeDamage(damage * structureDamageMultiplier, causedByPlayerTeam: false);
                ResolveImpact(transform.position, new Color(1f, 0.74f, 0.36f, 1f), playShake: false);
                return;
            }

            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = other.GetComponentInParent<PlayerController>();
            }

            if (playerController != null)
            {
                playerController.TakeDamage(damage);
                ResolveImpact(transform.position, new Color(1f, 0.48f, 0.34f, 1f), playShake: true);
                return;
            }

            Transform playerBase = BattleManager.Instance != null ? BattleManager.Instance.GetOpposingBaseTransform(isPlayerTeam: false) : null;
            if (playerBase != null && (other.transform == playerBase || other.transform.IsChildOf(playerBase)))
            {
                BattleManager.Instance.DamagePlayerBase(baseDamage);
                ResolveImpact(transform.position, new Color(1f, 0.5f, 0.4f, 1f), playShake: true);
            }
        }

        private void ApplyProfile(ProjectileProfile profile)
        {
            float sizeMultiplier = Mathf.Max(0.65f, profile.SizeMultiplier);
            baseScale = Vector3.one * (0.24f * sizeMultiplier);
            transform.localScale = baseScale;

            if (sphereCollider != null)
            {
                float worldScale = Mathf.Max(transform.lossyScale.x, 0.001f);
                sphereCollider.radius = (baseColliderWorldRadius * Mathf.Max(0.8f, profile.CollisionRadiusMultiplier)) / worldScale;
            }

            moveSpeed = Mathf.Max(6f, defaultMoveSpeed * Mathf.Max(0.6f, profile.SpeedMultiplier));

            EnsureTrail(profile.Color, Mathf.Lerp(0.85f, 1.35f, sizeMultiplier - 0.65f));
            if (projectileRenderer != null && projectileRenderer.material != null && projectileRenderer.material.HasProperty("_Color"))
            {
                projectileRenderer.material.color = profile.Color;
            }
        }

        private void ResolveImpact(Vector3 position, Color color, bool playShake)
        {
            hasHit = true;
            if (playShake)
            {
                CameraShake.Instance?.PlayShake(shakeDuration, shakeMagnitude);
            }

            SpawnImpactBurst(position, color);
            Destroy(gameObject);
        }

        private static void SpawnImpactBurst(Vector3 position, Color color)
        {
            GameObject effectObject = new("EnemyLineProjectileImpact");
            effectObject.transform.position = position;

            ParticleSystem particleSystem = effectObject.AddComponent<ParticleSystem>();
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = particleSystem.main;
            main.playOnAwake = false;
            main.duration = 0.22f;
            main.loop = false;
            main.startLifetime = 0.18f;
            main.startSpeed = 2.6f;
            main.startSize = 0.18f;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 10) });

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.06f;

            particleSystem.Play();
            Object.Destroy(effectObject, 0.45f);
        }

        private void EnsureTrail(Color color, float widthScale)
        {
            if (projectileTrail == null)
            {
                projectileTrail = GetComponent<TrailRenderer>();
            }

            if (projectileTrail == null)
            {
                projectileTrail = gameObject.AddComponent<TrailRenderer>();
            }

            projectileTrail.time = 0.15f;
            projectileTrail.startWidth = 0.1f * widthScale;
            projectileTrail.endWidth = 0.01f * widthScale;
            projectileTrail.alignment = LineAlignment.View;
            projectileTrail.minVertexDistance = 0.02f;
            projectileTrail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            projectileTrail.receiveShadows = false;
            projectileTrail.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            Gradient gradient = new();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(Color.Lerp(color, Color.white, 0.18f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.82f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            projectileTrail.colorGradient = gradient;
        }

        public readonly struct ProjectileProfile
        {
            public static ProjectileProfile Default => new(1f, 1f, 1f, new Color(1f, 0.7f, 0.34f, 1f));

            public ProjectileProfile(float sizeMultiplier, float speedMultiplier, float collisionRadiusMultiplier, Color color)
            {
                SizeMultiplier = sizeMultiplier;
                SpeedMultiplier = speedMultiplier;
                CollisionRadiusMultiplier = collisionRadiusMultiplier;
                Color = color;
            }

            public float SizeMultiplier { get; }
            public float SpeedMultiplier { get; }
            public float CollisionRadiusMultiplier { get; }
            public Color Color { get; }
        }
    }
}
