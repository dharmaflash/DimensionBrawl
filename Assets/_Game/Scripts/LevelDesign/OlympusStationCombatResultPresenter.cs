using DimensionBrawl.Combat;
using DimensionBrawl.UI;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    [DisallowMultipleComponent]
    public sealed class OlympusStationCombatResultPresenter : MonoBehaviour
    {
        [SerializeField] private CombatEncounterController encounter;
        [SerializeField] private OlympusStageClearOverlay stageClearOverlay;
        [SerializeField] private MonoBehaviour resultSurfaceBehaviour;

        private CombatEncounterController subscribedEncounter;
        private ICombatSessionOverlay ResultSurface => resultSurfaceBehaviour as ICombatSessionOverlay;

        private void OnEnable()
        {
            SubscribeEncounter();
        }

        private void OnDisable()
        {
            UnsubscribeEncounter();
        }

        private void SubscribeEncounter()
        {
            if (subscribedEncounter == encounter)
            {
                return;
            }

            UnsubscribeEncounter();
            if (encounter == null || stageClearOverlay == null || ResultSurface == null)
            {
                Debug.LogError(
                    $"[{nameof(OlympusStationCombatResultPresenter)}] Missing authored encounter, result surface, or stage-clear overlay.",
                    this);
                return;
            }

            subscribedEncounter = encounter;
            subscribedEncounter.Won += HandleEncounterWon;
            subscribedEncounter.Failed += HandleEncounterFailed;
            if (subscribedEncounter.IsWon)
            {
                HandleEncounterWon();
            }
            else if (subscribedEncounter.IsFailed)
            {
                HandleEncounterFailed();
            }
        }

        private void UnsubscribeEncounter()
        {
            if (subscribedEncounter == null)
            {
                return;
            }

            subscribedEncounter.Won -= HandleEncounterWon;
            subscribedEncounter.Failed -= HandleEncounterFailed;
            subscribedEncounter = null;
        }

        private void HandleEncounterWon()
        {
            ResultSurface?.DismissForStageClear();
            stageClearOverlay.Show();
        }

        private void HandleEncounterFailed()
        {
            ResultSurface?.ShowFailure();
        }
    }
}
