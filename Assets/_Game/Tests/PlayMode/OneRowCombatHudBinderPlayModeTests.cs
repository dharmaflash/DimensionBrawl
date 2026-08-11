using System;
using System.Collections;
using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.Player;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class OneRowCombatHudBinderPlayModeTests
    {
        private const string CombatHudActionCatalogPath =
            "Assets/_Game/DesignData/UI/DB_CombatHudActions.asset";

        [Test]
        public void CatalogMarksEveryRoutedAdvancedCombatActionAsLive()
        {
            UnityEngine.Object catalog = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(CombatHudActionCatalogPath);
            Assert.That(catalog, Is.Not.Null, $"Missing combat HUD action catalog at {CombatHudActionCatalogPath}.");
            SerializedObject serializedCatalog = new(catalog);

            AssertLiveCatalogEntry(serializedCatalog, "Skill1", "Skill 1", "follow-up skill");
            AssertLiveCatalogEntry(serializedCatalog, "Ultimate", "Mode Swap", "mode swap");
            AssertLiveCatalogEntry(serializedCatalog, "SummonSlot1", "Summon 1", "pressure-block summon");
            AssertLiveCatalogEntry(serializedCatalog, "SummonSlot2", "Summon 2", "support summon slot 2");
            AssertLiveCatalogEntry(serializedCatalog, "SummonSlot3", "Summon 3", "support summon slot 3");
        }

        [UnityTest]
        public IEnumerator ConfigureRoutesActionsHoldPauseAndOverlayInputLock()
        {
            GameObject playerObject = new("OneRowHudPlayer", typeof(CharacterController));
            CombatHealth playerHealth = CreateHealth(playerObject, DamageTeam.Player, 100f);
            PlayerMovementController movement = playerObject.AddComponent<PlayerMovementController>();
            PlayerActionController action = playerObject.AddComponent<PlayerActionController>();
            PlayerCombatModeController mode = playerObject.AddComponent<PlayerCombatModeController>();
            PlayerRangedBasicAttackAction ranged = playerObject.AddComponent<PlayerRangedBasicAttackAction>();
            PlayerSkill1Action skill = playerObject.AddComponent<PlayerSkill1Action>();
            PlayerSummonSlot1Action summon1 = playerObject.AddComponent<PlayerSummonSlot1Action>();
            PlayerSupportSummonSlotAction summon2 = playerObject.AddComponent<PlayerSupportSummonSlotAction>();
            PlayerSupportSummonSlotAction summon3 = playerObject.AddComponent<PlayerSupportSummonSlotAction>();
            GameObject bossObject = new("OneRowHudBoss");
            CombatHealth bossHealth = CreateHealth(bossObject, DamageTeam.Enemy, 200f);
            GameObject encounterObject = new("OneRowHudEncounter");
            encounterObject.SetActive(false);
            CombatEncounterController encounter = encounterObject.AddComponent<CombatEncounterController>();
            encounter.ConfigureCombatants(playerHealth, bossHealth);

            Type presenterType = ResolveHudType("CombatHudPresenter");
            Type bridgeType = ResolveHudType("CombatHudInputBridge");
            Type joystickType = ResolveHudType("CombatHudVirtualJoystick");
            Type aimDragType = ResolveHudType("CombatHudAimDragInput");
            Type pointerInputType = ResolveHudType("CombatHudPointerActionInput");
            Type actionIdType = ResolveHudType("CombatHudActionId");
            Type binderType = ResolveHudType("OneRowCombatHudBinder");

            GameObject hudRoot = new("OneRowHudRoot", typeof(RectTransform));
            hudRoot.SetActive(false);
            Component presenter = hudRoot.AddComponent(presenterType);
            Component bridge = hudRoot.AddComponent(bridgeType);
            OneRowCombatHudSessionOverlayProbe sessionSurface =
                hudRoot.AddComponent<OneRowCombatHudSessionOverlayProbe>();
            Component binder = hudRoot.AddComponent(binderType);

            GameObject joystickObject = new("MoveJoystick", typeof(RectTransform));
            joystickObject.transform.SetParent(hudRoot.transform, false);
            Component joystick = joystickObject.AddComponent(joystickType);
            Invoke(joystick, "Configure", movement, null);

            GameObject aimObject = new("AimDragInput", typeof(RectTransform));
            aimObject.transform.SetParent(hudRoot.transform, false);
            Component aimDrag = aimObject.AddComponent(aimDragType);
            Invoke(aimDrag, "Configure", movement, mode, null, ranged);

            GameObject pointerObject = new("BasicAttackPointer", typeof(RectTransform));
            pointerObject.transform.SetParent(hudRoot.transform, false);
            Component pointerInput = pointerObject.AddComponent(pointerInputType);
            object basicAttackId = Enum.Parse(actionIdType, "BasicAttack");
            Invoke(pointerInput, "Configure", bridge, basicAttackId, true);

            try
            {
                Invoke(
                    binder,
                    "Configure",
                    presenter,
                    bridge,
                    joystick,
                    sessionSurface,
                    encounter,
                    playerHealth,
                    bossHealth,
                    movement,
                    action,
                    mode,
                    ranged,
                    skill,
                    summon1,
                    summon2,
                    summon3);
                encounterObject.SetActive(true);
                hudRoot.SetActive(true);

                int rangedInputCount = 0;
                ranged.RangedFireInputStarted += () => rangedInputCount++;
                Assert.That(mode.IsRangedMode, Is.True);

                Invoke(bridge, "RequestBasicAttack");
                Assert.That(rangedInputCount, Is.EqualTo(1));

                Invoke(bridge, "SetActionHeld", basicAttackId, true);
                Assert.That(ranged.IsFireHeld, Is.True);
                Invoke(bridge, "SetActionHeld", basicAttackId, false);
                Assert.That(ranged.IsFireHeld, Is.False);

                Invoke(bridge, "RequestSkill1");
                Invoke(bridge, "RequestSummonSlot1");
                Invoke(bridge, "RequestSummonSlot2");
                Invoke(bridge, "RequestSummonSlot3");
                Assert.That(skill.ShowUseBlockedHint, Is.True);
                Assert.That(summon1.ShowUseBlockedHint, Is.True);
                Assert.That(summon2.ShowUseBlockedHint, Is.True);
                Assert.That(summon3.ShowUseBlockedHint, Is.True);

                Invoke(bridge, "RequestUltimate");
                Assert.That(mode.IsMeleeMode, Is.True);
                Invoke(bridge, "RequestBasicAttack");
                Assert.That(ReadPrivateField<bool>(action, "mobileAttackQueued"), Is.True);
                Invoke(bridge, "RequestDodge");
                Assert.That(ReadPrivateField<bool>(action, "mobileDodgeQueued"), Is.True);

                Invoke(bridge, "RequestPause");
                Assert.That(sessionSurface.PauseCount, Is.EqualTo(1));
                Assert.That(ReadPublicProperty<bool>(binder, "IsCombatMenuInputLocked"), Is.True);
                Assert.That(ReadPublicProperty<bool>(joystick, "IsInputBlocked"), Is.True);
                Assert.That(ReadPublicProperty<bool>(aimDrag, "IsInputBlocked"), Is.True);
                Assert.That(ReadPublicProperty<bool>(pointerInput, "IsInputBlocked"), Is.True);
                Assert.That(movement.IsSharedMoveInputBlocked, Is.True);
                Assert.That(movement.IsSharedLookInputBlocked, Is.True);
                Assert.That(movement.AreSharedFacingRequestsBlocked, Is.True);
                Assert.That(action.IsCinematicInputLocked, Is.True);
                Assert.That(mode.IsCinematicInputLocked, Is.True);
                Assert.That(ranged.IsCinematicInputLocked, Is.True);
                Assert.That(skill.IsCinematicInputLocked, Is.True);
                Assert.That(summon1.IsCinematicInputLocked, Is.True);
                Assert.That(summon2.IsCinematicInputLocked, Is.True);
                Assert.That(summon3.IsCinematicInputLocked, Is.True);

                Invoke(bridge, "RequestBasicAttack");
                Assert.That(rangedInputCount, Is.EqualTo(1), "Combat menu lock must reject action requests.");

                sessionSurface.Resume();
                Assert.That(ReadPublicProperty<bool>(binder, "IsCombatMenuInputLocked"), Is.False);
                Assert.That(ReadPublicProperty<bool>(joystick, "IsInputBlocked"), Is.False);
                Assert.That(ReadPublicProperty<bool>(aimDrag, "IsInputBlocked"), Is.False);
                Assert.That(ReadPublicProperty<bool>(pointerInput, "IsInputBlocked"), Is.False);
                Assert.That(movement.IsSharedMoveInputBlocked, Is.False);
                Assert.That(action.IsCinematicInputLocked, Is.False);
                Assert.That(mode.IsCinematicInputLocked, Is.False);
                Assert.That(ranged.IsCinematicInputLocked, Is.False);

                Assert.That(ApplyDamage(playerHealth, DamageTeam.Enemy, 200f), Is.True);
                Assert.That(sessionSurface.FailureCount, Is.EqualTo(1));
                Assert.That(sessionSurface.Mode, Is.EqualTo(CombatSessionOverlayMode.Failure));
                Assert.That(ReadPublicProperty<bool>(binder, "IsCombatMenuInputLocked"), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(hudRoot);
                UnityEngine.Object.DestroyImmediate(encounterObject);
                UnityEngine.Object.DestroyImmediate(bossObject);
                UnityEngine.Object.DestroyImmediate(playerObject);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator RefreshPublishesObjectiveHealthBossAndHostileDamageFeedbackWithNullOptionals()
        {
            GameObject playerObject = new("OneRowHudReadoutPlayer", typeof(CharacterController));
            CombatHealth playerHealth = CreateHealth(playerObject, DamageTeam.Player, 100f);
            PlayerMovementController movement = playerObject.AddComponent<PlayerMovementController>();
            PlayerActionController action = playerObject.AddComponent<PlayerActionController>();
            GameObject bossObject = new("OneRowHudReadoutBoss");
            CombatHealth bossHealth = CreateHealth(bossObject, DamageTeam.Enemy, 200f);
            GameObject encounterObject = new("OneRowHudReadoutEncounter");
            encounterObject.SetActive(false);
            CombatEncounterController encounter = encounterObject.AddComponent<CombatEncounterController>();
            encounter.ConfigureCombatants(playerHealth, bossHealth);

            Type presenterType = ResolveHudType("CombatHudPresenter");
            Type binderType = ResolveHudType("OneRowCombatHudBinder");
            GameObject hudRoot = new("OneRowHudReadoutRoot", typeof(RectTransform));
            hudRoot.SetActive(false);
            Component presenter = hudRoot.AddComponent(presenterType);
            Text objectiveReadout = CreateText(hudRoot.transform, "Objective");
            Text healthReadout = CreateText(hudRoot.transform, "Health");
            Image healthFill = CreateImage(hudRoot.transform, "HealthFill");
            RectTransform bossHudRoot = new GameObject("BossHudRoot", typeof(RectTransform))
                .GetComponent<RectTransform>();
            bossHudRoot.SetParent(hudRoot.transform, false);
            Image bossHealthFill = CreateImage(bossHudRoot, "BossHealthFill");
            bossHealthFill.rectTransform.sizeDelta = new Vector2(200f, 20f);
            SetField(presenter, "objectiveText", objectiveReadout);
            SetField(presenter, "healthText", healthReadout);
            SetField(presenter, "healthFill", healthFill);
            SetField(presenter, "bossHudRoot", bossHudRoot);
            SetField(presenter, "bossHealthFill", bossHealthFill);
            Component binder = hudRoot.AddComponent(binderType);

            try
            {
                Invoke(
                    binder,
                    "Configure",
                    presenter,
                    null,
                    null,
                    null,
                    encounter,
                    playerHealth,
                    bossHealth,
                    movement,
                    action,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null);
                encounterObject.SetActive(true);
                hudRoot.SetActive(true);
                Invoke(binder, "RefreshHudNow");

                Assert.That(objectiveReadout.text, Is.EqualTo("Defeat the enemy."));
                Assert.That(healthReadout.text, Is.EqualTo("100/100"));
                Assert.That(healthFill.fillAmount, Is.EqualTo(1f).Within(0.001f));
                Assert.That(ReadPublicProperty<bool>(presenter, "BossHudVisible"), Is.True);
                Assert.That(ReadPublicProperty<float>(presenter, "BossHealthFillAmount"), Is.EqualTo(1f).Within(0.001f));

                Assert.That(ApplyDamage(bossHealth, DamageTeam.Player, 50f), Is.True);
                Assert.That(ReadPublicProperty<float>(presenter, "BossHealthFillAmount"), Is.EqualTo(0.75f).Within(0.001f));

                Assert.That(ApplyDamage(playerHealth, DamageTeam.Enemy, 25f), Is.True);
                Assert.That(healthReadout.text, Is.EqualTo("75/100"));
                Transform damageOverlay = hudRoot.transform.Find("PlayerDamageOverlay");
                Assert.That(damageOverlay, Is.Not.Null);
                Assert.That(damageOverlay.gameObject.activeSelf, Is.True);

                Assert.That(ApplyDamage(playerHealth, DamageTeam.Enemy, 100f), Is.True);
                Assert.That(objectiveReadout.text, Is.EqualTo("Combat failed."));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(hudRoot);
                UnityEngine.Object.DestroyImmediate(encounterObject);
                UnityEngine.Object.DestroyImmediate(bossObject);
                UnityEngine.Object.DestroyImmediate(playerObject);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
        }

        private static Type ResolveHudType(string typeName)
        {
            Type type = Type.GetType($"DimensionBrawl.UI.{typeName}, Assembly-CSharp", throwOnError: true);
            Assert.That(type, Is.Not.Null);
            return type;
        }

        private static void AssertLiveCatalogEntry(
            SerializedObject catalog,
            string actionName,
            string expectedDisplayName,
            string expectedDescription)
        {
            Type actionIdType = ResolveHudType("CombatHudActionId");
            int expectedActionId = Convert.ToInt32(Enum.Parse(actionIdType, actionName));
            SerializedProperty actions = catalog.FindProperty("actions");
            Assert.That(actions, Is.Not.Null);

            for (int i = 0; i < actions.arraySize; i++)
            {
                SerializedProperty action = actions.GetArrayElementAtIndex(i);
                if (action.FindPropertyRelative("actionId").intValue != expectedActionId)
                {
                    continue;
                }

                string description = action.FindPropertyRelative("placeholderState").stringValue;
                Assert.That(action.FindPropertyRelative("enabledInV1").boolValue, Is.True,
                    $"{actionName} is routed by the combat HUD and must be enabled in V1 metadata.");
                Assert.That(action.FindPropertyRelative("displayName").stringValue, Is.EqualTo(expectedDisplayName));
                Assert.That(description, Does.Contain(expectedDescription).IgnoreCase);
                Assert.That(description.IndexOf("placeholder", StringComparison.OrdinalIgnoreCase), Is.LessThan(0),
                    $"{actionName} must describe live behavior instead of a placeholder state.");
                return;
            }

            Assert.Fail($"Combat HUD action catalog is missing {actionName}.");
        }

        private static object Invoke(object target, string methodName, params object[] arguments)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null, $"Missing method {target.GetType().Name}.{methodName}.");
            return method.Invoke(target, arguments);
        }

        private static T ReadPublicProperty<T>(object target, string propertyName)
        {
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(property, Is.Not.Null, $"Missing property {target.GetType().Name}.{propertyName}.");
            return (T)property.GetValue(target);
        }

        private static CombatHealth CreateHealth(GameObject owner, DamageTeam team, float maxHealth)
        {
            CombatHealth health = owner.AddComponent<CombatHealth>();
            health.ConfigureTeam(team);
            health.ConfigureMaxHealth(maxHealth);
            return health;
        }

        private static bool ApplyDamage(CombatHealth target, DamageTeam sourceTeam, float amount)
        {
            return target.TryApplyDamage(new DamageInfo(
                null,
                sourceTeam,
                amount,
                target.transform.position,
                Vector3.forward,
                0f,
                DamageResponsePolicy.DamageOnly,
                CombatControlLockPolicy.None));
        }

        private static Text CreateText(Transform parent, string name)
        {
            GameObject textObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);
            return textObject.GetComponent<Text>();
        }

        private static Image CreateImage(Transform parent, string name)
        {
            GameObject imageObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            return imageObject.GetComponent<Image>();
        }

        private static T ReadPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing field {target.GetType().Name}.{fieldName}.");
            return (T)field.GetValue(target);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing field {target.GetType().Name}.{fieldName}.");
            field.SetValue(target, value);
        }
    }

    public sealed class OneRowCombatHudSessionOverlayProbe : MonoBehaviour, ICombatSessionOverlay
    {
        public CombatSessionOverlayMode Mode { get; private set; }
        public bool IsVisible => Mode != CombatSessionOverlayMode.Hidden;
        public int PauseCount { get; private set; }
        public int FailureCount { get; private set; }

        public event Action<bool> CombatInputBlockChanged;

        public void Configure(
            BossBarrageEncounterController resultSource,
            ActionScreenCuePresenter screenCuePresenter)
        {
        }

        public void ShowPause()
        {
            PauseCount++;
            SetMode(CombatSessionOverlayMode.Pause);
        }

        public void ShowSettings()
        {
            SetMode(CombatSessionOverlayMode.Settings);
        }

        public void ShowFailure()
        {
            FailureCount++;
            SetMode(CombatSessionOverlayMode.Failure);
        }

        public void Resume()
        {
            SetMode(CombatSessionOverlayMode.Hidden);
        }

        public void DismissForStageClear()
        {
            SetMode(CombatSessionOverlayMode.Hidden);
        }

        private void SetMode(CombatSessionOverlayMode newMode)
        {
            bool wasVisible = IsVisible;
            Mode = newMode;
            if (wasVisible != IsVisible)
            {
                CombatInputBlockChanged?.Invoke(IsVisible);
            }
        }
    }
}
