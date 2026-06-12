using DimensionBrawl.Combat;
using UnityEngine;
using UnityEngine.Events;

namespace DimensionBrawl.Test
{
    public sealed class ActionFoundationTestEncounter : MonoBehaviour
    {
        private enum EncounterState
        {
            Running,
            Won,
            Failed
        }

        [Header("Combatants")]
        [SerializeField] private CombatHealth playerHealth;
        [SerializeField] private CombatHealth enemyHealth;

        [Header("Inspectable Result Markers")]
        [SerializeField] private GameObject winMarker;
        [SerializeField] private GameObject failMarker;
        [SerializeField] private UnityEvent onWon = new UnityEvent();
        [SerializeField] private UnityEvent onFailed = new UnityEvent();

        private EncounterState state;

        public bool IsRunning => state == EncounterState.Running;
        public bool IsWon => state == EncounterState.Won;
        public bool IsFailed => state == EncounterState.Failed;

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.Died += HandlePlayerDied;
            }

            if (enemyHealth != null)
            {
                enemyHealth.Died += HandleEnemyDied;
            }

            SetMarkers();
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.Died -= HandlePlayerDied;
            }

            if (enemyHealth != null)
            {
                enemyHealth.Died -= HandleEnemyDied;
            }
        }

        private void HandleEnemyDied()
        {
            if (state != EncounterState.Running)
            {
                return;
            }

            state = EncounterState.Won;
            SetMarkers();
            onWon.Invoke();
        }

        private void HandlePlayerDied()
        {
            if (state != EncounterState.Running)
            {
                return;
            }

            state = EncounterState.Failed;
            SetMarkers();
            onFailed.Invoke();
        }

        private void SetMarkers()
        {
            if (winMarker != null)
            {
                winMarker.SetActive(state == EncounterState.Won);
            }

            if (failMarker != null)
            {
                failMarker.SetActive(state == EncounterState.Failed);
            }
        }
    }
}
