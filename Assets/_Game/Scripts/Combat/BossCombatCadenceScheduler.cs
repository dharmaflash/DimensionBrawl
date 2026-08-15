using System;
using System.Collections.Generic;
using UnityEngine;

namespace DimensionBrawl.Combat
{
    [DisallowMultipleComponent]
    public sealed class BossCombatCadenceScheduler : MonoBehaviour
    {
        private static BossCombatCadenceScheduler instance;

        private readonly List<BossBasicFireEmitter> basicFireEmitters = new(4);
        private readonly List<BossPressureActionDirector> actionDirectors = new(4);
        private readonly List<BossBarrageEmitter> barrageEmitters = new(4);
        private readonly List<EnemySummonPacingDirector> summonPacingDirectors = new(4);
        private readonly Dictionary<int, UnityEngine.Object> externalSuspensionOwners = new(2);
        private int nextExternalSuspensionToken;

        public static int RegisteredBasicFireEmitterCount =>
            instance != null ? instance.basicFireEmitters.Count : 0;
        public static int RegisteredActionDirectorCount =>
            instance != null ? instance.actionDirectors.Count : 0;
        public static int RegisteredBarrageEmitterCount =>
            instance != null ? instance.barrageEmitters.Count : 0;
        public static int RegisteredSummonPacingDirectorCount =>
            instance != null ? instance.summonPacingDirectors.Count : 0;
        public static bool IsTicking => instance != null && instance.enabled;
        public static bool IsExternallySuspended =>
            instance != null && instance.PruneAndCountExternalSuspensions() > 0;
        public static int ExternalSuspensionCount =>
            instance != null ? instance.PruneAndCountExternalSuspensions() : 0;

        public static IDisposable AcquireExternalSuspension(UnityEngine.Object owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            BossCombatCadenceScheduler scheduler = EnsureInstance();
            int token = ++scheduler.nextExternalSuspensionToken;
            scheduler.externalSuspensionOwners.Add(token, owner);
            return new ExternalSuspensionLease(scheduler, token);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
        }

        public static void Register(BossBasicFireEmitter emitter)
        {
            if (emitter == null)
            {
                return;
            }

            BossCombatCadenceScheduler scheduler = EnsureInstance();
            if (!scheduler.basicFireEmitters.Contains(emitter))
            {
                scheduler.basicFireEmitters.Add(emitter);
            }

            scheduler.enabled = true;
        }

        public static void Register(BossPressureActionDirector director)
        {
            if (director == null)
            {
                return;
            }

            BossCombatCadenceScheduler scheduler = EnsureInstance();
            if (!scheduler.actionDirectors.Contains(director))
            {
                scheduler.actionDirectors.Add(director);
            }

            scheduler.enabled = true;
        }

        public static void Register(BossBarrageEmitter emitter)
        {
            if (emitter == null)
            {
                return;
            }

            BossCombatCadenceScheduler scheduler = EnsureInstance();
            if (!scheduler.barrageEmitters.Contains(emitter))
            {
                scheduler.barrageEmitters.Add(emitter);
            }

            scheduler.enabled = true;
        }

        public static void Register(EnemySummonPacingDirector director)
        {
            if (director == null)
            {
                return;
            }

            BossCombatCadenceScheduler scheduler = EnsureInstance();
            if (!scheduler.summonPacingDirectors.Contains(director))
            {
                scheduler.summonPacingDirectors.Add(director);
            }

            scheduler.enabled = true;
        }

        public static void Unregister(BossBasicFireEmitter emitter)
        {
            if (instance == null || emitter == null)
            {
                return;
            }

            instance.basicFireEmitters.Remove(emitter);
            instance.DisableWhenEmpty();
        }

        public static void Unregister(BossPressureActionDirector director)
        {
            if (instance == null || director == null)
            {
                return;
            }

            instance.actionDirectors.Remove(director);
            instance.DisableWhenEmpty();
        }

        public static void Unregister(BossBarrageEmitter emitter)
        {
            if (instance == null || emitter == null)
            {
                return;
            }

            instance.barrageEmitters.Remove(emitter);
            instance.DisableWhenEmpty();
        }

        public static void Unregister(EnemySummonPacingDirector director)
        {
            if (instance == null || director == null)
            {
                return;
            }

            instance.summonPacingDirectors.Remove(director);
            instance.DisableWhenEmpty();
        }

        private static BossCombatCadenceScheduler EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            GameObject root = new GameObject("[BossCombatCadenceScheduler]")
            {
                hideFlags = HideFlags.DontSave
            };
            DontDestroyOnLoad(root);
            instance = root.AddComponent<BossCombatCadenceScheduler>();
            return instance;
        }

        private void OnDestroy()
        {
            externalSuspensionOwners.Clear();
            if (instance == this)
            {
                instance = null;
            }
        }

        private void Update()
        {
            if (PruneAndCountExternalSuspensions() > 0)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
            {
                return;
            }

            // Basic-fire events feed action decisions; queued pressure patterns then advance this frame.
            TickBasicFireEmitters(deltaTime);
            TickActionDirectors(deltaTime);
            TickBarrageEmitters(deltaTime);
            TickSummonPacingDirectors(deltaTime);
            DisableWhenEmpty();
        }

        private void TickBasicFireEmitters(float deltaTime)
        {
            int index = 0;
            while (index < basicFireEmitters.Count)
            {
                BossBasicFireEmitter emitter = basicFireEmitters[index];
                if (emitter == null)
                {
                    basicFireEmitters.RemoveAt(index);
                    continue;
                }

                if (emitter.isActiveAndEnabled)
                {
                    emitter.Tick(deltaTime * CombatTimeDilationReceiver.ResolveTimeScale(emitter));
                }

                index++;
            }
        }

        private void TickActionDirectors(float deltaTime)
        {
            int index = 0;
            while (index < actionDirectors.Count)
            {
                BossPressureActionDirector director = actionDirectors[index];
                if (director == null)
                {
                    actionDirectors.RemoveAt(index);
                    continue;
                }

                if (director.isActiveAndEnabled)
                {
                    director.Tick(deltaTime * CombatTimeDilationReceiver.ResolveTimeScale(director));
                }

                index++;
            }
        }

        private void TickBarrageEmitters(float deltaTime)
        {
            int index = 0;
            while (index < barrageEmitters.Count)
            {
                BossBarrageEmitter emitter = barrageEmitters[index];
                if (emitter == null)
                {
                    barrageEmitters.RemoveAt(index);
                    continue;
                }

                if (emitter.isActiveAndEnabled)
                {
                    emitter.Tick(deltaTime * CombatTimeDilationReceiver.ResolveTimeScale(emitter));
                }

                index++;
            }
        }

        private void TickSummonPacingDirectors(float deltaTime)
        {
            int index = 0;
            while (index < summonPacingDirectors.Count)
            {
                EnemySummonPacingDirector director = summonPacingDirectors[index];
                if (director == null)
                {
                    summonPacingDirectors.RemoveAt(index);
                    continue;
                }

                if (director.isActiveAndEnabled)
                {
                    director.Tick(deltaTime * CombatTimeDilationReceiver.ResolveTimeScale(director));
                }

                index++;
            }
        }

        private void DisableWhenEmpty()
        {
            if (basicFireEmitters.Count > 0
                || actionDirectors.Count > 0
                || barrageEmitters.Count > 0
                || summonPacingDirectors.Count > 0)
            {
                return;
            }

            enabled = false;
        }

        private int PruneAndCountExternalSuspensions()
        {
            if (externalSuspensionOwners.Count == 0)
            {
                return 0;
            }

            List<int> staleTokens = null;
            foreach (KeyValuePair<int, UnityEngine.Object> pair in externalSuspensionOwners)
            {
                if (pair.Value != null)
                {
                    continue;
                }

                staleTokens ??= new List<int>();
                staleTokens.Add(pair.Key);
            }

            if (staleTokens != null)
            {
                for (int index = 0; index < staleTokens.Count; index++)
                {
                    externalSuspensionOwners.Remove(staleTokens[index]);
                }
            }

            return externalSuspensionOwners.Count;
        }

        private sealed class ExternalSuspensionLease : IDisposable
        {
            private BossCombatCadenceScheduler scheduler;
            private readonly int token;

            public ExternalSuspensionLease(
                BossCombatCadenceScheduler scheduler,
                int token)
            {
                this.scheduler = scheduler;
                this.token = token;
            }

            public void Dispose()
            {
                if (scheduler != null)
                {
                    scheduler.externalSuspensionOwners.Remove(token);
                    scheduler = null;
                }
            }
        }
    }
}
