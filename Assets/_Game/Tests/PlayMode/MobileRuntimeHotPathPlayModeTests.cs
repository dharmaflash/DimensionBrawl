using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;
using IsekaiBrawl.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DimensionBrawl.Tests
{
    public sealed class MobileRuntimeHotPathPlayModeTests
    {
        [UnityTest]
        public IEnumerator ArenaDecorativeAnimationsShareOneLateUpdateScheduler()
        {
            int initialMotionCount =
                ActionFoundationArenaAnimationScheduler.RegisteredTransformMotionCount;
            int initialShapeCount =
                ActionFoundationArenaAnimationScheduler.RegisteredFloatingShapeCount;
            int initialInfluenceDriverCount =
                ActionFoundationArenaAnimationScheduler.RegisteredInfluenceDriverCount;
            GameObject root = new GameObject("ArenaAnimationSchedulerTest");
            var motions = new List<ActionFoundationArenaTransformMotion>();
            var shapes = new List<ActionFoundationArenaFloatingShape>();
            for (int i = 0; i < 8; i++)
            {
                GameObject motionObject = new GameObject($"Motion_{i}");
                motionObject.transform.SetParent(root.transform, worldPositionStays: false);
                ActionFoundationArenaTransformMotion motion =
                    motionObject.AddComponent<ActionFoundationArenaTransformMotion>();
                motion.Configure(
                    new Vector3(0f, 90f, 0f),
                    Vector3.up,
                    0.2f,
                    1f,
                    i * 0.1f);
                motions.Add(motion);

                GameObject shapeObject = new GameObject($"Shape_{i}");
                shapeObject.transform.SetParent(root.transform, worldPositionStays: false);
                ActionFoundationArenaFloatingShape shape =
                    shapeObject.AddComponent<ActionFoundationArenaFloatingShape>();
                shape.Configure(
                    new Vector3(30f, 0f, 0f),
                    Vector3.up,
                    0.2f,
                    1f,
                    i * 0.1f,
                    Color.white,
                    Color.white,
                    0f,
                    0f);
                shapes.Add(shape);
            }

            GameObject influenceObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            influenceObject.name = "InfluenceRenderer";
            influenceObject.transform.SetParent(root.transform, worldPositionStays: false);
            Renderer influenceRenderer = influenceObject.GetComponent<Renderer>();
            GameObject influenceTargetObject = new GameObject("InfluenceTarget");
            influenceTargetObject.transform.SetParent(root.transform, worldPositionStays: false);
            influenceTargetObject.transform.position = new Vector3(1f, 2f, 3f);
            ActionFoundationArenaShapeInfluenceDriver influenceDriver =
                influenceObject.AddComponent<ActionFoundationArenaShapeInfluenceDriver>();
            influenceDriver.Configure(
                new[] { influenceRenderer },
                new[] { influenceTargetObject.transform });

            try
            {
                yield return null;
                Vector3 initialMotionPosition = motions[0].transform.localPosition;
                Quaternion initialMotionRotation = motions[0].transform.localRotation;
                Vector3 initialShapePosition = shapes[0].transform.localPosition;
                yield return null;

                Assert.That(
                    Resources.FindObjectsOfTypeAll<ActionFoundationArenaAnimationScheduler>().Length,
                    Is.EqualTo(1));
                Assert.That(
                    ActionFoundationArenaAnimationScheduler.RegisteredTransformMotionCount,
                    Is.EqualTo(initialMotionCount + 8));
                Assert.That(
                    ActionFoundationArenaAnimationScheduler.RegisteredFloatingShapeCount,
                    Is.EqualTo(initialShapeCount + 8));
                Assert.That(
                    ActionFoundationArenaAnimationScheduler.RegisteredInfluenceDriverCount,
                    Is.EqualTo(initialInfluenceDriverCount + 1));
                Assert.That(motions[0].transform.localPosition, Is.Not.EqualTo(initialMotionPosition));
                Assert.That(motions[0].transform.localRotation, Is.Not.EqualTo(initialMotionRotation));
                Assert.That(shapes[0].transform.localPosition, Is.Not.EqualTo(initialShapePosition));

                influenceTargetObject.transform.position = new Vector3(4f, 5f, 6f);
                yield return null;
                var propertyBlock = new MaterialPropertyBlock();
                influenceRenderer.GetPropertyBlock(propertyBlock);
                Assert.That(propertyBlock.GetFloat("_Activate_Target"), Is.EqualTo(1f));
                Assert.That(
                    propertyBlock.GetVector("_target"),
                    Is.EqualTo(new Vector4(4f, 5f, 6f, 0f)));
                Assert.That(
                    typeof(ActionFoundationArenaTransformMotion).GetMethod(
                        "LateUpdate",
                        BindingFlags.Instance | BindingFlags.NonPublic),
                    Is.Null);
                Assert.That(
                    typeof(ActionFoundationArenaFloatingShape).GetMethod(
                        "LateUpdate",
                        BindingFlags.Instance | BindingFlags.NonPublic),
                    Is.Null);
                Assert.That(
                    typeof(ActionFoundationArenaShapeInfluenceDriver).GetMethod(
                        "LateUpdate",
                        BindingFlags.Instance | BindingFlags.NonPublic),
                    Is.Null);
            }
            finally
            {
                Object.Destroy(root);
            }

            yield return null;
            Assert.That(
                ActionFoundationArenaAnimationScheduler.RegisteredTransformMotionCount,
                Is.EqualTo(initialMotionCount));
            Assert.That(
                ActionFoundationArenaAnimationScheduler.RegisteredFloatingShapeCount,
                Is.EqualTo(initialShapeCount));
            Assert.That(
                ActionFoundationArenaAnimationScheduler.RegisteredInfluenceDriverCount,
                Is.EqualTo(initialInfluenceDriverCount));
        }

        [UnityTest]
        public IEnumerator CombatHealthRegistryTracksActiveLifecycle()
        {
            GameObject root = new GameObject("CombatHealthRegistryTest");
            CombatHealth health = root.AddComponent<CombatHealth>();

            try
            {
                yield return null;
                Assert.IsTrue(Contains(CombatHealth.ActiveInstances, health));

                root.SetActive(false);
                Assert.IsFalse(Contains(CombatHealth.ActiveInstances, health));

                root.SetActive(true);
                Assert.IsTrue(Contains(CombatHealth.ActiveInstances, health));
            }
            finally
            {
                Object.Destroy(root);
            }

            yield return null;
            Assert.IsFalse(Contains(CombatHealth.ActiveInstances, health));
        }

        [UnityTest]
        public IEnumerator CombatHealthColliderBindingCacheReusesAndRefreshesResolvedHealth()
        {
            GameObject root = new GameObject("ColliderHealthCacheRoot");
            GameObject hitbox = new GameObject("ColliderHealthCacheHitbox");
            hitbox.transform.SetParent(root.transform, worldPositionStays: false);
            SphereCollider collider = hitbox.AddComponent<SphereCollider>();
            CombatHealth health = root.AddComponent<CombatHealth>();

            try
            {
                yield return null;

                int initialCacheCount = CombatHealth.CachedColliderBindingCount;
                Assert.AreSame(health, CombatHealth.ResolveFromCollider(collider));
                Assert.AreEqual(initialCacheCount + 1, CombatHealth.CachedColliderBindingCount);
                Assert.AreSame(health, CombatHealth.ResolveFromCollider(collider));
                Assert.AreEqual(initialCacheCount + 1, CombatHealth.CachedColliderBindingCount);

                Object.Destroy(health);
                yield return null;
                CombatHealth replacementHealth = root.AddComponent<CombatHealth>();
                Assert.AreSame(replacementHealth, CombatHealth.ResolveFromCollider(collider));
                Assert.AreEqual(initialCacheCount + 1, CombatHealth.CachedColliderBindingCount);
            }
            finally
            {
                Object.Destroy(root);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator SummonProxyColliderBindingCacheReusesAndRefreshesResolvedProxy()
        {
            GameObject root = new GameObject("ColliderProxyCacheRoot");
            GameObject hitbox = new GameObject("ColliderProxyCacheHitbox");
            hitbox.transform.SetParent(root.transform, worldPositionStays: false);
            SphereCollider collider = hitbox.AddComponent<SphereCollider>();
            CombatHealth health = root.AddComponent<CombatHealth>();
            SummonFrontlineProxy proxy = root.AddComponent<SummonFrontlineProxy>();
            proxy.ConfigureHealth(health);

            try
            {
                yield return null;

                int initialCacheCount = SummonFrontlineProxy.CachedColliderBindingCount;
                Assert.AreSame(proxy, SummonFrontlineProxy.ResolveFromCollider(collider));
                Assert.AreEqual(initialCacheCount + 1, SummonFrontlineProxy.CachedColliderBindingCount);
                Assert.AreSame(proxy, SummonFrontlineProxy.ResolveFromCollider(collider));
                Assert.AreEqual(initialCacheCount + 1, SummonFrontlineProxy.CachedColliderBindingCount);

                Object.Destroy(proxy);
                yield return null;
                SummonFrontlineProxy replacementProxy = root.AddComponent<SummonFrontlineProxy>();
                replacementProxy.ConfigureHealth(health);
                Assert.AreSame(replacementProxy, SummonFrontlineProxy.ResolveFromCollider(collider));
                Assert.AreEqual(initialCacheCount + 1, SummonFrontlineProxy.CachedColliderBindingCount);
            }
            finally
            {
                Object.Destroy(root);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PressureScreenColliderBindingCacheRefreshesNegativeResults()
        {
            GameObject root = new GameObject("ColliderPressureScreenCacheRoot");
            GameObject hitbox = new GameObject("ColliderPressureScreenCacheHitbox");
            hitbox.transform.SetParent(root.transform, worldPositionStays: false);
            SphereCollider collider = hitbox.AddComponent<SphereCollider>();

            try
            {
                yield return null;

                int initialCacheCount = SummonPressureScreen.CachedColliderBindingCount;
                Assert.IsNull(SummonPressureScreen.ResolveFromCollider(collider));
                Assert.AreEqual(initialCacheCount + 1, SummonPressureScreen.CachedColliderBindingCount);
                Assert.IsNull(SummonPressureScreen.ResolveFromCollider(collider));
                Assert.AreEqual(initialCacheCount + 1, SummonPressureScreen.CachedColliderBindingCount);

                SummonPressureScreen screen = root.AddComponent<SummonPressureScreen>();
                Assert.AreSame(screen, SummonPressureScreen.ResolveFromCollider(collider));
                Assert.AreEqual(initialCacheCount + 1, SummonPressureScreen.CachedColliderBindingCount);

                Object.Destroy(screen);
                yield return null;
                Assert.IsNull(SummonPressureScreen.ResolveFromCollider(collider));
                Assert.AreEqual(initialCacheCount + 1, SummonPressureScreen.CachedColliderBindingCount);
            }
            finally
            {
                Object.Destroy(root);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PerfectDodgeTimeWarpUsesRegisteredProjectileReceiver()
        {
            GameObject player = new GameObject("TimeWarpRegistryPlayer");
            CombatHealth playerHealth = player.AddComponent<CombatHealth>();
            playerHealth.ConfigureTeam(DamageTeam.Player);
            PerfectDodgeTimeWarp timeWarp = player.AddComponent<PerfectDodgeTimeWarp>();

            GameObject source = new GameObject("TimeWarpRegistrySource");
            CombatHealth sourceHealth = source.AddComponent<CombatHealth>();
            sourceHealth.ConfigureTeam(DamageTeam.Enemy);

            GameObject projectileRoot = new GameObject("TimeWarpRegistryProjectile");
            BossBarrageProjectile projectile = projectileRoot.AddComponent<BossBarrageProjectile>();
            projectile.Configure(
                sourceHealth,
                DamageTeam.Enemy,
                1f,
                Vector3.back,
                0f,
                2f,
                0.2f);

            try
            {
                yield return null;
                CombatTimeDilationReceiver projectileReceiver =
                    projectileRoot.GetComponent<CombatTimeDilationReceiver>();
                Assert.IsNotNull(projectileReceiver);
                Assert.IsTrue(Contains(CombatTimeDilationReceiver.ActiveInstances, projectileReceiver));

                MethodInfo trigger = typeof(PerfectDodgeTimeWarp).GetMethod(
                    "HandlePerfectDodgeTriggered",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(trigger);
                DamageInfo damageInfo = new DamageInfo(
                    sourceHealth,
                    DamageTeam.Enemy,
                    1f,
                    player.transform.position,
                    Vector3.back,
                    0f);
                trigger.Invoke(
                    timeWarp,
                    new object[] { damageInfo });

                Assert.Less(projectileReceiver.CurrentTimeScale, 1f);
                Assert.GreaterOrEqual(timeWarp.LastAffectedReceiverCount, 2);
                Assert.GreaterOrEqual(timeWarp.ReceiverContextCacheCount, 2);
                int contextBuildCount = timeWarp.ReceiverContextBuildCount;

                MethodInfo applyThreatTimeDilation = typeof(PerfectDodgeTimeWarp).GetMethod(
                    "ApplyThreatTimeDilation",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(applyThreatTimeDilation);
                applyThreatTimeDilation.Invoke(timeWarp, new object[] { damageInfo });
                Assert.AreEqual(
                    contextBuildCount,
                    timeWarp.ReceiverContextBuildCount,
                    "Repeated time-warp refreshes should reuse receiver component classification.");
            }
            finally
            {
                Object.Destroy(projectileRoot);
                Object.Destroy(source);
                Object.Destroy(player);
            }

            yield return null;
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator EnemyCardStackReadoutsSkipEquivalentTextAndLayoutUpdates()
        {
            GameObject root = new GameObject("EnemyCardStackHotPathTest", typeof(RectTransform));
            root.SetActive(false);
            RectTransform rootRect = (RectTransform)root.transform;
            rootRect.sizeDelta = new Vector2(180f, 220f);

            Component countdownText = CreateText(rootRect, "CountdownText");
            Component projectileText = CreateText(rootRect, "ProjectileCountdownText");
            Component headerText = CreateText(rootRect, "HeaderText");
            EnemyCardStackUI cardStack = root.AddComponent<EnemyCardStackUI>();
            SetPrivateField(cardStack, "countdownText", countdownText);
            SetPrivateField(cardStack, "projectileCountdownText", projectileText);
            SetPrivateField(cardStack, "headerText", headerText);

            try
            {
                root.SetActive(true);
                yield return null;

                InvokePrivate(cardStack, "UpdateCountdown", 1.04f);
                Assert.AreEqual("SUMMON  1.0s", GetText(countdownText));
                SetText(countdownText, "unchanged-tenth");
                InvokePrivate(cardStack, "UpdateCountdown", 1.02f);
                Assert.AreEqual(
                    "unchanged-tenth",
                    GetText(countdownText),
                    "Equivalent displayed tenths should not rewrite TMP text.");
                InvokePrivate(cardStack, "UpdateCountdown", 0.94f);
                Assert.AreEqual("SUMMON  0.9s", GetText(countdownText));

                InvokePrivate(cardStack, "UpdateProjectileCountdown", 2.2f);
                Assert.AreEqual("HEX  CHARGING", GetText(projectileText));
                SetText(projectileText, "unchanged-band");
                InvokePrivate(cardStack, "UpdateProjectileCountdown", 2.0f);
                Assert.AreEqual(
                    "unchanged-band",
                    GetText(projectileText),
                    "Equivalent warning bands should not rewrite TMP text.");
                InvokePrivate(cardStack, "UpdateProjectileCountdown", 1.7f);
                Assert.AreEqual("HEX  SOON", GetText(projectileText));

                InvokePrivate(cardStack, "ApplyPanelLayout");
                RectTransform headerRect = GetRectTransform(headerText);
                headerRect.anchoredPosition = new Vector2(999f, 999f);
                InvokePrivate(cardStack, "ApplyPanelLayout");
                Assert.AreEqual(
                    new Vector2(999f, 999f),
                    headerRect.anchoredPosition,
                    "An unchanged panel width should keep the cached layout.");

                rootRect.sizeDelta = new Vector2(140f, 220f);
                InvokePrivate(cardStack, "ApplyPanelLayout");
                Assert.AreNotEqual(
                    new Vector2(999f, 999f),
                    headerRect.anchoredPosition,
                    "A width change should invalidate and reapply the panel layout.");
            }
            finally
            {
                Object.Destroy(root);
            }

            yield return null;
        }

        private static Component CreateText(RectTransform parent, string name)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            System.Type textType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            Assert.IsNotNull(textType, "Expected the TextMeshPro runtime assembly.");
            return textObject.AddComponent(textType);
        }

        private static string GetText(Component text)
        {
            PropertyInfo property = text.GetType().GetProperty("text");
            Assert.IsNotNull(property);
            return (string)property.GetValue(text);
        }

        private static void SetText(Component text, string value)
        {
            PropertyInfo property = text.GetType().GetProperty("text");
            Assert.IsNotNull(property);
            property.SetValue(text, value);
        }

        private static RectTransform GetRectTransform(Component text)
        {
            PropertyInfo property = text.GetType().GetProperty("rectTransform");
            Assert.IsNotNull(property);
            return (RectTransform)property.GetValue(text);
        }

        private static object InvokePrivate(object target, string methodName, params object[] arguments)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Expected method '{methodName}'.");
            return method.Invoke(target, arguments);
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Expected field '{fieldName}'.");
            field.SetValue(target, value);
        }

        private static bool Contains<T>(IReadOnlyList<T> items, T expected)
            where T : Object
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == expected)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
