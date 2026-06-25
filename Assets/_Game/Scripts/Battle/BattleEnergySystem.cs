using System;
using UnityEngine;

namespace IsekaiBrawl.Gameplay
{
    public class BattleEnergySystem : MonoBehaviour
    {
        public static BattleEnergySystem Instance { get; private set; }

        public event Action<float, float> OnEnergyChanged;

        [SerializeField] private float maxEnergy = 130f;
        [SerializeField] private float startingEnergy = 40f;
        [SerializeField] private float baseChargeRate = 2.1f;
        [SerializeField] private bool applyModeTuning = true;

        public float CurrentEnergy { get; private set; }
        public float MaxEnergy => maxEnergy;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ApplyModeDefaults();
            CurrentEnergy = Mathf.Clamp(startingEnergy, 0f, maxEnergy);
        }

        private void Start()
        {
            NotifyEnergyChanged();
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Battle)
            {
                return;
            }

            float chargeRate = baseChargeRate;
            if (chargeRate <= 0f)
            {
                return;
            }

            AddEnergy(chargeRate * Time.deltaTime);
        }

        public void SetForwardMovementActive(bool active)
        {
            // Energy no longer depends on locomotion state.
        }

        public void AddEnergy(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            float previousEnergy = CurrentEnergy;
            CurrentEnergy = Mathf.Clamp(CurrentEnergy + amount, 0f, maxEnergy);
            if (!Mathf.Approximately(previousEnergy, CurrentEnergy))
            {
                NotifyEnergyChanged();
            }
        }

        public bool SpendEnergy(float amount)
        {
            if (amount <= 0f)
            {
                return true;
            }

            if (CurrentEnergy < amount)
            {
                return false;
            }

            CurrentEnergy = Mathf.Clamp(CurrentEnergy - amount, 0f, maxEnergy);
            NotifyEnergyChanged();
            return true;
        }

        private void ApplyModeDefaults()
        {
            if (!applyModeTuning)
            {
                return;
            }

            switch (BattleModeContext.CurrentMode)
            {
                case BattleMode.AsyncPvp:
                    maxEnergy = 138f;
                    startingEnergy = 40f;
                    baseChargeRate = 2.1f;
                    break;
                case BattleMode.Sandbox:
                    maxEnergy = 150f;
                    startingEnergy = 40f;
                    baseChargeRate = 2.1f;
                    break;
                default:
                    maxEnergy = 130f;
                    startingEnergy = 40f;
                    baseChargeRate = 2.1f;
                    break;
            }

            if (BattleModeContext.CurrentMode == BattleMode.StoryPve && PveStageContext.SelectedStage != null)
            {
                if (PveStageContext.SelectedStage.StartingEnergyOverride >= 0f)
                {
                    startingEnergy = PveStageContext.SelectedStage.StartingEnergyOverride;
                }
            }
        }

        private void NotifyEnergyChanged()
        {
            OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
        }
    }
}
