using System;
using System.Collections.Generic;
using UnityEngine;

namespace IsekaiBrawl.Gameplay
{
    [RequireComponent(typeof(PlayerController))]
    public class JustDodgeDetector : MonoBehaviour
    {
        public event Action OnJustDodge;

        [SerializeField] private float threatArmingLeadTime = 0.7f;
        [SerializeField] private float dodgeCueLeadTime = 0.4f;
        [SerializeField] private float justDodgeWindowStart = 0.24f;
        [SerializeField] private float justDodgeWindowEnd = 0.045f;
        [SerializeField] private float requiredHorizontalDisplacement = 0.7f;
        [SerializeField] private float recentInputGraceWindow = 0.12f;
        [SerializeField] private float minimumHorizontalMotion = 0.035f;
        [SerializeField] private float minimumHorizontalInput = 0.18f;

        private readonly Dictionary<EnemyProjectile, TrackedThreat> activeThreats = new();
        private PlayerController playerController;
        private float previousPlayerX;
        private float lastHorizontalMovementTime;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            previousPlayerX = transform.position.x;
        }

        private void Update()
        {
            if (playerController == null)
            {
                return;
            }

            TrackHorizontalMovement();
            CleanupResolvedThreats();

            if (activeThreats.Count == 0)
            {
                return;
            }

            EnemyProjectile[] trackedProjectiles = new EnemyProjectile[activeThreats.Count];
            activeThreats.Keys.CopyTo(trackedProjectiles, 0);
            for (int index = 0; index < trackedProjectiles.Length; index++)
            {
                EnemyProjectile projectile = trackedProjectiles[index];
                if (projectile == null)
                {
                    continue;
                }

                if (!activeThreats.TryGetValue(projectile, out TrackedThreat threat))
                {
                    continue;
                }

                if (!projectile.TryGetPerspectiveThreatSnapshot(playerController, out EnemyProjectile.PerspectiveThreatSnapshot snapshot))
                {
                    activeThreats.Remove(projectile);
                    continue;
                }

                bool isAligned = snapshot.AbsoluteLateralDelta <= snapshot.ThreatRadius;
                if (isAligned && snapshot.TimeToPlane <= threatArmingLeadTime && !threat.IsArmed)
                {
                    threat.IsArmed = true;
                    threat.ArmedPlayerX = transform.position.x;
                }

                if (threat.IsArmed && !threat.CueShown && snapshot.TimeToPlane <= dodgeCueLeadTime)
                {
                    threat.CueShown = true;
                    projectile.ShowImmediateThreatCue(snapshot);
                }

                bool isWithinJustWindow =
                    snapshot.TimeToPlane <= justDodgeWindowStart &&
                    snapshot.TimeToPlane >= justDodgeWindowEnd;

                if (threat.IsArmed && isWithinJustWindow && !isAligned)
                {
                    float horizontalDisplacement = Mathf.Abs(transform.position.x - threat.ArmedPlayerX);
                    bool movedRecently = Time.time - Mathf.Max(lastHorizontalMovementTime, threat.EscapeStartedAt) <= recentInputGraceWindow;
                    bool hasHorizontalIntent = Mathf.Abs(playerController.CurrentMoveInput.x) >= minimumHorizontalInput || movedRecently;

                    if (horizontalDisplacement >= requiredHorizontalDisplacement && hasHorizontalIntent && projectile.TryMarkDodged())
                    {
                        activeThreats.Remove(projectile);
                        projectile.NotifyPerspectiveDodgeSuccess(snapshot);
                        OnJustDodge?.Invoke();
                        return;
                    }
                }

                if (threat.IsArmed)
                {
                    if (isAligned)
                    {
                        threat.WasAlignedLastFrame = true;
                    }
                    else if (threat.WasAlignedLastFrame)
                    {
                        threat.WasAlignedLastFrame = false;
                        threat.EscapeStartedAt = Time.time;
                    }
                }

                activeThreats[projectile] = threat;
            }
        }

        public void RegisterIncomingProjectile(EnemyProjectile projectile)
        {
            if (projectile == null)
            {
                return;
            }

            activeThreats[projectile] = new TrackedThreat();
        }

        private void TrackHorizontalMovement()
        {
            float currentPlayerX = transform.position.x;
            float deltaX = Mathf.Abs(currentPlayerX - previousPlayerX);
            if (deltaX >= minimumHorizontalMotion)
            {
                lastHorizontalMovementTime = Time.time;
            }

            previousPlayerX = currentPlayerX;
        }

        private void CleanupResolvedThreats()
        {
            if (activeThreats.Count == 0)
            {
                return;
            }

            List<EnemyProjectile> resolvedProjectiles = null;
            foreach (KeyValuePair<EnemyProjectile, TrackedThreat> pair in activeThreats)
            {
                if (pair.Key != null && !pair.Key.WasDodged)
                {
                    continue;
                }

                resolvedProjectiles ??= new List<EnemyProjectile>();
                resolvedProjectiles.Add(pair.Key);
            }

            if (resolvedProjectiles == null)
            {
                return;
            }

            foreach (EnemyProjectile projectile in resolvedProjectiles)
            {
                activeThreats.Remove(projectile);
            }
        }

        private struct TrackedThreat
        {
            public bool IsArmed;
            public bool CueShown;
            public bool WasAlignedLastFrame;
            public float ArmedPlayerX;
            public float EscapeStartedAt;
        }
    }
}
