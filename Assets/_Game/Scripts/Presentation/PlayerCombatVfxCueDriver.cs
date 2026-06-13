using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DisallowMultipleComponent]
    public sealed class PlayerCombatVfxCueDriver : MonoBehaviour
    {
        [SerializeField] private PlayerActionController actionController;
        [SerializeField] private CombatVfxCuePlayer cuePlayer;
        [SerializeField] private Transform attackAnchor;
        [SerializeField] private Transform dodgeAnchor;

        private void Awake()
        {
            if (actionController == null)
            {
                actionController = GetComponent<PlayerActionController>();
            }

            if (cuePlayer == null)
            {
                cuePlayer = GetComponent<CombatVfxCuePlayer>();
            }
        }

        private void OnEnable()
        {
            if (actionController == null)
            {
                return;
            }

            actionController.BasicAttackStarted += HandleBasicAttackStarted;
            actionController.BasicAttackHit += HandleBasicAttackHit;
            actionController.DodgeStarted += HandleDodgeStarted;
        }

        private void OnDisable()
        {
            if (actionController == null)
            {
                return;
            }

            actionController.BasicAttackStarted -= HandleBasicAttackStarted;
            actionController.BasicAttackHit -= HandleBasicAttackHit;
            actionController.DodgeStarted -= HandleDodgeStarted;
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

        private void Play(CombatVfxCueId cueId, Transform anchor, Vector3 direction, float intensity)
        {
            if (cuePlayer == null)
            {
                return;
            }

            cuePlayer.PlayCue(cueId, anchor != null ? anchor : transform, direction, intensity);
        }

        private static float ResolveComboIntensity(int comboIndex)
        {
            return Mathf.Lerp(1f, 1.35f, Mathf.Clamp01(comboIndex / 4f));
        }
    }
}
