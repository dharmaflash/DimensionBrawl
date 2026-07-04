using System.Collections.Generic;
using DimensionBrawl.Combat;
using DimensionBrawl.Enemies;
using DimensionBrawl.Player;
using UnityEngine;
using UnityEngine.Rendering;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class PerfectDodgeVfxDirector : MonoBehaviour
    {
        private const string WorldFxShaderName = "DimensionBrawl/CombatCues/PerfectDodgeWorldFx";
        private const string AfterimageShaderName = "DimensionBrawl/CombatCues/PerfectDodgeAfterimage";

        private static readonly int ColorAId = Shader.PropertyToID("_ColorA");
        private static readonly int ColorBId = Shader.PropertyToID("_ColorB");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int RimColorId = Shader.PropertyToID("_RimColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int AgeId = Shader.PropertyToID("_Age01");
        private static readonly int PulseId = Shader.PropertyToID("_Pulse");
        private static readonly int LayerModeId = Shader.PropertyToID("_LayerMode");

        [Header("References")]
        [SerializeField] private PlayerActionController actionController;
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private Material worldFxMaterial;
        [SerializeField] private Material afterimageMaterial;
        [SerializeField] private bool playProceduralVisuals;
        [SerializeField] private AudioClip[] timeWarpClips;
        [SerializeField] private AudioClip[] successClips;

        [Header("Timing")]
        [SerializeField, Min(0.05f)] private float domainSeconds = 3f;
        [SerializeField, Min(0.05f)] private float shockwaveSeconds = 0.72f;
        [SerializeField, Min(0.05f)] private float counterWindowSeconds = 1.05f;
        [SerializeField, Min(0.05f)] private float afterimageSeconds = 0.48f;

        [Header("Domain")]
        [SerializeField, Min(0.2f)] private float matrixDomainRadius = 7.2f;
        [SerializeField, Min(0.2f)] private float shockwaveRadius = 14.5f;
        [SerializeField, Range(0f, 3f)] private float worldIntensity = 1.35f;
        [SerializeField] private Color matrixCyan = new Color(0.12f, 0.96f, 1f, 0.82f);
        [SerializeField] private Color matrixViolet = new Color(0.58f, 0.24f, 1f, 0.72f);
        [SerializeField] private Color matrixWhite = new Color(0.92f, 1f, 1f, 0.86f);

        [Header("Afterimage")]
        [SerializeField, Range(0, 8)] private int afterimageCount = 5;
        [SerializeField, Range(1, 32)] private int maxAfterimageRenderers = 18;
        [SerializeField, Min(0f)] private float afterimageSpacing = 0.34f;
        [SerializeField, Range(0f, 1f)] private float afterimageAlpha = 0.42f;

        [Header("Threat Freeze")]
        [SerializeField, Min(0.1f)] private float threatRadius = 42f;
        [SerializeField, Range(0, 16)] private int maxThreatMarkers = 10;
        [SerializeField, Range(0f, 1f)] private float threatMarkerAlpha = 0.52f;

        [Header("Audio")]
        [SerializeField, Range(0f, 1f)] private float timeWarpVolume = 0.38f;
        [SerializeField, Range(0f, 1f)] private float successVolume = 0.46f;
        [SerializeField] private Vector2 timeWarpPitchRange = new Vector2(0.92f, 1.04f);
        [SerializeField] private Vector2 successPitchRange = new Vector2(1.04f, 1.1f);

        private readonly List<Transform> threatTargets = new List<Transform>(16);
        private readonly HashSet<int> threatTargetIds = new HashSet<int>();
        private Material runtimeWorldMaterial;
        private Material runtimeAfterimageMaterial;
        private AudioSource audioSource;
        private int playRequestCount;
        private int lastAfterimageRendererCount;
        private int lastThreatMarkerCount;

        public int PlayRequestCount => playRequestCount;
        public int LastAfterimageRendererCount => lastAfterimageRendererCount;
        public int LastThreatMarkerCount => lastThreatMarkerCount;
        public float DomainSeconds => domainSeconds;
        public float MatrixDomainRadius => matrixDomainRadius;
        public bool PlayProceduralVisuals => playProceduralVisuals;
        public bool HasWorldMaterial => ResolveWorldMaterial() != null;
        public bool HasAfterimageMaterial => ResolveAfterimageMaterial() != null;

        public void Configure(PlayerActionController newActionController, CombatHealth newPlayerHealth)
        {
            actionController = newActionController;
            playerHealth = newPlayerHealth;
        }

        public void ConfigureAudio(AudioClip[] newTimeWarpClips, AudioClip[] newSuccessClips)
        {
            timeWarpClips = newTimeWarpClips;
            successClips = newSuccessClips;
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
        }

        private void OnDestroy()
        {
            DestroyRuntimeMaterial(runtimeWorldMaterial);
            DestroyRuntimeMaterial(runtimeAfterimageMaterial);
        }

        public void Play(DamageInfo damageInfo, Transform anchor, Vector3 dodgeDirection, float intensity, float audioIntensity)
        {
            playRequestCount++;
            PlayRandomAudio(timeWarpClips, timeWarpVolume * audioIntensity, timeWarpPitchRange);
            PlayRandomAudio(successClips, successVolume * audioIntensity, successPitchRange);

            if (!playProceduralVisuals)
            {
                lastAfterimageRendererCount = 0;
                lastThreatMarkerCount = 0;
                return;
            }

            Material worldMaterial = ResolveWorldMaterial();
            Material ghostMaterial = ResolveAfterimageMaterial();
            if (worldMaterial == null || ghostMaterial == null)
            {
                return;
            }

            Transform resolvedAnchor = anchor != null ? anchor : transform;
            Vector3 origin = resolvedAnchor.position;
            Vector3 planarDirection = ResolvePlanarDirection(dodgeDirection, resolvedAnchor);
            float resolvedIntensity = Mathf.Max(0.05f, intensity) * worldIntensity;

            SpawnMatrixDomain(origin, planarDirection, resolvedIntensity, worldMaterial);
            SpawnCounterWindow(resolvedAnchor, planarDirection, resolvedIntensity, worldMaterial);
            lastAfterimageRendererCount = SpawnAfterimages(planarDirection, resolvedIntensity, ghostMaterial);
            lastThreatMarkerCount = SpawnThreatMarkers(damageInfo, origin, resolvedIntensity, worldMaterial);
        }

        private void SpawnMatrixDomain(Vector3 origin, Vector3 direction, float intensity, Material material)
        {
            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

            GameObject shockRoot = new GameObject("PerfectDodge_MatrixShockwave");
            shockRoot.transform.SetPositionAndRotation(origin + Vector3.up * 0.06f, rotation);
            List<Renderer> shockRenderers = new List<Renderer>(10)
            {
                AddMeshRenderer(shockRoot.transform, "OuterShockTorus", CreateRingMesh(144, 0.86f, 1f), material, Vector3.zero, Quaternion.identity, Vector3.one),
                AddMeshRenderer(shockRoot.transform, "RefractedShockTorus", CreateRingMesh(144, 0.58f, 0.64f), material, Vector3.up * 0.025f, Quaternion.Euler(0f, 19f, 0f), Vector3.one),
                AddMeshRenderer(shockRoot.transform, "CountercutSlashA", CreateRibbonMesh(1.92f, 0.035f), material, Vector3.up * 0.04f, Quaternion.Euler(0f, 31f, 0f), Vector3.one),
                AddMeshRenderer(shockRoot.transform, "CountercutSlashB", CreateRibbonMesh(1.68f, 0.028f), material, Vector3.up * 0.052f, Quaternion.Euler(0f, -37f, 0f), Vector3.one)
            };
            AddRadialTickRenderers(shockRoot.transform, material, shockRenderers, 16, 0.92f, 0.09f, 0.022f, 0.072f);
            ConfigureTransient(
                shockRoot,
                shockRenderers,
                matrixWhite,
                matrixCyan,
                shockwaveSeconds,
                new Vector3(1.1f, 1f, 1.1f),
                new Vector3(shockwaveRadius, 1f, shockwaveRadius),
                235f,
                0f,
                0.82f,
                intensity,
                1f,
                1.2f);

            GameObject domainRoot = new GameObject("PerfectDodge_MatrixDomain");
            domainRoot.transform.SetPositionAndRotation(origin + Vector3.up * 0.08f, rotation);
            List<Renderer> domainRenderers = new List<Renderer>(24)
            {
                AddMeshRenderer(domainRoot.transform, "MatrixOuterCircuit", CreateRingMesh(192, 0.92f, 1f), material, Vector3.zero, Quaternion.identity, Vector3.one),
                AddMeshRenderer(domainRoot.transform, "MatrixMidCircuit", CreateRingMesh(192, 0.58f, 0.64f), material, Vector3.up * 0.025f, Quaternion.Euler(0f, 31f, 0f), Vector3.one),
                AddMeshRenderer(domainRoot.transform, "MatrixInnerCircuit", CreateRingMesh(144, 0.28f, 0.34f), material, Vector3.up * 0.042f, Quaternion.Euler(0f, -23f, 0f), Vector3.one),
                AddMeshRenderer(domainRoot.transform, "SpacetimeAxisA", CreateRibbonMesh(1.9f, 0.022f), material, Vector3.up * 0.055f, Quaternion.identity, Vector3.one),
                AddMeshRenderer(domainRoot.transform, "SpacetimeAxisB", CreateRibbonMesh(1.9f, 0.022f), material, Vector3.up * 0.064f, Quaternion.Euler(0f, 90f, 0f), Vector3.one),
                AddMeshRenderer(domainRoot.transform, "FractureDiagonalA", CreateRibbonMesh(1.48f, 0.018f), material, Vector3.up * 0.09f, Quaternion.Euler(0f, 33f, 0f), Vector3.one),
                AddMeshRenderer(domainRoot.transform, "FractureDiagonalB", CreateRibbonMesh(1.36f, 0.014f), material, Vector3.up * 0.10f, Quaternion.Euler(0f, -41f, 0f), Vector3.one)
            };
            AddRadialTickRenderers(domainRoot.transform, material, domainRenderers, 28, 0.78f, 0.055f, 0.014f, 0.06f);
            AddRadialTickRenderers(domainRoot.transform, material, domainRenderers, 12, 0.42f, 0.035f, 0.012f, 0.045f);
            ConfigureTransient(
                domainRoot,
                domainRenderers,
                matrixCyan,
                matrixViolet,
                domainSeconds,
                new Vector3(matrixDomainRadius * 0.46f, 1f, matrixDomainRadius * 0.46f),
                new Vector3(matrixDomainRadius, 1f, matrixDomainRadius),
                46f,
                0.05f,
                0.46f,
                intensity * 0.78f,
                0.55f,
                0.2f);
        }

        private void SpawnCounterWindow(Transform anchor, Vector3 direction, float intensity, Material material)
        {
            GameObject root = new GameObject("PerfectDodge_CounterWindow");
            root.transform.SetParent(anchor, worldPositionStays: false);
            root.transform.localPosition = Vector3.up * 0.88f + Vector3.forward * 0.34f;
            root.transform.localRotation = Quaternion.identity;

            List<Renderer> renderers = new List<Renderer>(8)
            {
                AddMeshRenderer(root.transform, "CounterWindowRing", CreateRingMesh(96, 0.72f, 0.82f), material, Vector3.zero, Quaternion.Euler(90f, 0f, 0f), Vector3.one),
                AddMeshRenderer(root.transform, "CounterWindowInner", CreateRingMesh(96, 0.36f, 0.42f), material, Vector3.forward * 0.018f, Quaternion.Euler(90f, 0f, 0f), Vector3.one),
                AddMeshRenderer(root.transform, "CounterChevronA", CreateChevronMesh(0.54f, 0.16f), material, new Vector3(-0.16f, 0f, 0.025f), Quaternion.Euler(0f, 0f, 0f), Vector3.one),
                AddMeshRenderer(root.transform, "CounterChevronB", CreateChevronMesh(0.54f, 0.16f), material, new Vector3(0.16f, 0f, 0.025f), Quaternion.Euler(0f, 0f, 180f), Vector3.one)
            };

            ConfigureTransient(
                root,
                renderers,
                matrixWhite,
                matrixCyan,
                counterWindowSeconds,
                Vector3.one * 0.82f,
                Vector3.one * 1.26f,
                128f,
                0.05f,
                0.68f,
                intensity,
                1f,
                2f);
        }

        private int SpawnAfterimages(Vector3 dodgeDirection, float intensity, Material material)
        {
            if (afterimageCount <= 0 || material == null)
            {
                return 0;
            }

            Renderer[] sourceRenderers = GetComponentsInChildren<Renderer>(includeInactive: false);
            int copiedRendererCount = 0;
            for (int ghostIndex = 0; ghostIndex < afterimageCount; ghostIndex++)
            {
                float ghost01 = afterimageCount <= 1 ? 1f : ghostIndex / (afterimageCount - 1f);
                Vector3 offset = -dodgeDirection * afterimageSpacing * (ghostIndex + 1) + Vector3.up * (0.015f * ghostIndex);
                GameObject ghostRoot = new GameObject($"PerfectDodge_Afterimage_{ghostIndex + 1:00}");
                List<Renderer> ghostRenderers = new List<Renderer>(sourceRenderers.Length);

                for (int i = 0; i < sourceRenderers.Length; i++)
                {
                    if (copiedRendererCount >= maxAfterimageRenderers || !TryCreateAfterimageRenderer(sourceRenderers[i], ghostRoot.transform, offset, material, out Renderer ghostRenderer))
                    {
                        continue;
                    }

                    ghostRenderers.Add(ghostRenderer);
                    copiedRendererCount++;
                }

                if (ghostRenderers.Count == 0)
                {
                    Destroy(ghostRoot);
                    continue;
                }

                ConfigureTransient(
                    ghostRoot,
                    ghostRenderers,
                    new Color(matrixCyan.r, matrixCyan.g, matrixCyan.b, afterimageAlpha * (1f - ghost01 * 0.18f)),
                    matrixViolet,
                    afterimageSeconds + ghostIndex * 0.045f,
                    Vector3.one,
                    Vector3.one * (1.012f + ghost01 * 0.018f),
                    0f,
                    0.025f + ghost01 * 0.045f,
                    afterimageAlpha * (1f - ghost01 * 0.12f),
                    intensity * (0.74f - ghost01 * 0.08f),
                    0.2f,
                    0f);
            }

            return copiedRendererCount;
        }

        private int SpawnThreatMarkers(DamageInfo damageInfo, Vector3 origin, float intensity, Material material)
        {
            threatTargets.Clear();
            threatTargetIds.Clear();
            DamageTeam playerTeam = playerHealth != null ? playerHealth.Team : DamageTeam.Player;

            if (damageInfo.Source != null && CombatTeamUtility.AreHostile(damageInfo.SourceTeam, playerTeam))
            {
                TryAddThreatTarget(damageInfo.Source.transform, origin);
            }

            CombatHealth[] healths = FindObjectsByType<CombatHealth>(FindObjectsSortMode.None);
            for (int i = 0; i < healths.Length && threatTargets.Count < maxThreatMarkers; i++)
            {
                CombatHealth health = healths[i];
                if (health == null
                    || !health.IsAlive
                    || health == playerHealth
                    || !CombatTeamUtility.AreHostile(health.Team, playerTeam))
                {
                    continue;
                }

                TryAddThreatTarget(health.transform, origin);
            }

            BossBarrageProjectile[] barrageProjectiles = FindObjectsByType<BossBarrageProjectile>(FindObjectsSortMode.None);
            for (int i = 0; i < barrageProjectiles.Length && threatTargets.Count < maxThreatMarkers; i++)
            {
                BossBarrageProjectile projectile = barrageProjectiles[i];
                if (projectile != null
                    && projectile.IsActive
                    && CombatTeamUtility.AreHostile(projectile.SourceTeam, playerTeam))
                {
                    TryAddThreatTarget(projectile.transform, origin);
                }
            }

            LaneActionProjectile[] laneProjectiles = FindObjectsByType<LaneActionProjectile>(FindObjectsSortMode.None);
            for (int i = 0; i < laneProjectiles.Length && threatTargets.Count < maxThreatMarkers; i++)
            {
                LaneActionProjectile projectile = laneProjectiles[i];
                if (projectile != null
                    && projectile.IsActive
                    && CombatTeamUtility.AreHostile(projectile.SourceTeam, playerTeam))
                {
                    TryAddThreatTarget(projectile.transform, origin);
                }
            }

            int markerCount = 0;
            for (int i = 0; i < threatTargets.Count; i++)
            {
                if (SpawnThreatMarker(threatTargets[i], intensity, material))
                {
                    markerCount++;
                }
            }

            return markerCount;
        }

        private bool SpawnThreatMarker(Transform target, float intensity, Material material)
        {
            if (target == null || material == null)
            {
                return false;
            }

            ResolveTargetBounds(target, out float targetRadius, out float targetHeight);
            GameObject root = new GameObject("PerfectDodge_ThreatFreezeMarker");
            root.transform.position = target.position;
            root.transform.rotation = Quaternion.identity;

            List<Renderer> renderers = new List<Renderer>(8)
            {
                AddMeshRenderer(root.transform, "FreezeBaseRing", CreateRingMesh(96, 0.84f, 1f), material, Vector3.up * 0.05f, Quaternion.identity, new Vector3(targetRadius, 1f, targetRadius)),
                AddMeshRenderer(root.transform, "FreezeUpperRing", CreateRingMesh(96, 0.84f, 1f), material, Vector3.up * Mathf.Max(0.72f, targetHeight * 0.78f), Quaternion.identity, new Vector3(targetRadius * 0.72f, 1f, targetRadius * 0.72f))
            };

            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f;
                Vector3 localPosition = Quaternion.Euler(0f, angle, 0f) * (Vector3.forward * targetRadius * 0.86f);
                localPosition.y = targetHeight * 0.42f;
                renderers.Add(AddMeshRenderer(
                    root.transform,
                    $"FreezeVerticalShard_{i:00}",
                    CreateVerticalRibbonMesh(0.035f, Mathf.Max(0.74f, targetHeight * 0.82f)),
                    material,
                    localPosition,
                    Quaternion.Euler(0f, angle, 0f),
                    Vector3.one));
            }

            PerfectDodgeThreatMarker marker = root.AddComponent<PerfectDodgeThreatMarker>();
            marker.Configure(
                target,
                renderers.ToArray(),
                matrixViolet,
                matrixCyan,
                domainSeconds,
                threatMarkerAlpha,
                intensity * 0.58f);
            return true;
        }

        private void TryAddThreatTarget(Transform target, Vector3 origin)
        {
            if (target == null || threatTargets.Count >= maxThreatMarkers)
            {
                return;
            }

            Vector3 planarDelta = Vector3.ProjectOnPlane(target.position - origin, Vector3.up);
            if (planarDelta.sqrMagnitude > threatRadius * threatRadius)
            {
                return;
            }

            int id = target.GetInstanceID();
            if (!threatTargetIds.Add(id))
            {
                return;
            }

            threatTargets.Add(target);
        }

        private bool TryCreateAfterimageRenderer(Renderer source, Transform parent, Vector3 worldOffset, Material material, out Renderer ghostRenderer)
        {
            ghostRenderer = null;
            if (source == null
                || !source.enabled
                || source is ParticleSystemRenderer
                || source is TrailRenderer
                || source is LineRenderer)
            {
                return false;
            }

            Mesh mesh = null;
            if (source is SkinnedMeshRenderer skinnedMeshRenderer && skinnedMeshRenderer.sharedMesh != null)
            {
                mesh = new Mesh
                {
                    name = $"{skinnedMeshRenderer.sharedMesh.name}_PerfectDodgeGhost",
                    hideFlags = HideFlags.HideAndDontSave
                };
                skinnedMeshRenderer.BakeMesh(mesh);
            }
            else if (source is MeshRenderer && source.TryGetComponent(out MeshFilter meshFilter) && meshFilter.sharedMesh != null)
            {
                mesh = Instantiate(meshFilter.sharedMesh);
                mesh.name = $"{meshFilter.sharedMesh.name}_PerfectDodgeGhost";
                mesh.hideFlags = HideFlags.HideAndDontSave;
            }

            if (mesh == null || mesh.vertexCount == 0)
            {
                DestroyGeneratedMesh(mesh);
                return false;
            }

            GameObject ghost = new GameObject(source.name + "_PerfectDodgeGhost");
            ghost.transform.SetParent(parent, worldPositionStays: true);
            ghost.transform.SetPositionAndRotation(source.transform.position + worldOffset, source.transform.rotation);
            ghost.transform.localScale = source.transform.lossyScale;
            ghost.layer = source.gameObject.layer;

            MeshFilter ghostFilter = ghost.AddComponent<MeshFilter>();
            ghostFilter.sharedMesh = mesh;
            MeshRenderer renderer = ghost.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = CreateMaterialArray(material, Mathf.Max(1, mesh.subMeshCount));
            ConfigureRenderer(renderer);
            ghostRenderer = renderer;
            return true;
        }

        private Material ResolveWorldMaterial()
        {
            if (worldFxMaterial != null)
            {
                return worldFxMaterial;
            }

            if (runtimeWorldMaterial != null)
            {
                return runtimeWorldMaterial;
            }

            Shader shader = Shader.Find(WorldFxShaderName);
            if (shader == null)
            {
                return null;
            }

            runtimeWorldMaterial = new Material(shader)
            {
                name = "Runtime_PerfectDodgeWorldFx",
                hideFlags = HideFlags.HideAndDontSave
            };
            return runtimeWorldMaterial;
        }

        private Material ResolveAfterimageMaterial()
        {
            if (afterimageMaterial != null)
            {
                return afterimageMaterial;
            }

            if (runtimeAfterimageMaterial != null)
            {
                return runtimeAfterimageMaterial;
            }

            Shader shader = Shader.Find(AfterimageShaderName);
            if (shader == null)
            {
                return null;
            }

            runtimeAfterimageMaterial = new Material(shader)
            {
                name = "Runtime_PerfectDodgeAfterimage",
                hideFlags = HideFlags.HideAndDontSave
            };
            return runtimeAfterimageMaterial;
        }

        private void PlayRandomAudio(AudioClip[] clips, float volume, Vector2 pitchRange)
        {
            if (clips == null || clips.Length == 0 || volume <= 0f)
            {
                return;
            }

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip == null)
            {
                return;
            }

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.08f;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.minDistance = 3f;
                audioSource.maxDistance = 28f;
                audioSource.priority = 126;
            }

            audioSource.pitch = Random.Range(
                Mathf.Min(pitchRange.x, pitchRange.y),
                Mathf.Max(pitchRange.x, pitchRange.y));
            audioSource.PlayOneShot(clip, volume);
        }

        private static void ConfigureTransient(
            GameObject root,
            List<Renderer> renderers,
            Color colorA,
            Color colorB,
            float lifetime,
            Vector3 startScale,
            Vector3 endScale,
            float spinDegreesPerSecond,
            float verticalLift,
            float alpha,
            float intensity,
            float pulse,
            float layerMode)
        {
            PerfectDodgeTransientVisual visual = root.AddComponent<PerfectDodgeTransientVisual>();
            visual.Configure(
                renderers.ToArray(),
                colorA,
                colorB,
                lifetime,
                startScale,
                endScale,
                spinDegreesPerSecond,
                verticalLift,
                alpha,
                intensity,
                pulse,
                layerMode);
        }

        private static Renderer AddMeshRenderer(
            Transform parent,
            string name,
            Mesh mesh,
            Material material,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale)
        {
            mesh.hideFlags = HideFlags.HideAndDontSave;
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, worldPositionStays: false);
            child.transform.localPosition = localPosition;
            child.transform.localRotation = localRotation;
            child.transform.localScale = localScale;

            MeshFilter filter = child.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            ConfigureRenderer(renderer);
            return renderer;
        }

        private static void AddRadialTickRenderers(
            Transform parent,
            Material material,
            List<Renderer> renderers,
            int count,
            float radius,
            float length,
            float width,
            float height)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = 360f * i / Mathf.Max(1, count);
                Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
                Vector3 position = rotation * (Vector3.forward * radius);
                position.y = height;
                renderers.Add(AddMeshRenderer(
                    parent,
                    $"MatrixTick_{count:00}_{i:00}",
                    CreateRibbonMesh(length, width),
                    material,
                    position,
                    rotation,
                    Vector3.one));
            }
        }

        private static Mesh CreateRingMesh(int segments, float innerRadius, float outerRadius)
        {
            int safeSegments = Mathf.Max(12, segments);
            Vector3[] vertices = new Vector3[(safeSegments + 1) * 2];
            Vector3[] normals = new Vector3[vertices.Length];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] triangles = new int[safeSegments * 6];

            for (int i = 0; i <= safeSegments; i++)
            {
                float angle01 = i / (float)safeSegments;
                float angle = angle01 * Mathf.PI * 2f;
                Vector3 radial = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                int vertexIndex = i * 2;
                vertices[vertexIndex] = radial * innerRadius;
                vertices[vertexIndex + 1] = radial * outerRadius;
                normals[vertexIndex] = Vector3.up;
                normals[vertexIndex + 1] = Vector3.up;
                uvs[vertexIndex] = new Vector2(0f, angle01);
                uvs[vertexIndex + 1] = new Vector2(1f, angle01);
            }

            for (int i = 0; i < safeSegments; i++)
            {
                int vertexIndex = i * 2;
                int triangleIndex = i * 6;
                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = vertexIndex + 1;
                triangles[triangleIndex + 2] = vertexIndex + 2;
                triangles[triangleIndex + 3] = vertexIndex + 1;
                triangles[triangleIndex + 4] = vertexIndex + 3;
                triangles[triangleIndex + 5] = vertexIndex + 2;
            }

            Mesh mesh = new Mesh
            {
                name = "PerfectDodge_RingMesh"
            };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateRibbonMesh(float length, float width)
        {
            float halfLength = length * 0.5f;
            float halfWidth = width * 0.5f;
            Mesh mesh = new Mesh
            {
                name = "PerfectDodge_RibbonMesh"
            };
            mesh.SetVertices(new[]
            {
                new Vector3(-halfLength, 0f, -halfWidth),
                new Vector3(-halfLength, 0f, halfWidth),
                new Vector3(halfLength, 0f, -halfWidth),
                new Vector3(halfLength, 0f, halfWidth)
            });
            mesh.SetNormals(new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up });
            mesh.SetUVs(0, new[] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 0f), new Vector2(1f, 1f) });
            mesh.SetTriangles(new[] { 0, 1, 2, 2, 1, 3 }, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateVerticalRibbonMesh(float width, float height)
        {
            float halfWidth = width * 0.5f;
            Mesh mesh = new Mesh
            {
                name = "PerfectDodge_VerticalRibbonMesh"
            };
            mesh.SetVertices(new[]
            {
                new Vector3(-halfWidth, -height * 0.5f, 0f),
                new Vector3(-halfWidth, height * 0.5f, 0f),
                new Vector3(halfWidth, -height * 0.5f, 0f),
                new Vector3(halfWidth, height * 0.5f, 0f)
            });
            mesh.SetNormals(new[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward });
            mesh.SetUVs(0, new[] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 0f), new Vector2(1f, 1f) });
            mesh.SetTriangles(new[] { 0, 1, 2, 2, 1, 3 }, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateChevronMesh(float width, float height)
        {
            float halfWidth = width * 0.5f;
            Mesh mesh = new Mesh
            {
                name = "PerfectDodge_ChevronMesh"
            };
            mesh.SetVertices(new[]
            {
                new Vector3(-halfWidth, -height * 0.5f, 0f),
                new Vector3(0f, height * 0.5f, 0f),
                new Vector3(halfWidth, -height * 0.5f, 0f),
                new Vector3(0f, height * 0.08f, 0f)
            });
            mesh.SetNormals(new[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward });
            mesh.SetUVs(0, new[] { new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Vector2(1f, 0f), new Vector2(0.5f, 0.4f) });
            mesh.SetTriangles(new[] { 0, 1, 3, 3, 1, 2 }, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void ConfigureRenderer(Renderer renderer)
        {
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.allowOcclusionWhenDynamic = false;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        }

        private static Material[] CreateMaterialArray(Material material, int count)
        {
            Material[] materials = new Material[Mathf.Max(1, count)];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }

            return materials;
        }

        private static Vector3 ResolvePlanarDirection(Vector3 direction, Transform fallback)
        {
            Vector3 planar = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planar.sqrMagnitude > 0.0001f)
            {
                return planar.normalized;
            }

            if (fallback != null)
            {
                planar = Vector3.ProjectOnPlane(fallback.forward, Vector3.up);
                if (planar.sqrMagnitude > 0.0001f)
                {
                    return planar.normalized;
                }
            }

            return Vector3.forward;
        }

        private static void ResolveTargetBounds(Transform target, out float radius, out float height)
        {
            radius = 0.9f;
            height = 1.8f;
            if (target == null)
            {
                return;
            }

            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(includeInactive: false);
            if (renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            radius = Mathf.Clamp(Mathf.Max(bounds.extents.x, bounds.extents.z) * 1.16f, 0.5f, 3.2f);
            height = Mathf.Clamp(bounds.size.y, 0.9f, 4.8f);
        }

        private static void DestroyRuntimeMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(material);
            }
            else
            {
                DestroyImmediate(material);
            }
        }

        private static void DestroyGeneratedMesh(Mesh mesh)
        {
            if (mesh == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(mesh);
            }
            else
            {
                DestroyImmediate(mesh);
            }
        }

        private static void DestroyGeneratedMeshes(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(includeInactive: true);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                Mesh mesh = meshFilters[i] != null ? meshFilters[i].sharedMesh : null;
                if (mesh != null && (mesh.hideFlags & HideFlags.HideAndDontSave) != 0)
                {
                    DestroyGeneratedMesh(mesh);
                }
            }
        }

        private sealed class PerfectDodgeTransientVisual : MonoBehaviour
        {
            private Renderer[] renderers = System.Array.Empty<Renderer>();
            private MaterialPropertyBlock propertyBlock;
            private Color colorA;
            private Color colorB;
            private Vector3 authoredPosition;
            private Vector3 startScale;
            private Vector3 endScale;
            private float lifetime;
            private float elapsed;
            private float spin;
            private float verticalLift;
            private float alpha;
            private float intensity;
            private float pulse;
            private float layerMode;

            public void Configure(
                Renderer[] newRenderers,
                Color newColorA,
                Color newColorB,
                float newLifetime,
                Vector3 newStartScale,
                Vector3 newEndScale,
                float newSpin,
                float newVerticalLift,
                float newAlpha,
                float newIntensity,
                float newPulse,
                float newLayerMode)
            {
                renderers = newRenderers ?? System.Array.Empty<Renderer>();
                colorA = newColorA;
                colorB = newColorB;
                lifetime = Mathf.Max(0.01f, newLifetime);
                startScale = newStartScale;
                endScale = newEndScale;
                spin = newSpin;
                verticalLift = newVerticalLift;
                alpha = Mathf.Clamp01(newAlpha);
                intensity = Mathf.Max(0f, newIntensity);
                pulse = Mathf.Clamp01(newPulse);
                layerMode = newLayerMode;
                authoredPosition = transform.localPosition;
                Apply(0f);
            }

            private void Update()
            {
                elapsed += Time.unscaledDeltaTime > 0f ? Time.unscaledDeltaTime : Time.deltaTime;
                float age01 = Mathf.Clamp01(elapsed / lifetime);
                Apply(age01);
                if (Mathf.Abs(spin) > 0.01f)
                {
                    transform.Rotate(Vector3.up, spin * Time.unscaledDeltaTime, Space.Self);
                }

                if (age01 >= 1f)
                {
                    Destroy(gameObject);
                }
            }

            private void OnDestroy()
            {
                DestroyGeneratedMeshes(gameObject);
            }

            private void Apply(float age01)
            {
                propertyBlock ??= new MaterialPropertyBlock();
                float eased = Mathf.SmoothStep(0f, 1f, age01);
                transform.localScale = Vector3.Lerp(startScale, endScale, eased);
                transform.localPosition = authoredPosition + Vector3.up * verticalLift * eased;

                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer renderer = renderers[i];
                    if (renderer == null)
                    {
                        continue;
                    }

                    renderer.GetPropertyBlock(propertyBlock);
                    propertyBlock.SetColor(ColorAId, colorA);
                    propertyBlock.SetColor(ColorBId, colorB);
                    propertyBlock.SetColor(BaseColorId, colorA);
                    propertyBlock.SetColor(RimColorId, colorB);
                    propertyBlock.SetColor(ColorId, colorA);
                    propertyBlock.SetFloat(AlphaId, alpha);
                    propertyBlock.SetFloat(IntensityId, intensity);
                    propertyBlock.SetFloat(AgeId, age01);
                    propertyBlock.SetFloat(PulseId, pulse);
                    propertyBlock.SetFloat(LayerModeId, layerMode);
                    renderer.SetPropertyBlock(propertyBlock);
                }
            }
        }

        private sealed class PerfectDodgeThreatMarker : MonoBehaviour
        {
            private MaterialPropertyBlock propertyBlock;
            private Transform target;
            private Renderer[] renderers = System.Array.Empty<Renderer>();
            private Color colorA;
            private Color colorB;
            private float lifetime;
            private float elapsed;
            private float alpha;
            private float intensity;

            public void Configure(
                Transform newTarget,
                Renderer[] newRenderers,
                Color newColorA,
                Color newColorB,
                float newLifetime,
                float newAlpha,
                float newIntensity)
            {
                target = newTarget;
                renderers = newRenderers ?? System.Array.Empty<Renderer>();
                colorA = newColorA;
                colorB = newColorB;
                lifetime = Mathf.Max(0.01f, newLifetime);
                alpha = Mathf.Clamp01(newAlpha);
                intensity = Mathf.Max(0f, newIntensity);
                Apply(0f);
            }

            private void Update()
            {
                elapsed += Time.unscaledDeltaTime > 0f ? Time.unscaledDeltaTime : Time.deltaTime;
                if (target != null)
                {
                    transform.position = target.position;
                }

                transform.Rotate(Vector3.up, 42f * Time.unscaledDeltaTime, Space.World);
                float age01 = Mathf.Clamp01(elapsed / lifetime);
                Apply(age01);
                if (age01 >= 1f)
                {
                    Destroy(gameObject);
                }
            }

            private void OnDestroy()
            {
                DestroyGeneratedMeshes(gameObject);
            }

            private void Apply(float age01)
            {
                propertyBlock ??= new MaterialPropertyBlock();
                float life = 1f - age01;
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer renderer = renderers[i];
                    if (renderer == null)
                    {
                        continue;
                    }

                    renderer.GetPropertyBlock(propertyBlock);
                    propertyBlock.SetColor(ColorAId, colorA);
                    propertyBlock.SetColor(ColorBId, colorB);
                    propertyBlock.SetColor(BaseColorId, colorA);
                    propertyBlock.SetColor(RimColorId, colorB);
                    propertyBlock.SetColor(ColorId, colorA);
                    propertyBlock.SetFloat(AlphaId, alpha * Mathf.SmoothStep(0f, 1f, life));
                    propertyBlock.SetFloat(IntensityId, intensity);
                    propertyBlock.SetFloat(AgeId, age01);
                    propertyBlock.SetFloat(PulseId, life);
                    propertyBlock.SetFloat(LayerModeId, 2.6f);
                    renderer.SetPropertyBlock(propertyBlock);
                }
            }
        }
    }
}
