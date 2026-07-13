using UnityEngine;
using UnityEngine.Events;

namespace DimensionBrawl.Combat
{
    public sealed class CombatEncounterController : MonoBehaviour
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

        public void ConfigureCombatants(CombatHealth newPlayerHealth, CombatHealth newEnemyHealth)
        {
            bool wasActive = isActiveAndEnabled;
            if (wasActive)
            {
                UnsubscribeHealthEvents();
            }

            playerHealth = newPlayerHealth;
            enemyHealth = newEnemyHealth;

            if (wasActive)
            {
                SubscribeHealthEvents();
            }

            SetMarkers();
        }

        private void OnEnable()
        {
            SubscribeHealthEvents();
            SetMarkers();
        }

        private void OnDisable()
        {
            UnsubscribeHealthEvents();
        }

        private void SubscribeHealthEvents()
        {
            if (playerHealth != null)
            {
                playerHealth.Died += HandlePlayerDied;
            }

            if (enemyHealth != null)
            {
                enemyHealth.Died += HandleEnemyDied;
            }
        }

        private void UnsubscribeHealthEvents()
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
