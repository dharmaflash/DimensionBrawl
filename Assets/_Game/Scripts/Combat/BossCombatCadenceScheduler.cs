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

        public static int RegisteredBasicFireEmitterCount =>
            instance != null ? instance.basicFireEmitters.Count : 0;
        public static int RegisteredActionDirectorCount =>
            instance != null ? instance.actionDirectors.Count : 0;
        public static int RegisteredBarrageEmitterCount =>
            instance != null ? instance.barrageEmitters.Count : 0;
        public static int RegisteredSummonPacingDirectorCount =>
            instance != null ? instance.summonPacingDirectors.Count : 0;
        public static bool IsTicking => instance != null && instance.enabled;

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
            if (instance == this)
            {
                instance = null;
            }
        }

        private void Update()
        {
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
    }
}
