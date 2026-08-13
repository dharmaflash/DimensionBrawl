using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using DimensionBrawl.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class UISceneTransitionHandoffPlayModeTests
    {
        private const string TransitionOverlayPrefabPath =
            "Assets/_Game/UI/Transitions/PF_UI_TransitionOverlay.prefab";
        private const string SceneShellPrefabPath =
            "Assets/_Game/UI/Common/PF_UI_SceneShell.prefab";
        private const string CombatHudPrefabPath =
            "Assets/_Game/UI/CombatHud/PF_UI_CombatHud.prefab";

        private const string OwnerTypeName =
            "DimensionBrawl.UI.UISceneTransitionHandoffOwner";
        private const string PresenterTypeName =
            "DimensionBrawl.UI.UITransitionPresenter";
        private const string ReceiverTypeName =
            "DimensionBrawl.UI.UISceneTransitionArrivalReceiver";
        private const string GateTypeName =
            "DimensionBrawl.UI.UIRouteInteractableGate";
        private const string RouteTypeName =
            "DimensionBrawl.UI.UIScreenRouteTable+Route";
        private const string RouteIdTypeName =
            "DimensionBrawl.UI.UIRouteId";

        [SetUp]
        public void PrepareTransitionRuntime()
        {
            TryResetProductType(ReceiverTypeName);
            TryResetProductType(OwnerTypeName);
            TryResetProductType("DimensionBrawl.UI.UITransitionHandoffService");
        }

        [TearDown]
        public void ResetTransitionRuntime()
        {
            TryResetProductType(ReceiverTypeName);
            TryResetProductType(OwnerTypeName);
            TryResetProductType("DimensionBrawl.UI.UITransitionHandoffService");

            EventSystem[] eventSystems = UnityEngine.Object.FindObjectsByType<EventSystem>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < eventSystems.Length; i++)
            {
                if (eventSystems[i] != null
                    && eventSystems[i].gameObject.name.StartsWith(
                        "UI Transition Handoff Test",
                        StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(eventSystems[i].gameObject);
                }
            }

            GameObject inputRoot = GameObject.Find("UI Transition Handoff Test Input");
            if (inputRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(inputRoot);
            }

            GameObject[] leftovers = UnityEngine.Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < leftovers.Length; i++)
            {
                GameObject candidate = leftovers[i];
                if (candidate != null
                    && candidate.transform.parent == null
                    && candidate.name.StartsWith("UI Transition Handoff Test", StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(candidate);
                }
            }
        }

        [Test]
        public void TransitionPrefabsBindOnePersistentOwnerAndOneArrivalReceiverPerShell()
        {
            Type ownerType = RequireProductType(OwnerTypeName);
            Type presenterType = RequireProductType(PresenterTypeName);
            Type receiverType = RequireProductType(ReceiverTypeName);

            GameObject overlay = RequirePrefab(TransitionOverlayPrefabPath);
            Component[] owners = overlay.GetComponentsInChildren(ownerType, true);
            Assert.That(owners, Has.Length.EqualTo(1),
                "The transition overlay must have one persistent handoff owner.");

            Component presenter = ReadProperty<Component>(owners[0], "Presenter");
            Assert.That(presenter, Is.Not.Null,
                "The persistent handoff owner must bind its transition presenter.");
            Assert.That(presenterType.IsInstanceOfType(presenter), Is.True);
            Assert.That(presenter.gameObject, Is.SameAs(owners[0].gameObject),
                "The persistent owner and presenter must remain on the same detachable root.");

            AssertReceiverCount(SceneShellPrefabPath, receiverType);
            AssertReceiverCount(CombatHudPrefabPath, receiverType);
        }

        [UnityTest]
        public IEnumerator ExactGenerationRejectsDuplicateAndStaleReadyThenFailOpenRestoresInput()
        {
            Type ownerType = RequireProductType(OwnerTypeName);
            Type gateType = RequireProductType(GateTypeName);
            Type routeType = RequireProductType(RouteTypeName);
            Type routeIdType = RequireProductType(RouteIdTypeName);
            Type ticketType = RequireProductType("DimensionBrawl.UI.UISceneTransitionTicket");

            GameObject inputRoot = new GameObject("UI Transition Handoff Test Input");
            inputRoot.SetActive(false);
            CanvasGroup dimGroup = inputRoot.AddComponent<CanvasGroup>();
            Button button = new GameObject("Capability", typeof(RectTransform), typeof(Button))
                .GetComponent<Button>();
            button.transform.SetParent(inputRoot.transform, false);
            Component gate = inputRoot.AddComponent(gateType);
            SetPrivateField(gate, "selectables", new Selectable[] { button });
            SetPrivateField(gate, "dimGroups", new[] { dimGroup });
            inputRoot.SetActive(true);

            GameObject eventSystemObject = new GameObject(
                "UI Transition Handoff Test EventSystem",
                typeof(EventSystem));
            EventSystem eventSystem = eventSystemObject.GetComponent<EventSystem>();
            Assert.That(eventSystem.enabled, Is.True);

            GameObject overlayPrefab = RequirePrefab(TransitionOverlayPrefabPath);
            GameObject overlay = UnityEngine.Object.Instantiate(overlayPrefab);
            Component owner = overlay.GetComponentsInChildren(ownerType, true).Single();

            yield return null;

            Scene destinationScene = SceneManager.GetActiveScene();
            object route = CreateRoute(
                routeType,
                routeIdType,
                "Lobby",
                destinationScene.name,
                string.Empty);
            MethodInfo beginRoute = RequireMethod(
                ownerType,
                "TryBeginRoute",
                method => method.GetParameters().Length == 3);

            object[] firstBeginArguments = { route, null, null };
            Assert.That(beginRoute.Invoke(owner, firstBeginArguments), Is.EqualTo(true));
            object firstTicket = firstBeginArguments[1];
            uint firstGeneration = ReadProperty<uint>(firstTicket, "Generation");
            Assert.That(firstGeneration, Is.GreaterThan(0u));
            Assert.That(ReadProperty<bool>(firstTicket, "IsValid"), Is.True);

            object[] duplicateBeginArguments = { route, null, null };
            Assert.That(beginRoute.Invoke(owner, duplicateBeginArguments), Is.EqualTo(false),
                "An active generation must reject a competing route begin.");
            Assert.That((string)duplicateBeginArguments[2], Is.Not.Empty);

            object staleTicket = CreateTicket(
                ticketType,
                firstTicket,
                firstGeneration + 1u);
            MethodInfo markActivation = RequireMethod(ownerType, "TryMarkActivationRequested");
            MethodInfo markArrived = RequireMethod(ownerType, "TryMarkDestinationArrived");
            MethodInfo markReady = RequireMethod(ownerType, "TryMarkDestinationReady");

            Assert.That(markActivation.Invoke(owner, new[] { staleTicket }), Is.EqualTo(false));
            Assert.That(markActivation.Invoke(owner, new[] { firstTicket }), Is.EqualTo(true));
            Assert.That(
                markArrived.Invoke(owner, new object[] { staleTicket, destinationScene }),
                Is.EqualTo(false));
            Assert.That(
                markArrived.Invoke(owner, new object[] { firstTicket, destinationScene }),
                Is.EqualTo(true));

            Assert.That(ReadProperty<bool>(gate, "GlobalRouteLocked"), Is.True);
            Assert.That(button.interactable, Is.False);
            Assert.That(dimGroup.alpha, Is.EqualTo(1f).Within(0.001f),
                "A transition-ticket arrival lock must not dim the destination scene.");
            Assert.That(eventSystem.enabled, Is.False);
            Assert.That(ReadProperty<bool>(ReadProperty<object>(owner, "Presenter"), "IsFullyCovered"),
                Is.True);
            Assert.That(
                markReady.Invoke(owner, new object[] { staleTicket, destinationScene }),
                Is.EqualTo(false),
                "A stale generation must not reveal the destination.");

            MethodInfo failOpen = RequireMethod(ownerType, "FailOpen");
            Assert.That(
                failOpen.Invoke(owner, new object[] { firstTicket, "Focused handoff test" }),
                Is.EqualTo(true));
            Assert.That(ReadProperty<bool>(owner, "HasActiveTicket"), Is.False);
            Assert.That(ReadProperty<bool>(gate, "GlobalRouteLocked"), Is.False);
            Assert.That(button.interactable, Is.True);
            Assert.That(dimGroup.alpha, Is.EqualTo(1f).Within(0.001f));
            Assert.That(eventSystem.enabled, Is.True);
            Assert.That(ReadProperty<bool>(ReadProperty<object>(owner, "Presenter"), "IsFullyCovered"),
                Is.False);

            object[] secondBeginArguments = { route, null, null };
            Assert.That(beginRoute.Invoke(owner, secondBeginArguments), Is.EqualTo(true));
            object secondTicket = secondBeginArguments[1];
            Assert.That(
                ReadProperty<uint>(secondTicket, "Generation"),
                Is.GreaterThan(firstGeneration),
                "A completed generation must never be reused.");
            Assert.That(
                failOpen.Invoke(owner, new object[] { secondTicket, "Focused handoff cleanup" }),
                Is.EqualTo(true));
        }

        [UnityTest]
        public IEnumerator ArrivalReceiverWaitsForReadinessAndReleasesExactlyOnce()
        {
            Type ownerType = RequireProductType(OwnerTypeName);
            Type receiverType = RequireProductType(ReceiverTypeName);
            Type routeType = RequireProductType(RouteTypeName);
            Type routeIdType = RequireProductType(RouteIdTypeName);

            GameObject overlay = UnityEngine.Object.Instantiate(RequirePrefab(TransitionOverlayPrefabPath));
            Component owner = overlay.GetComponentsInChildren(ownerType, true).Single();
            Component presenter = ReadProperty<Component>(owner, "Presenter");
            SetPrivateField(presenter, "defaultFadeSeconds", 0.01f);

            Scene scene = SceneManager.GetActiveScene();
            object route = CreateRoute(routeType, routeIdType, "Lobby", scene.name, string.Empty);
            MethodInfo beginRoute = RequireMethod(
                ownerType,
                "TryBeginRoute",
                method => method.GetParameters().Length == 3);
            object[] beginArguments = { route, null, null };
            Assert.That(beginRoute.Invoke(owner, beginArguments), Is.EqualTo(true));
            object ticket = beginArguments[1];
            Assert.That(
                RequireMethod(ownerType, "TryMarkActivationRequested")
                    .Invoke(owner, new[] { ticket }),
                Is.EqualTo(true));
            Assert.That(
                RequireMethod(ownerType, "TryMarkDestinationArrived")
                    .Invoke(owner, new object[] { ticket, scene }),
                Is.EqualTo(true));

            GameObject readinessRoot = new GameObject("UI Transition Handoff Test Readiness");
            readinessRoot.SetActive(false);
            UISceneTransitionReadinessProbe probe =
                readinessRoot.AddComponent<UISceneTransitionReadinessProbe>();
            Component receiver = readinessRoot.AddComponent(receiverType);
            readinessRoot.SetActive(true);

            yield return null;
            Assert.That(ReadProperty<bool>(receiver, "HasCrossedRenderLayoutBoundary"), Is.True);
            Assert.That(ReadProperty<bool>(receiver, "ReadySignalAttempted"), Is.False);
            Assert.That(ReadProperty<int>(receiver, "LastObservedReadinessSourceCount"), Is.EqualTo(1));

            probe.IsReady = true;
            float deadline = Time.realtimeSinceStartup + 2f;
            while (!ReadProperty<bool>(receiver, "ReadySignalAccepted"))
            {
                Assert.Less(Time.realtimeSinceStartup, deadline, "Arrival receiver did not accept readiness.");
                yield return null;
            }

            while (ReadProperty<bool>(owner, "HasActiveTicket"))
            {
                Assert.Less(Time.realtimeSinceStartup, deadline, "Destination reveal did not complete.");
                yield return null;
            }

            Assert.That(ReadProperty<int>(receiver, "ReadySignalAttemptCount"), Is.EqualTo(1));
            Assert.That(ReadProperty<bool>(presenter, "IsFullyCovered"), Is.False);
        }

        [UnityTest]
        public IEnumerator ArrivalReceiverReobservesSceneLoadedAfterActivationRaceExactlyOnce()
        {
            Type ownerType = RequireProductType(OwnerTypeName);
            Type receiverType = RequireProductType(ReceiverTypeName);
            Type routeType = RequireProductType(RouteTypeName);
            Type routeIdType = RequireProductType(RouteIdTypeName);

            GameObject readinessRoot = new GameObject("UI Transition Handoff Test Activation Race");
            readinessRoot.SetActive(false);
            UISceneTransitionReadinessProbe probe =
                readinessRoot.AddComponent<UISceneTransitionReadinessProbe>();
            Component receiver = readinessRoot.AddComponent(receiverType);
            readinessRoot.SetActive(true);
            yield return null;

            Assert.That(ReadProperty<uint>(receiver, "ObservedGeneration"), Is.EqualTo(0u),
                "The receiver must remain idle when activation has not issued a ticket yet.");

            GameObject overlay = UnityEngine.Object.Instantiate(RequirePrefab(TransitionOverlayPrefabPath));
            Component owner = overlay.GetComponentsInChildren(ownerType, true).Single();
            Component presenter = ReadProperty<Component>(owner, "Presenter");
            SetPrivateField(presenter, "defaultFadeSeconds", 0.01f);
            yield return null;

            Scene scene = SceneManager.GetActiveScene();
            object route = CreateRoute(routeType, routeIdType, "Lobby", scene.name, string.Empty);
            MethodInfo beginRoute = RequireMethod(
                ownerType,
                "TryBeginRoute",
                method => method.GetParameters().Length == 3);
            object[] beginArguments = { route, null, null };
            Assert.That(beginRoute.Invoke(owner, beginArguments), Is.EqualTo(true));
            object ticket = beginArguments[1];
            Assert.That(
                RequireMethod(ownerType, "TryMarkActivationRequested")
                    .Invoke(owner, new[] { ticket }),
                Is.EqualTo(true));
            Assert.That(
                RequireMethod(ownerType, "TryMarkDestinationArrived")
                    .Invoke(owner, new object[] { ticket, scene }),
                Is.EqualTo(true));

            MethodInfo handleSceneLoaded = RequireMethod(
                receiverType,
                "HandleSceneLoaded",
                method => method.GetParameters().Length == 2);
            handleSceneLoaded.Invoke(receiver, new object[] { scene, LoadSceneMode.Single });
            handleSceneLoaded.Invoke(receiver, new object[] { scene, LoadSceneMode.Single });

            yield return null;
            Assert.That(
                ReadProperty<uint>(receiver, "ObservedGeneration"),
                Is.EqualTo(ReadProperty<uint>(ticket, "Generation")),
                "The sceneLoaded callback must re-observe the ticket missed during activation.");
            Assert.That(ReadProperty<bool>(receiver, "HasCrossedRenderLayoutBoundary"), Is.True);
            Assert.That(ReadProperty<bool>(receiver, "ReadySignalAttempted"), Is.False);
            Assert.That(ReadProperty<int>(receiver, "LastObservedReadinessSourceCount"), Is.EqualTo(1));

            probe.IsReady = true;
            float deadline = Time.realtimeSinceStartup + 2f;
            while (!ReadProperty<bool>(receiver, "ReadySignalAccepted"))
            {
                Assert.Less(Time.realtimeSinceStartup, deadline,
                    "The re-observed activation did not accept destination readiness.");
                yield return null;
            }

            while (ReadProperty<bool>(owner, "HasActiveTicket"))
            {
                Assert.Less(Time.realtimeSinceStartup, deadline,
                    "The re-observed destination reveal did not complete.");
                yield return null;
            }

            handleSceneLoaded.Invoke(receiver, new object[] { scene, LoadSceneMode.Single });
            yield return null;
            Assert.That(ReadProperty<int>(receiver, "ReadySignalAttemptCount"), Is.EqualTo(1),
                "Duplicate sceneLoaded notifications must not emit duplicate ready signals.");
            Assert.That(ReadProperty<bool>(presenter, "IsFullyCovered"), Is.False);
        }

        [UnityTest]
        public IEnumerator TerminalHandoffServiceCoversBeforeDispatchAndRestoresFailure()
        {
            Type ownerType = RequireProductType(OwnerTypeName);
            GameObject overlay = UnityEngine.Object.Instantiate(RequirePrefab(TransitionOverlayPrefabPath));
            Component owner = overlay.GetComponentsInChildren(ownerType, true).Single();
            Component presenter = ReadProperty<Component>(owner, "Presenter");
            SetPrivateField(presenter, "defaultFadeSeconds", 0.01f);

            Scene scene = SceneManager.GetActiveScene();
            int dispatchCount = 0;
            int failureCount = 0;
            bool coveredAtDispatch = false;
            string observedFailure = string.Empty;
            bool began = UITransitionHandoffService.TryBeginTerminalHandoff(
                new UITransitionHandoffDestination(
                    UITransitionDestinationKind.Lobby,
                    scene.name,
                    scene.path),
                () =>
                {
                    dispatchCount++;
                    coveredAtDispatch = ReadProperty<bool>(presenter, "IsFullyCovered");
                    return UITransitionDispatchResult.Failure("Expected focused dispatch failure.");
                },
                error =>
                {
                    failureCount++;
                    observedFailure = error;
                },
                out string beginError);
            Assert.That(began, Is.True, beginError);

            float deadline = Time.realtimeSinceStartup + 2f;
            while (failureCount == 0)
            {
                Assert.Less(Time.realtimeSinceStartup, deadline, "Terminal handoff failure did not restore.");
                yield return null;
            }

            Assert.That(dispatchCount, Is.EqualTo(1));
            Assert.That(failureCount, Is.EqualTo(1));
            Assert.That(coveredAtDispatch, Is.True);
            Assert.That(observedFailure, Is.EqualTo("Expected focused dispatch failure."));
            Assert.That(ReadProperty<bool>(owner, "HasActiveTicket"), Is.False);
            Assert.That(ReadProperty<bool>(presenter, "IsFullyCovered"), Is.False);
        }

        private static void AssertReceiverCount(string prefabPath, Type receiverType)
        {
            Component[] receivers = RequirePrefab(prefabPath)
                .GetComponentsInChildren(receiverType, true);
            Assert.That(receivers, Has.Length.EqualTo(1),
                $"{prefabPath} must contain exactly one destination readiness receiver.");
        }

        private static GameObject RequirePrefab(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, $"Missing prefab: {path}");
            return prefab;
        }

        private static object CreateRoute(
            Type routeType,
            Type routeIdType,
            string routeIdName,
            string sceneName,
            string scenePath)
        {
            object routeId = Enum.Parse(routeIdType, routeIdName);
            ConstructorInfo constructor = routeType.GetConstructor(new[]
            {
                routeIdType,
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(bool),
                typeof(float)
            });
            Assert.That(constructor, Is.Not.Null, "Missing UIScreenRouteTable.Route constructor.");
            return constructor.Invoke(new[]
            {
                routeId,
                sceneName,
                scenePath,
                string.Empty,
                string.Empty,
                false,
                0f
            });
        }

        private static object CreateTicket(Type ticketType, object template, uint generation)
        {
            ConstructorInfo constructor = ticketType.GetConstructors(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Single(info => info.GetParameters().Length == 6);
            return constructor.Invoke(new object[]
            {
                ReadProperty<int>(template, "OwnerInstanceId"),
                generation,
                ReadProperty<object>(template, "RouteId"),
                ReadProperty<int>(template, "SourceSceneHandle"),
                ReadProperty<string>(template, "DestinationSceneName"),
                ReadProperty<string>(template, "DestinationScenePath")
            });
        }

        private static Type RequireProductType(string fullName)
        {
            Type type = Type.GetType(fullName + ", DimensionBrawl.Runtime")
                ?? Type.GetType(fullName + ", Assembly-CSharp");
            Assert.That(type, Is.Not.Null, $"Missing product type {fullName}.");
            return type;
        }

        private static MethodInfo RequireMethod(
            Type type,
            string methodName,
            Func<MethodInfo, bool> predicate = null)
        {
            MethodInfo method = type.GetMethods(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(candidate => candidate.Name == methodName
                    && (predicate == null || predicate(candidate)));
            Assert.That(method, Is.Not.Null, $"Missing method {type.Name}.{methodName}.");
            return method;
        }

        private static T ReadProperty<T>(object target, string propertyName)
        {
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null,
                $"Missing property {target.GetType().Name}.{propertyName}.");
            return (T)property.GetValue(target);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null,
                $"Missing field {target.GetType().Name}.{fieldName}.");
            field.SetValue(target, value);
        }

        private static void TryResetProductType(string fullName)
        {
            Type type = Type.GetType(fullName + ", DimensionBrawl.Runtime")
                ?? Type.GetType(fullName + ", Assembly-CSharp");
            MethodInfo reset = type?.GetMethod(
                "ResetForTests",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            reset?.Invoke(null, null);
        }
    }

    public sealed class UISceneTransitionReadinessProbe : MonoBehaviour, IUISceneTransitionReadinessSource
    {
        public bool IsReady { get; set; }
        public bool IsSceneTransitionReady => IsReady;
    }
}
