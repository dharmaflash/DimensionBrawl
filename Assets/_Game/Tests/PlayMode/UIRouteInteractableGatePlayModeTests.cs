using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class UIRouteInteractableGatePlayModeTests
    {
        private const string LobbyPrefabPath =
            "Assets/_Game/UI/Lobby/PF_UI_LobbyScreen.prefab";

        private static readonly string[] PlaceholderNames =
        {
            "Anchor_Character",
            "Anchor_Summon",
            "Anchor_Inventory",
            "Anchor_Mail",
            "Anchor_Settings"
        };

        [UnityTest]
        public IEnumerator LobbyPlaceholdersRemainDisabledAcrossRouteAndArrivalUnlocks()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LobbyPrefabPath);
            Assert.That(prefab, Is.Not.Null, $"Missing Lobby prefab: {LobbyPrefabPath}");

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            Assert.That(instance, Is.Not.Null);
            instance.hideFlags = HideFlags.HideAndDontSave;

            try
            {
                yield return null;

                Type gateType = RequireProductType("DimensionBrawl.UI.UIRouteInteractableGate");
                Component gate = instance.GetComponentInChildren(gateType, true);
                Assert.That(gate, Is.Not.Null);
                CanvasGroup[] dimGroups = ReadPrivateField<CanvasGroup[]>(gate, "dimGroups");
                Assert.That(dimGroups, Has.Length.EqualTo(1));
                CanvasGroup dimGroup = dimGroups[0];
                Assert.That(dimGroup, Is.Not.Null);
                Assert.That(dimGroup.alpha, Is.EqualTo(1f).Within(0.001f));

                Button[] placeholders = new Button[PlaceholderNames.Length];
                for (int i = 0; i < PlaceholderNames.Length; i++)
                {
                    placeholders[i] = FindRequiredButton(instance.transform, PlaceholderNames[i]);
                }

                AssertPlaceholdersDisabled(placeholders, "initial activation");

                Selectable[] registered = ReadPrivateField<Selectable[]>(gate, "selectables");
                Selectable enabledCapability = Array.Find(
                    registered,
                    selectable => selectable != null
                        && selectable.isActiveAndEnabled
                        && selectable.interactable);
                Assert.That(enabledCapability, Is.Not.Null, "Lobby needs one enabled route capability.");

                ApplyRouterState(gate, "Preparing", true);
                Assert.That(enabledCapability.interactable, Is.False);
                Assert.That(dimGroup.alpha, Is.EqualTo(0.68f).Within(0.001f),
                    "A local route lock must retain the authored lobby dim beat.");
                AssertPlaceholdersDisabled(placeholders, "route lock");

                ApplyRouterState(gate, "Completed", false);
                Assert.That(enabledCapability.interactable, Is.True);
                Assert.That(dimGroup.alpha, Is.EqualTo(1f).Within(0.001f));
                AssertPlaceholdersDisabled(placeholders, "successful route release");

                enabledCapability.interactable = false;
                ApplyRouterState(gate, "Preparing", true);
                ApplyRouterState(gate, "Failed", false);
                Assert.That(
                    enabledCapability.interactable,
                    Is.False,
                    "A route failure must restore the latest capability baseline.");
                AssertPlaceholdersDisabled(placeholders, "failed route release");

                enabledCapability.interactable = true;
                object transitionTicket = CreateTransitionTicket(instance.GetInstanceID());
                Assert.That(
                    RequireMethod(gateType, "AcquireTransitionRouteLock")
                        .Invoke(gate, new[] { transitionTicket }),
                    Is.EqualTo(true));
                Assert.That(ReadProperty(gate, "GlobalRouteLocked"), Is.True);
                Assert.That(enabledCapability.interactable, Is.False);
                Assert.That(dimGroup.alpha, Is.EqualTo(1f).Within(0.001f),
                    "A destination ticket must lock input without dimming the revealed scene.");

                ApplyRouterState(gate, "Preparing", true);
                Assert.That(dimGroup.alpha, Is.EqualTo(0.68f).Within(0.001f),
                    "A local route lock must still dim while a transition ticket is held.");
                ApplyRouterState(gate, "Completed", false);
                Assert.That(enabledCapability.interactable, Is.False,
                    "A local completion must not release the transition ticket lock.");
                Assert.That(dimGroup.alpha, Is.EqualTo(1f).Within(0.001f));

                Assert.That(
                    RequireMethod(gateType, "ReleaseTransitionRouteLock")
                        .Invoke(gate, new[] { transitionTicket }),
                    Is.EqualTo(true));
                Assert.That(ReadProperty(gate, "GlobalRouteLocked"), Is.False);
                Assert.That(enabledCapability.interactable, Is.True);
                Assert.That(dimGroup.alpha, Is.EqualTo(1f).Within(0.001f));
                AssertPlaceholdersDisabled(placeholders, "transition ticket release");

                RequireMethod(gateType, "SetGlobalRouteLocked").Invoke(gate, new object[] { true });
                Assert.That(ReadProperty(gate, "GlobalRouteLocked"), Is.True);
                Assert.That(enabledCapability.interactable, Is.False);
                Assert.That(dimGroup.alpha, Is.EqualTo(0.68f).Within(0.001f),
                    "An explicit external route lock must retain its visual dim.");
                AssertPlaceholdersDisabled(placeholders, "arrival lock");

                ApplyRouterState(gate, "Completed", false);
                Assert.That(
                    enabledCapability.interactable,
                    Is.False,
                    "A local completion must not release the global arrival lock.");

                RequireMethod(gateType, "SetGlobalRouteLocked").Invoke(gate, new object[] { false });
                Assert.That(ReadProperty(gate, "GlobalRouteLocked"), Is.False);
                Assert.That(enabledCapability.interactable, Is.True);
                Assert.That(dimGroup.alpha, Is.EqualTo(1f).Within(0.001f));
                AssertPlaceholdersDisabled(placeholders, "arrival lock release");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static void ApplyRouterState(
            Component gate,
            string phaseName,
            bool isRouting)
        {
            Type routeIdType = RequireProductType("DimensionBrawl.UI.UIRouteId");
            Type phaseType = RequireProductType("DimensionBrawl.UI.UISceneFlowPhase");
            Type stateType = RequireProductType("DimensionBrawl.UI.UISceneFlowState");
            object routeId = Enum.Parse(routeIdType, "Lobby");
            object phase = Enum.Parse(phaseType, phaseName);
            ConstructorInfo constructor = stateType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[]
                {
                    routeIdType,
                    typeof(string),
                    phaseType,
                    typeof(float),
                    typeof(string),
                    typeof(bool)
                },
                null);
            Assert.That(constructor, Is.Not.Null, "Missing UISceneFlowState constructor.");
            object state = constructor.Invoke(
                new[]
                {
                    routeId,
                    "UI_Lobby",
                    phase,
                    isRouting ? 0.5f : 1f,
                    phaseName,
                    isRouting
                });

            MethodInfo handler = RequireMethod(gate.GetType(), "HandleStateChanged");
            handler.Invoke(
                gate,
                new[] { state });
        }

        private static Button FindRequiredButton(Transform root, string objectName)
        {
            Transform[] descendants = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < descendants.Length; i++)
            {
                Transform candidate = descendants[i];
                if (candidate != null && candidate.name == objectName)
                {
                    Button button = candidate.GetComponent<Button>();
                    Assert.That(button, Is.Not.Null, $"{objectName} must retain its Button.");
                    return button;
                }
            }

            Assert.Fail($"Missing Lobby placeholder: {objectName}");
            return null;
        }

        private static object CreateTransitionTicket(int ownerInstanceId)
        {
            Type ticketType = RequireProductType("DimensionBrawl.UI.UISceneTransitionTicket");
            Type routeIdType = RequireProductType("DimensionBrawl.UI.UIRouteId");
            ConstructorInfo constructor = ticketType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(int),
                    typeof(uint),
                    routeIdType,
                    typeof(int),
                    typeof(string),
                    typeof(string)
                },
                null);
            Assert.That(constructor, Is.Not.Null, "Missing UISceneTransitionTicket constructor.");
            return constructor.Invoke(new object[]
            {
                ownerInstanceId,
                1u,
                Enum.Parse(routeIdType, "Lobby"),
                0,
                "UI_Lobby",
                string.Empty
            });
        }

        private static void AssertPlaceholdersDisabled(Button[] placeholders, string phase)
        {
            for (int i = 0; i < placeholders.Length; i++)
            {
                Assert.That(
                    placeholders[i].interactable,
                    Is.False,
                    $"{placeholders[i].name} was promoted during {phase}.");
            }
        }

        private static T ReadPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing field {fieldName} on {target.GetType().Name}.");
            return (T)field.GetValue(target);
        }

        private static Type RequireProductType(string fullName)
        {
            Type type = Type.GetType(fullName + ", DimensionBrawl.Runtime")
                ?? Type.GetType(fullName + ", Assembly-CSharp");
            Assert.That(type, Is.Not.Null, $"Missing product type {fullName}.");
            return type;
        }

        private static MethodInfo RequireMethod(Type type, string methodName)
        {
            MethodInfo method = type.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Missing method {type.Name}.{methodName}.");
            return method;
        }

        private static object ReadProperty(object target, string propertyName)
        {
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null, $"Missing property {target.GetType().Name}.{propertyName}.");
            return property.GetValue(target);
        }
    }
}
