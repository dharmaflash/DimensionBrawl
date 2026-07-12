using System.Collections.Generic;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    [DisallowMultipleComponent]
    public sealed class CombatResourceTickScheduler : MonoBehaviour
    {
        private const float TickRateHz = 30f;
        private const float TickIntervalSeconds = 1f / TickRateHz;

        private static CombatResourceTickScheduler instance;

        private readonly List<SummonEnergyLadder> energyLadders = new(4);
        private readonly List<BossPressureCostLadder> bossCostLadders = new(4);
        private float accumulatedDeltaTime;

        public static int RegisteredEnergyLadderCount =>
            instance != null ? instance.energyLadders.Count : 0;
        public static int RegisteredBossCostLadderCount =>
            instance != null ? instance.bossCostLadders.Count : 0;
        public static bool IsTicking => instance != null && instance.enabled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
        }

        public static void Register(SummonEnergyLadder ladder)
        {
            if (ladder == null)
            {
                return;
            }

            CombatResourceTickScheduler scheduler = EnsureInstance();
            if (!scheduler.energyLadders.Contains(ladder))
            {
                scheduler.energyLadders.Add(ladder);
            }

            scheduler.enabled = true;
        }

        public static void Register(BossPressureCostLadder ladder)
        {
            if (ladder == null)
            {
                return;
            }

            CombatResourceTickScheduler scheduler = EnsureInstance();
            if (!scheduler.bossCostLadders.Contains(ladder))
            {
                scheduler.bossCostLadders.Add(ladder);
            }

            scheduler.enabled = true;
        }

        public static void Unregister(SummonEnergyLadder ladder)
        {
            if (instance == null || ladder == null)
            {
                return;
            }

            instance.energyLadders.Remove(ladder);
            instance.DisableWhenEmpty();
        }

        public static void Unregister(BossPressureCostLadder ladder)
        {
            if (instance == null || ladder == null)
            {
                return;
            }

            instance.bossCostLadders.Remove(ladder);
            instance.DisableWhenEmpty();
        }

        private static CombatResourceTickScheduler EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            GameObject root = new GameObject("[CombatResourceTickScheduler]");
            DontDestroyOnLoad(root);
            instance = root.AddComponent<CombatResourceTickScheduler>();
            return instance;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
            {
                return;
            }

            accumulatedDeltaTime += deltaTime;
            if (accumulatedDeltaTime + 0.000001f < TickIntervalSeconds)
            {
                return;
            }

            float tickDeltaTime = accumulatedDeltaTime;
            accumulatedDeltaTime = 0f;
            TickEnergyLadders(tickDeltaTime);
            TickBossCostLadders(tickDeltaTime);
            DisableWhenEmpty();
        }

        private void TickEnergyLadders(float deltaTime)
        {
            int index = 0;
            while (index < energyLadders.Count)
            {
                SummonEnergyLadder ladder = energyLadders[index];
                if (ladder == null)
                {
                    energyLadders.RemoveAt(index);
                    continue;
                }

                if (ladder.isActiveAndEnabled)
                {
                    ladder.Tick(deltaTime);
                }

                index++;
            }
        }

        private void TickBossCostLadders(float deltaTime)
        {
            int index = 0;
            while (index < bossCostLadders.Count)
            {
                BossPressureCostLadder ladder = bossCostLadders[index];
                if (ladder == null)
                {
                    bossCostLadders.RemoveAt(index);
                    continue;
                }

                if (ladder.isActiveAndEnabled)
                {
                    float timeScale = CombatTimeDilationReceiver.ResolveTimeScale(ladder);
                    ladder.Tick(deltaTime * timeScale);
                }

                index++;
            }
        }

        private void DisableWhenEmpty()
        {
            if (energyLadders.Count > 0 || bossCostLadders.Count > 0)
            {
                return;
            }

            accumulatedDeltaTime = 0f;
            enabled = false;
        }
    }
}
