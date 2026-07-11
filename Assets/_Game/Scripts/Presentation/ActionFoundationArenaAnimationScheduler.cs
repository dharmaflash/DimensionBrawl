using System.Collections.Generic;
using UnityEngine;

namespace DimensionBrawl.Presentation
{
    [DefaultExecutionOrder(10000)]
    public sealed class ActionFoundationArenaAnimationScheduler : MonoBehaviour
    {
        private static readonly List<ActionFoundationArenaTransformMotion> TransformMotions = new();
        private static readonly List<ActionFoundationArenaFloatingShape> FloatingShapes = new();
        private static readonly List<ActionFoundationArenaShapeInfluenceDriver> InfluenceDrivers = new();

        private static ActionFoundationArenaAnimationScheduler instance;

        public static int RegisteredTransformMotionCount => TransformMotions.Count;
        public static int RegisteredFloatingShapeCount => FloatingShapes.Count;
        public static int RegisteredInfluenceDriverCount => InfluenceDrivers.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            TransformMotions.Clear();
            FloatingShapes.Clear();
            InfluenceDrivers.Clear();
            instance = null;
        }

        internal static void Register(ActionFoundationArenaTransformMotion motion)
        {
            Register(TransformMotions, motion);
        }

        internal static void Unregister(ActionFoundationArenaTransformMotion motion)
        {
            TransformMotions.Remove(motion);
            RefreshEnabledState();
        }

        internal static void Register(ActionFoundationArenaFloatingShape shape)
        {
            Register(FloatingShapes, shape);
        }

        internal static void Unregister(ActionFoundationArenaFloatingShape shape)
        {
            FloatingShapes.Remove(shape);
            RefreshEnabledState();
        }

        internal static void Register(ActionFoundationArenaShapeInfluenceDriver driver)
        {
            Register(InfluenceDrivers, driver);
        }

        internal static void Unregister(ActionFoundationArenaShapeInfluenceDriver driver)
        {
            InfluenceDrivers.Remove(driver);
            RefreshEnabledState();
        }

        private static void Register<T>(List<T> targets, T target)
            where T : MonoBehaviour
        {
            if (!Application.isPlaying || target == null)
            {
                return;
            }

            if (!targets.Contains(target))
            {
                targets.Add(target);
            }

            EnsureInstance();
            instance.enabled = true;
        }

        private static void EnsureInstance()
        {
            if (instance != null)
            {
                return;
            }

            var schedulerObject = new GameObject("Runtime_ActionFoundationArenaAnimationScheduler")
            {
                hideFlags = HideFlags.DontSave
            };
            DontDestroyOnLoad(schedulerObject);
            instance = schedulerObject.AddComponent<ActionFoundationArenaAnimationScheduler>();
        }

        private void LateUpdate()
        {
            float time = Time.time;
            float deltaTime = Time.deltaTime;

            for (int i = TransformMotions.Count - 1; i >= 0; i--)
            {
                ActionFoundationArenaTransformMotion motion = TransformMotions[i];
                if (motion == null || !motion.isActiveAndEnabled)
                {
                    TransformMotions.RemoveAt(i);
                    continue;
                }

                motion.TickScheduled(time, deltaTime);
            }

            for (int i = FloatingShapes.Count - 1; i >= 0; i--)
            {
                ActionFoundationArenaFloatingShape shape = FloatingShapes[i];
                if (shape == null || !shape.isActiveAndEnabled)
                {
                    FloatingShapes.RemoveAt(i);
                    continue;
                }

                shape.TickScheduled(time, deltaTime);
            }

            for (int i = InfluenceDrivers.Count - 1; i >= 0; i--)
            {
                ActionFoundationArenaShapeInfluenceDriver driver = InfluenceDrivers[i];
                if (driver == null || !driver.isActiveAndEnabled)
                {
                    InfluenceDrivers.RemoveAt(i);
                    continue;
                }

                driver.TickScheduled();
            }

            RefreshEnabledState();
        }

        private static void RefreshEnabledState()
        {
            if (instance != null)
            {
                instance.enabled = TransformMotions.Count > 0
                    || FloatingShapes.Count > 0
                    || InfluenceDrivers.Count > 0;
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
