using System;
using System.Collections;
using System.Reflection;
using DimensionBrawl.LevelDesign;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class StageSelectExactBindingPlayModeTests
    {
        private const string RouteTablePath =
            "Assets/_Game/DesignData/UI/DB_UIRouteTable.asset";
        private const string ProductionStageCatalogPath =
            "Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset";
        private const string ProductionStageSelectPrefabPath =
            "Assets/_Game/UI/StageSelect/PF_UI_StageSelectScreen.prefab";
        private const string PlayableStagePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/StageDefinitions/DB_PlayableStage_OlympusInvasion.asset";
        private const string CorridorScenePath =
            "Assets/_Game/Scenes/OlympusCorridorInvasionStage.unity";
        private const string StageClearScenePath =
            "Assets/_Game/Scenes/UI/UI_StageClear.unity";
        private const string ProductionTrainingStageId = "story_v1_training_route";
        private const string ProductionCourtyardStageId =
            "story_v1_courtyard_drill_route";
        private const string StageAId = "exact-binding-a";
        private const string StageBId = "exact-binding-b";
        private const string StageALoadingCardId = "exact-binding-a-loading";
        private const string StageBLoadingCardId = "exact-binding-b-loading";
        private const int CombatRouteId = 40;

        [SetUp]
        public void PrepareTransitionRuntime()
        {
            TryResetUiTransitionRuntime("DimensionBrawl.UI.UISceneTransitionArrivalReceiver");
            TryResetUiTransitionRuntime("DimensionBrawl.UI.UISceneTransitionHandoffOwner");
            TryResetUiTransitionRuntime("DimensionBrawl.UI.UITransitionHandoffService");
        }

        [TearDown]
        public void ReleaseTransitionRuntime()
        {
            TryResetUiTransitionRuntime("DimensionBrawl.UI.UISceneTransitionArrivalReceiver");
            TryResetUiTransitionRuntime("DimensionBrawl.UI.UISceneTransitionHandoffOwner");
            TryResetUiTransitionRuntime("DimensionBrawl.UI.UITransitionHandoffService");
        }

        [UnityTest]
        public IEnumerator ProductionStageSelectKeepsCourtyardCardQuarantined()
        {
            ScriptableObject catalog = LoadRequired<ScriptableObject>(
                ProductionStageCatalogPath);
            GameObject prefab = LoadRequired<GameObject>(ProductionStageSelectPrefabPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            Assert.That(instance, Is.Not.Null);
            instance.hideFlags = HideFlags.HideAndDontSave;

            try
            {
                instance.SetActive(false);
                Component presenter = instance.GetComponentInChildren(
                    RequireProductType("DimensionBrawl.UI.StageSelectScreenPresenter"),
                    true);
                Assert.That(presenter, Is.Not.Null);
                SetPrivateField(presenter, "focusSelectedStageOnEnable", false);
                SetPrivateField(presenter, "backWithEscape", false);

                Assert.That(ReadProperty(catalog, "ProjectionSchemaVersion"), Is.EqualTo(1));
                Assert.That(ReadProperty(catalog, "CatalogProjectionGeneration"), Is.EqualTo(2));
                Assert.That(ReadProperty(catalog, "StageCount"), Is.EqualTo(1));
                Assert.That(
                    ReadPrivateField<ScriptableObject>(presenter, "stageCatalog"),
                    Is.SameAs(catalog));
                Assert.That(
                    ReadPrivateField<string>(presenter, "selectedStageId"),
                    Is.EqualTo(ProductionTrainingStageId));

                object[] quarantinedArguments = { ProductionCourtyardStageId, null };
                Assert.That(
                    (bool)RequireMethod(catalog.GetType(), "TryGetStage").Invoke(
                        catalog,
                        quarantinedArguments),
                    Is.False);
                Assert.That(ReadProperty(quarantinedArguments[1], "Id"), Is.Null);

                Array focusEntries = ReadPrivateField<Array>(presenter, "stageFocusEntries");
                Assert.That(focusEntries.Length, Is.EqualTo(1));
                Button trainingButton = AssertProductionBinding(
                    focusEntries.GetValue(0),
                    ProductionTrainingStageId,
                    "01-1_StageCard");

                Transform courtyardShell = FindRequiredDescendant(
                    instance.transform,
                    "01-2_StageCard");
                Button courtyardButton = courtyardShell.GetComponent<Button>();
                Assert.That(courtyardShell.gameObject.activeSelf, Is.False);
                Assert.That(courtyardButton, Is.Not.Null);
                Assert.That(courtyardButton.interactable, Is.False);
                Assert.That(courtyardButton.targetGraphic, Is.Not.Null);
                Assert.That(courtyardButton.targetGraphic.raycastTarget, Is.False);
                CanvasGroup courtyardCanvasGroup = courtyardShell.GetComponent<CanvasGroup>();
                if (courtyardCanvasGroup != null)
                {
                    Assert.That(courtyardCanvasGroup.interactable, Is.False);
                    Assert.That(courtyardCanvasGroup.blocksRaycasts, Is.False);
                }

                Button startButton = ReadPrivateField<Button>(presenter, "startButton");
                Button backButton = ReadPrivateField<Button>(presenter, "backButton");
                AssertProductionRouteGateMembership(
                    instance.transform,
                    backButton,
                    startButton,
                    trainingButton);
                Assert.That(GetRuntimeListenerCount(trainingButton.onClick), Is.Zero);
                Assert.That(GetRuntimeListenerCount(courtyardButton.onClick), Is.Zero);
                Assert.That(GetRuntimeListenerCount(startButton.onClick), Is.Zero);

                instance.SetActive(true);
                yield return null;

                Assert.That(GetRuntimeListenerCount(trainingButton.onClick), Is.EqualTo(1));
                Assert.That(GetRuntimeListenerCount(courtyardButton.onClick), Is.Zero);
                Assert.That(GetRuntimeListenerCount(startButton.onClick), Is.EqualTo(1));
                object projection = ReadProperty(presenter, "SelectedRouteProjection");
                Assert.That(projection, Is.Not.Null);
                Assert.That(ReadProperty(projection, "CatalogEntryId"),
                    Is.EqualTo(ProductionTrainingStageId));
                Assert.That(ReadProperty(projection, "EntryScenePath"),
                    Is.EqualTo(CorridorScenePath));
                Assert.That(
                    ReadProperty(presenter, "SelectedRouteRejectReason").ToString(),
                    Is.EqualTo("None"));

                instance.SetActive(false);
                Assert.That(GetRuntimeListenerCount(trainingButton.onClick), Is.Zero);
                Assert.That(GetRuntimeListenerCount(courtyardButton.onClick), Is.Zero);
                Assert.That(GetRuntimeListenerCount(startButton.onClick), Is.Zero);

                instance.SetActive(true);
                yield return null;
                Assert.That(GetRuntimeListenerCount(trainingButton.onClick), Is.EqualTo(1));
                Assert.That(GetRuntimeListenerCount(courtyardButton.onClick), Is.Zero);
                Assert.That(GetRuntimeListenerCount(startButton.onClick), Is.EqualTo(1));
                Assert.That(
                    ReadProperty(
                        ReadProperty(presenter, "SelectedRouteProjection"),
                        "CatalogEntryId"),
                    Is.EqualTo(ProductionTrainingStageId));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [UnityTest]
        public IEnumerator ExactTwoRowBindingSelectsAndDispatchesOnlyTheClickedStageAfterReenable()
        {
            StageDefinitionProfile definitionA = CreateStageDefinition(
                "EXACT-BINDING-A-SEGMENT",
                CorridorScenePath);
            StageDefinitionProfile definitionB = CreateStageDefinition(
                "EXACT-BINDING-B-SEGMENT",
                StageClearScenePath);
            PlayableStageDefinition routeA = CreatePlayableStageDefinition(definitionA);
            PlayableStageDefinition routeB = CreatePlayableStageDefinition(definitionB);
            ScriptableObject catalog = CreateStageCatalog(
                (StageAId, StageALoadingCardId, routeA),
                (StageBId, StageBLoadingCardId, routeB));
            ScriptableObject loadingCardDeck = CreateLoadingCardDeck();
            AudioClip startClip = AudioClip.Create(
                "ExactBindingAcceptedStart",
                64,
                1,
                44100,
                false);
            var root = new GameObject("Exact stage-card binding success fixture");
            root.SetActive(false);

            try
            {
                Assert.That(Convert.ToInt32(ReadProperty(catalog, "StageCount")), Is.EqualTo(2));
                Assert.That(catalog.hideFlags, Is.EqualTo(HideFlags.HideAndDontSave));
                Assert.That(routeA, Is.Not.SameAs(routeB));
                Assert.That(routeA.hideFlags, Is.EqualTo(HideFlags.HideAndDontSave));
                Assert.That(routeB.hideFlags, Is.EqualTo(HideFlags.HideAndDontSave));
                Component router = root.AddComponent(
                    RequireProductType("DimensionBrawl.UI.UISceneFlowRouter"));
                Component presenter = root.AddComponent(
                    RequireProductType("DimensionBrawl.UI.StageSelectScreenPresenter"));
                Button startButton = CreateButton(root.transform, "Start");
                Button stageAButton = CreateButton(root.transform, "Stage A");
                Button stageBButton = CreateButton(root.transform, "Stage B");
                Text shownLoadingCardId = CreateLoadingRouteCapture(
                    root.transform,
                    router,
                    loadingCardDeck);

                SetPrivateField(
                    router,
                    "routeTable",
                    LoadRequired<ScriptableObject>(RouteTablePath));
                ConfigurePresenter(
                    presenter,
                    catalog,
                    router,
                    startButton,
                    startClip,
                    CreateFocusEntries(
                        (StageAId, stageAButton, stageAButton.GetComponent<RectTransform>()),
                        (StageBId, stageBButton, stageBButton.GetComponent<RectTransform>())));

                int startEventCount = 0;
                ReadPrivateField<UnityEvent>(presenter, "startRequested")
                    .AddListener(() => startEventCount++);

                root.SetActive(true);
                Assert.That(GetRuntimeListenerCount(stageAButton.onClick), Is.EqualTo(1));
                Assert.That(GetRuntimeListenerCount(stageBButton.onClick), Is.EqualTo(1));
                Assert.That(GetRuntimeListenerCount(startButton.onClick), Is.EqualTo(1));

                stageBButton.onClick.Invoke();
                AssertSelectedProjection(
                    presenter,
                    routeB,
                    StageBId,
                    "UI_StageClear",
                    StageClearScenePath,
                    StageBLoadingCardId);

                root.SetActive(false);
                Assert.That(GetRuntimeListenerCount(stageAButton.onClick), Is.Zero);
                Assert.That(GetRuntimeListenerCount(stageBButton.onClick), Is.Zero);
                Assert.That(GetRuntimeListenerCount(startButton.onClick), Is.Zero);

                root.SetActive(true);
                Assert.That(GetRuntimeListenerCount(stageAButton.onClick), Is.EqualTo(1));
                Assert.That(GetRuntimeListenerCount(stageBButton.onClick), Is.EqualTo(1));
                Assert.That(GetRuntimeListenerCount(startButton.onClick), Is.EqualTo(1));

                stageBButton.onClick.Invoke();
                AssertSelectedProjection(
                    presenter,
                    routeB,
                    StageBId,
                    "UI_StageClear",
                    StageClearScenePath,
                    StageBLoadingCardId);

                startButton.onClick.Invoke();
                Assert.That(startEventCount, Is.EqualTo(1));
                Assert.That(Convert.ToInt32(ReadProperty(router, "RouteRequestCount")), Is.EqualTo(1));
                Assert.That(Convert.ToBoolean(ReadProperty(presenter, "HasAcceptedStartRequest")), Is.True);
                Assert.That(root.GetComponent<AudioSource>(), Is.Not.Null);

                for (int frame = 0;
                     frame < 8
                     && !string.Equals(
                         shownLoadingCardId.text,
                         StageBLoadingCardId,
                         StringComparison.Ordinal);
                     frame++)
                {
                    yield return null;
                }

                Assert.That(shownLoadingCardId.text, Is.EqualTo(StageBLoadingCardId));
                Assert.That(shownLoadingCardId.text, Is.Not.EqualTo(StageALoadingCardId));
                object currentState = ReadProperty(router, "CurrentState");
                Assert.That(Convert.ToInt32(ReadProperty(currentState, "RouteId")), Is.EqualTo(CombatRouteId));
                Assert.That(ReadProperty(currentState, "SceneName"), Is.EqualTo("UI_StageClear"));

                startButton.onClick.Invoke();
                Assert.That(startEventCount, Is.EqualTo(1));
                Assert.That(Convert.ToInt32(ReadProperty(router, "RouteRequestCount")), Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(startClip);
                UnityEngine.Object.DestroyImmediate(loadingCardDeck);
                UnityEngine.Object.DestroyImmediate(catalog);
                DestroyPlayableStageDefinition(routeB);
                DestroyPlayableStageDefinition(routeA);
                UnityEngine.Object.DestroyImmediate(definitionB);
                UnityEngine.Object.DestroyImmediate(definitionA);
            }
        }

        [Test]
        public void InvalidExactBindingMatrixFailsClosedWithoutRouteEventOrSfx()
        {
            StageDefinitionProfile definitionA = CreateStageDefinition(
                "INVALID-BINDING-A-SEGMENT",
                CorridorScenePath);
            StageDefinitionProfile definitionB = CreateStageDefinition(
                "INVALID-BINDING-B-SEGMENT",
                StageClearScenePath);
            PlayableStageDefinition routeA = CreatePlayableStageDefinition(definitionA);
            PlayableStageDefinition routeB = CreatePlayableStageDefinition(definitionB);
            ScriptableObject catalog = CreateStageCatalog(
                (StageAId, StageALoadingCardId, routeA),
                (StageBId, StageBLoadingCardId, routeB));
            AudioClip startClip = AudioClip.Create(
                "InvalidExactBindingStart",
                64,
                1,
                44100,
                false);

            try
            {
                foreach (InvalidBindingKind kind in Enum.GetValues(typeof(InvalidBindingKind)))
                {
                    AssertInvalidBindingFailsClosed(kind, catalog, startClip);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(startClip);
                UnityEngine.Object.DestroyImmediate(catalog);
                DestroyPlayableStageDefinition(routeB);
                DestroyPlayableStageDefinition(routeA);
                UnityEngine.Object.DestroyImmediate(definitionB);
                UnityEngine.Object.DestroyImmediate(definitionA);
            }
        }

        private static void AssertInvalidBindingFailsClosed(
            InvalidBindingKind kind,
            ScriptableObject catalog,
            AudioClip startClip)
        {
            var root = new GameObject($"Invalid exact binding fixture: {kind}");
            root.SetActive(false);

            try
            {
                Component router = root.AddComponent(
                    RequireProductType("DimensionBrawl.UI.UISceneFlowRouter"));
                Component presenter = root.AddComponent(
                    RequireProductType("DimensionBrawl.UI.StageSelectScreenPresenter"));
                Button startButton = CreateButton(root.transform, "Start");
                Button stageAButton = CreateButton(root.transform, "Stage A");
                Button stageBButton = CreateButton(root.transform, "Stage B");
                RectTransform detachedTarget = CreateButton(root.transform, "Detached target")
                    .GetComponent<RectTransform>();

                Array focusEntries = kind switch
                {
                    InvalidBindingKind.MissingCatalogRow => CreateFocusEntries(
                        (StageAId, stageAButton, stageAButton.GetComponent<RectTransform>())),
                    InvalidBindingKind.UnknownStageId => CreateFocusEntries(
                        (StageAId, stageAButton, stageAButton.GetComponent<RectTransform>()),
                        ("unknown-stage", stageBButton, stageBButton.GetComponent<RectTransform>())),
                    InvalidBindingKind.DuplicateStageId => CreateFocusEntries(
                        (StageAId, stageAButton, stageAButton.GetComponent<RectTransform>()),
                        (StageAId, stageBButton, stageBButton.GetComponent<RectTransform>())),
                    InvalidBindingKind.SharedButton => CreateFocusEntries(
                        (StageAId, stageAButton, stageAButton.GetComponent<RectTransform>()),
                        (StageBId, stageAButton, stageAButton.GetComponent<RectTransform>())),
                    InvalidBindingKind.NullButton => CreateFocusEntries(
                        (StageAId, stageAButton, stageAButton.GetComponent<RectTransform>()),
                        (StageBId, null, stageBButton.GetComponent<RectTransform>())),
                    InvalidBindingKind.ButtonTargetMismatch => CreateFocusEntries(
                        (StageAId, stageAButton, stageAButton.GetComponent<RectTransform>()),
                        (StageBId, stageBButton, detachedTarget)),
                    _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
                };

                SetPrivateField(
                    router,
                    "routeTable",
                    LoadRequired<ScriptableObject>(RouteTablePath));
                ConfigurePresenter(
                    presenter,
                    catalog,
                    router,
                    startButton,
                    startClip,
                    focusEntries);

                int startEventCount = 0;
                ReadPrivateField<UnityEvent>(presenter, "startRequested")
                    .AddListener(() => startEventCount++);

                root.SetActive(true);
                Assert.That(
                    ReadProperty(presenter, "SelectedRouteRejectReason").ToString(),
                    Is.EqualTo("InvalidStageSelectionBindings"),
                    kind.ToString());
                Assert.That(
                    Convert.ToBoolean(ReadProperty(presenter, "HasSelectedRouteProjection")),
                    Is.False,
                    kind.ToString());
                Assert.That(startButton.interactable, Is.False, kind.ToString());
                Assert.That(GetRuntimeListenerCount(stageAButton.onClick), Is.Zero, kind.ToString());
                Assert.That(GetRuntimeListenerCount(stageBButton.onClick), Is.Zero, kind.ToString());

                stageAButton.onClick.Invoke();
                stageBButton.onClick.Invoke();
                startButton.onClick.Invoke();
                RequireMethod(presenter.GetType(), "HandleStartClicked").Invoke(presenter, null);

                Assert.That(
                    Convert.ToInt32(ReadProperty(router, "RouteRequestCount")),
                    Is.Zero,
                    kind.ToString());
                Assert.That(startEventCount, Is.Zero, kind.ToString());
                Assert.That(
                    Convert.ToBoolean(ReadProperty(presenter, "HasAcceptedStartRequest")),
                    Is.False,
                    kind.ToString());
                Assert.That(
                    Convert.ToBoolean(ReadProperty(presenter, "HasSelectedRouteProjection")),
                    Is.False,
                    kind.ToString());
                Assert.That(
                    ReadProperty(presenter, "SelectedRouteRejectReason").ToString(),
                    Is.EqualTo("InvalidStageSelectionBindings"),
                    kind.ToString());
                Assert.That(root.GetComponent<AudioSource>(), Is.Null, kind.ToString());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void ConfigurePresenter(
            Component presenter,
            ScriptableObject catalog,
            Component router,
            Button startButton,
            AudioClip startClip,
            Array focusEntries)
        {
            SetPrivateField(presenter, "stageCatalog", catalog);
            SetPrivateField(presenter, "selectedStageId", StageAId);
            SetPrivateField(presenter, "router", router);
            SetPrivateField(presenter, "startButton", startButton);
            SetPrivateField(presenter, "startButtonSfx", startClip);
            SetPrivateField(presenter, "stageFocusEntries", focusEntries);
            SetPrivateField(presenter, "requireExactStageCardBindings", true);
            SetPrivateField(presenter, "focusSelectedStageOnEnable", false);
            SetPrivateField(presenter, "backWithEscape", false);
        }

        private static Button AssertProductionBinding(
            object focusEntry,
            string expectedStageId,
            string expectedCardName)
        {
            Assert.That(focusEntry, Is.Not.Null);
            Assert.That(ReadProperty(focusEntry, "StageId"), Is.EqualTo(expectedStageId));
            var button = (Button)ReadProperty(focusEntry, "SelectionButton");
            var target = (RectTransform)ReadProperty(focusEntry, "StageTarget");
            Assert.That(button, Is.Not.Null);
            Assert.That(target, Is.Not.Null);
            Assert.That(target, Is.SameAs(button.transform));
            Assert.That(target.name, Is.EqualTo(expectedCardName));
            Assert.That(target.gameObject.activeSelf, Is.True);
            Assert.That(button.interactable, Is.True);
            Assert.That(button.targetGraphic, Is.Not.Null);
            Assert.That(button.targetGraphic.raycastTarget, Is.True);
            return button;
        }

        private static void AssertReadableStageCardText(
            Transform card,
            string objectName,
            int minimumSize,
            int maximumSize)
        {
            Text text = FindRequiredDescendant(card, objectName).GetComponent<Text>();
            Assert.That(text, Is.Not.Null, objectName);
            Assert.That(text.gameObject.activeSelf, Is.True, objectName);
            Assert.That(text.enabled, Is.True, objectName);
            Assert.That(text.color.a, Is.GreaterThan(0.01f), objectName);
            Assert.That(text.resizeTextForBestFit, Is.True, objectName);
            Assert.That(text.resizeTextMinSize, Is.EqualTo(minimumSize), objectName);
            Assert.That(text.resizeTextMaxSize, Is.EqualTo(maximumSize), objectName);
            Assert.That(text.raycastTarget, Is.False, objectName);
            Assert.That(text.GetComponent<Outline>(), Is.Not.Null, objectName);
        }

        private static void AssertGlobalProductDetailTruth(Transform root)
        {
            AssertHiddenProductDetailObject(
                root,
                "ChapterProgressLabel",
                requireEmptyText: true);
            AssertHiddenProductDetailObject(
                root,
                "ChapterPercentText",
                requireEmptyText: true);
            AssertHiddenProductDetailObject(
                root,
                "ChapterProgress",
                requireEmptyText: false);
            AssertHiddenProductDetailObject(
                root,
                "ChapterProgressBackground",
                requireEmptyText: false);
            AssertHiddenProductDetailObject(
                root,
                "SummaryFrame",
                requireEmptyText: false);
            AssertHiddenProductDetailObject(
                root,
                "SummaryText",
                requireEmptyText: true);
        }

        private static void AssertProductionChapterInventory(Transform root)
        {
            Transform selected = FindRequiredDescendant(
                root,
                "EP 01_SelectedChapterCard");
            Button selectedButton = selected.GetComponent<Button>();
            Assert.That(selected.gameObject.activeSelf, Is.True);
            Assert.That(selectedButton, Is.Not.Null);
            Assert.That(selectedButton.enabled, Is.True);
            Assert.That(selectedButton.interactable, Is.False);
            Assert.That(selectedButton.targetGraphic, Is.Not.Null);
            Assert.That(selectedButton.targetGraphic.raycastTarget, Is.False);
            Assert.That(
                FindRequiredDescendant(selected, "EpisodeText").GetComponent<Text>().text,
                Is.EqualTo("EP 01"));
            Assert.That(
                FindRequiredDescendant(selected, "TitleText").GetComponent<Text>().text,
                Is.EqualTo("차원 안정화"));
            Transform percent = FindRequiredDescendant(selected, "PercentText");
            Assert.That(percent.gameObject.activeSelf, Is.False);
            Assert.That(percent.GetComponent<Text>().text, Is.Empty);

            string[] placeholders =
            {
                "EP 02_ChapterCard",
                "EP 03_ChapterCard",
                "EP 04_ChapterCard"
            };
            for (int i = 0; i < placeholders.Length; i++)
            {
                Transform placeholder = FindRequiredDescendant(root, placeholders[i]);
                Button button = placeholder.GetComponent<Button>();
                Assert.That(placeholder.gameObject.activeSelf, Is.False, placeholders[i]);
                Assert.That(button, Is.Not.Null, placeholders[i]);
                Assert.That(button.interactable, Is.False, placeholders[i]);
                if (button.targetGraphic != null)
                {
                    Assert.That(button.targetGraphic.raycastTarget, Is.False, placeholders[i]);
                }

                CanvasGroup canvasGroup = placeholder.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    Assert.That(canvasGroup.interactable, Is.False, placeholders[i]);
                    Assert.That(canvasGroup.blocksRaycasts, Is.False, placeholders[i]);
                }
            }
        }

        private static void AssertProductionRouteGateMembership(
            Transform root,
            Button backButton,
            Button startButton,
            Button trainingButton)
        {
            Component[] gates = root.GetComponentsInChildren(
                RequireProductType("DimensionBrawl.UI.UIRouteInteractableGate"),
                true);
            Assert.That(gates.Length, Is.EqualTo(1));
            Selectable[] selectables = ReadPrivateField<Selectable[]>(gates[0], "selectables");
            Assert.That(selectables, Is.Not.Null);
            Assert.That(
                selectables,
                Is.EquivalentTo(new Selectable[]
                {
                    backButton,
                    startButton,
                    trainingButton
                }));
        }

        private static void AssertHiddenProductDetailObject(
            Transform root,
            string objectName,
            bool requireEmptyText)
        {
            Transform target = FindRequiredDescendant(root, objectName);
            Assert.That(target.gameObject.activeSelf, Is.False, objectName);
            if (!requireEmptyText)
            {
                return;
            }

            Text text = target.GetComponent<Text>();
            Assert.That(text, Is.Not.Null, objectName);
            Assert.That(text.text, Is.Empty, objectName);
        }

        private static void AssertProductionStartButtonVisualTruth(Button startButton)
        {
            Assert.That(startButton, Is.Not.Null);
            Assert.That(startButton.name, Is.EqualTo("StartButton"));
            Assert.That(startButton.gameObject.activeSelf, Is.True);
            Assert.That(startButton.enabled, Is.True);
            Assert.That(startButton.interactable, Is.True);
            Assert.That(startButton.targetGraphic, Is.Not.Null);
            Assert.That(startButton.targetGraphic.raycastTarget, Is.True);

            CanvasGroup canvasGroup = startButton.GetComponent<CanvasGroup>();
            Assert.That(canvasGroup, Is.Not.Null);
            Assert.That(canvasGroup.alpha, Is.EqualTo(1f).Within(0.001f));
            Assert.That(canvasGroup.interactable, Is.True);
            Assert.That(canvasGroup.blocksRaycasts, Is.True);

            Transform frame = FindRequiredDescendant(startButton.transform, "Frame");
            Graphic frameGraphic = frame.GetComponent<Graphic>();
            Assert.That(frame.gameObject.activeSelf, Is.True);
            Assert.That(frameGraphic, Is.Not.Null);
            Assert.That(frameGraphic.color.a, Is.GreaterThan(0.01f));

            Text label = FindRequiredDescendant(startButton.transform, "StageStartText")
                .GetComponent<Text>();
            Assert.That(label, Is.Not.Null);
            Assert.That(label.gameObject.activeSelf, Is.True);
            Assert.That(label.text, Is.EqualTo("작전 시작"));
            Assert.That(label.resizeTextForBestFit, Is.True);
            Assert.That(label.resizeTextMinSize, Is.EqualTo(20));
            Assert.That(label.resizeTextMaxSize, Is.EqualTo(30));
            Assert.That(label.raycastTarget, Is.False);
        }

        private static Transform FindRequiredDescendant(Transform root, string name)
        {
            Transform match = null;
            Transform[] descendants = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < descendants.Length; i++)
            {
                if (!string.Equals(descendants[i].name, name, StringComparison.Ordinal))
                {
                    continue;
                }

                Assert.That(match, Is.Null, $"Duplicate descendant {name} under {root.name}.");
                match = descendants[i];
            }

            Assert.That(match, Is.Not.Null, $"Missing descendant {name} under {root.name}.");
            return match;
        }

        private static void AssertOptionalPresenterTextIsEmpty(
            object presenter,
            string fieldName)
        {
            Text target = ReadPrivateFieldAllowNull<Text>(presenter, fieldName);
            if (target == null)
            {
                return;
            }

            Assert.That(target.text, Is.Empty, fieldName);
            Assert.That(target.gameObject.activeSelf, Is.False, fieldName);
        }

        private static void AssertSelectedProjection(
            Component presenter,
            PlayableStageDefinition expectedRoute,
            string expectedCatalogId,
            string expectedSceneName,
            string expectedScenePath,
            string expectedLoadingCardId)
        {
            object projection = ReadProperty(presenter, "SelectedRouteProjection");
            Assert.That(projection, Is.Not.Null);
            Assert.That(ReadProperty(projection, "CatalogEntryId"), Is.EqualTo(expectedCatalogId));
            Assert.That(ReadProperty(projection, "PlayableStage"), Is.SameAs(expectedRoute));
            Assert.That(ReadProperty(projection, "EntrySceneName"), Is.EqualTo(expectedSceneName));
            Assert.That(ReadProperty(projection, "EntryScenePath"), Is.EqualTo(expectedScenePath));
            Assert.That(ReadProperty(projection, "LoadingCardId"), Is.EqualTo(expectedLoadingCardId));
            Assert.That(
                ReadProperty(presenter, "SelectedRouteRejectReason").ToString(),
                Is.EqualTo("None"));
        }

        private static Array CreateFocusEntries(
            params (string StageId, Button Button, RectTransform Target)[] bindings)
        {
            Type entryType = RequireProductType(
                "DimensionBrawl.UI.StageSelectScreenPresenter").GetNestedType(
                "StageFocusEntry",
                BindingFlags.NonPublic);
            Assert.That(entryType, Is.Not.Null);
            Array entries = Array.CreateInstance(entryType, bindings.Length);
            for (int i = 0; i < bindings.Length; i++)
            {
                object entry = Activator.CreateInstance(entryType);
                SetPrivateField(entry, "stageId", bindings[i].StageId);
                SetPrivateField(entry, "selectionButton", bindings[i].Button);
                SetPrivateField(entry, "stageTarget", bindings[i].Target);
                SetPrivateField(entry, "chapterTarget", null);
                entries.SetValue(entry, i);
            }

            return entries;
        }

        private static Button CreateButton(Transform parent, string name)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            return buttonObject.GetComponent<Button>();
        }

        private static Text CreateLoadingRouteCapture(
            Transform parent,
            Component router,
            ScriptableObject deck)
        {
            var captureObject = new GameObject(
                "Loading route capture",
                typeof(RectTransform),
                typeof(CanvasGroup));
            captureObject.transform.SetParent(parent, false);
            var idObject = new GameObject(
                "Shown loading card id",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            idObject.transform.SetParent(captureObject.transform, false);
            Text idText = idObject.GetComponent<Text>();
            Component cardPresenter = captureObject.AddComponent(
                RequireProductType("DimensionBrawl.UI.UILoadingCardPresenter"));
            Component transitionPresenter = captureObject.AddComponent(
                RequireProductType("DimensionBrawl.UI.UITransitionPresenter"));
            Component loader = captureObject.AddComponent(
                RequireProductType("DimensionBrawl.UI.UISceneRouteLoader"));

            SetPrivateField(cardPresenter, "deck", deck);
            SetPrivateField(cardPresenter, "idText", idText);
            SetPrivateField(cardPresenter, "applyOnEnable", false);
            SetPrivateField(
                transitionPresenter,
                "fadeGroup",
                captureObject.GetComponent<CanvasGroup>());
            SetPrivateField(transitionPresenter, "loadingCardPresenter", cardPresenter);
            SetPrivateField(transitionPresenter, "defaultFadeSeconds", 60f);
            SetPrivateField(loader, "transitionPresenter", transitionPresenter);
            SetPrivateField(router, "routeLoader", loader);
            return idText;
        }

        private static ScriptableObject CreateLoadingCardDeck()
        {
            ScriptableObject deck = ScriptableObject.CreateInstance(
                RequireProductType("DimensionBrawl.UI.UILoadingCardDeck"));
            deck.hideFlags = HideFlags.HideAndDontSave;
            var serializedDeck = new SerializedObject(deck);
            SerializedProperty cards = serializedDeck.FindProperty("cards");
            cards.arraySize = 2;
            ConfigureLoadingCard(cards.GetArrayElementAtIndex(0), StageALoadingCardId);
            ConfigureLoadingCard(cards.GetArrayElementAtIndex(1), StageBLoadingCardId);
            serializedDeck.ApplyModifiedPropertiesWithoutUndo();
            return deck;
        }

        private static void ConfigureLoadingCard(SerializedProperty card, string id)
        {
            card.FindPropertyRelative("id").stringValue = id;
            card.FindPropertyRelative("title").stringValue = id;
            card.FindPropertyRelative("description").stringValue = id + " description";
            card.FindPropertyRelative("backgroundSprite").objectReferenceValue = null;
            card.FindPropertyRelative("weight").intValue = 1;
        }

        private static ScriptableObject CreateStageCatalog(
            params (string StageId, string LoadingCardId, PlayableStageDefinition Route)[] entries)
        {
            ScriptableObject catalog = ScriptableObject.CreateInstance(
                RequireProductType("DimensionBrawl.UI.UIStageCatalog"));
            catalog.hideFlags = HideFlags.HideAndDontSave;
            var serializedCatalog = new SerializedObject(catalog);
            serializedCatalog.FindProperty("projectionSchemaVersion").intValue = 1;
            serializedCatalog.FindProperty("catalogProjectionGeneration").intValue = 1;
            SerializedProperty stages = serializedCatalog.FindProperty("stages");
            stages.arraySize = entries.Length;
            for (int i = 0; i < entries.Length; i++)
            {
                SerializedProperty stage = stages.GetArrayElementAtIndex(i);
                stage.FindPropertyRelative("id").stringValue = entries[i].StageId;
                stage.FindPropertyRelative("displayName").stringValue =
                    entries[i].Route.ReferenceBlock.StageTemplate.Title;
                stage.FindPropertyRelative("summary").stringValue =
                    entries[i].Route.ReferenceBlock.StageTemplate.Objective;
                stage.FindPropertyRelative("threatTags").stringValue = string.Empty;
                stage.FindPropertyRelative("recommendedSummonRole").stringValue = string.Empty;
                stage.FindPropertyRelative("mockRewardPreview").stringValue = string.Empty;
                stage.FindPropertyRelative("presentationProvenance").intValue = 1;
                stage.FindPropertyRelative("playableStage").objectReferenceValue = entries[i].Route;
                stage.FindPropertyRelative("loadingCardId").stringValue = entries[i].LoadingCardId;
                stage.FindPropertyRelative("canonicalProjectionDigest").stringValue = string.Empty;
            }

            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            var digests = new string[entries.Length];
            for (int i = 0; i < entries.Length; i++)
            {
                object[] digestArguments =
                {
                    i,
                    ResolveUiRouteId(CombatRouteId),
                    null,
                    null
                };
                Assert.That(
                    (bool)RequireMethod(
                        catalog.GetType(),
                        "TryComputeCanonicalProjectionDigest").Invoke(
                        catalog,
                        digestArguments),
                    Is.True,
                    digestArguments[3]?.ToString());
                digests[i] = (string)digestArguments[2];
            }

            serializedCatalog.Update();
            stages = serializedCatalog.FindProperty("stages");
            for (int i = 0; i < digests.Length; i++)
            {
                stages.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("canonicalProjectionDigest")
                    .stringValue = digests[i];
            }

            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            return catalog;
        }

        private static StageDefinitionProfile CreateStageDefinition(string stageId, string scenePath)
        {
            StageDefinitionProfile definition =
                ScriptableObject.CreateInstance<StageDefinitionProfile>();
            definition.hideFlags = HideFlags.HideAndDontSave;
            var serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("stageId").stringValue = stageId;
            serializedDefinition.FindProperty("mapScenePath").stringValue = scenePath;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            return definition;
        }

        private static PlayableStageDefinition CreatePlayableStageDefinition(
            StageDefinitionProfile entryDefinition)
        {
            PlayableStageDefinition source = LoadRequired<PlayableStageDefinition>(
                PlayableStagePath);
            PlayableStageDefinition route = UnityEngine.Object.Instantiate(source);
            route.hideFlags = HideFlags.HideAndDontSave;
            route.name = entryDefinition.StageId + " route";

            var serializedRoute = new SerializedObject(route);
            SerializedProperty segments = serializedRoute.FindProperty("sceneSegments");
            Assert.That(segments.arraySize, Is.GreaterThan(0));
            for (int i = 0; i < segments.arraySize; i++)
            {
                segments.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("stageDefinition")
                    .objectReferenceValue = entryDefinition;
            }

            serializedRoute.FindProperty("canonicalRouteDigest").stringValue = string.Empty;
            serializedRoute.ApplyModifiedPropertiesWithoutUndo();

            serializedRoute.Update();
            serializedRoute.FindProperty("canonicalRouteDigest").stringValue =
                route.ComputeCanonicalRouteDigest();
            SerializedProperty reference = serializedRoute.FindProperty("referenceBlock");
            reference.FindPropertyRelative("canonicalReferenceDigest").stringValue = string.Empty;
            reference.FindPropertyRelative("canonicalBriefingDigest").stringValue = string.Empty;
            serializedRoute.ApplyModifiedPropertiesWithoutUndo();

            serializedRoute.Update();
            reference = serializedRoute.FindProperty("referenceBlock");
            reference.FindPropertyRelative("canonicalReferenceDigest").stringValue =
                route.ComputeCanonicalReferenceDigest();
            serializedRoute.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(
                route.TryComputeCanonicalBriefingDigest(
                    out string briefingDigest,
                    out StageBriefingBuildRejectReason briefingRejectReason),
                Is.True,
                briefingRejectReason.ToString());
            serializedRoute.Update();
            serializedRoute.FindProperty("referenceBlock")
                .FindPropertyRelative("canonicalBriefingDigest")
                .stringValue = briefingDigest;
            serializedRoute.ApplyModifiedPropertiesWithoutUndo();

            StageResultProgressionJoinBlock sourceJoin = source.ResultProgressionJoin;
            StageProgressionNode node = UnityEngine.Object.Instantiate(sourceJoin.ProgressionNode);
            node.hideFlags = HideFlags.HideAndDontSave;
            var serializedNode = new SerializedObject(node);
            serializedNode.FindProperty("playableStageId").stringValue = route.PlayableStageId;
            serializedNode.FindProperty("routeRevision").intValue = route.RouteRevision;
            serializedNode.FindProperty("canonicalRouteDigest").stringValue = route.CanonicalRouteDigest;
            serializedNode.FindProperty("contentDigest").stringValue = string.Empty;
            serializedNode.FindProperty("bindingDigest").stringValue = string.Empty;
            serializedNode.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(
                node.TryComputeCanonicalDigests(
                    out string nodeContentDigest,
                    out string nodeBindingDigest,
                    out string nodeError),
                Is.True,
                nodeError);
            serializedNode.Update();
            serializedNode.FindProperty("contentDigest").stringValue = nodeContentDigest;
            serializedNode.FindProperty("bindingDigest").stringValue = nodeBindingDigest;
            serializedNode.ApplyModifiedPropertiesWithoutUndo();

            StageProgressionGraph graph = UnityEngine.Object.Instantiate(sourceJoin.ProgressionGraph);
            graph.hideFlags = HideFlags.HideAndDontSave;
            var serializedGraph = new SerializedObject(graph);
            SerializedProperty graphNodes = serializedGraph.FindProperty("nodes");
            graphNodes.arraySize = 1;
            graphNodes.GetArrayElementAtIndex(0).objectReferenceValue = node;
            serializedGraph.FindProperty("canonicalDigest").stringValue = string.Empty;
            serializedGraph.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(
                graph.TryComputeCanonicalDigest(out string graphDigest, out string graphError),
                Is.True,
                graphError);
            serializedGraph.Update();
            serializedGraph.FindProperty("canonicalDigest").stringValue = graphDigest;
            serializedGraph.ApplyModifiedPropertiesWithoutUndo();

            serializedRoute.Update();
            SerializedProperty join = serializedRoute.FindProperty("resultProgressionJoin");
            join.FindPropertyRelative("progressionNode").objectReferenceValue = node;
            join.FindPropertyRelative("progressionGraph").objectReferenceValue = graph;
            join.FindPropertyRelative("canonicalDigest").stringValue = string.Empty;
            serializedRoute.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(
                route.TryComputeResultProgressionJoinDigest(
                    out string joinDigest,
                    out string joinError),
                Is.True,
                joinError);
            serializedRoute.Update();
            serializedRoute.FindProperty("resultProgressionJoin")
                .FindPropertyRelative("canonicalDigest")
                .stringValue = joinDigest;
            serializedRoute.ApplyModifiedPropertiesWithoutUndo();

            Assert.That(
                route.TryCreateBriefingReadModel(
                    out _,
                    out StageBriefingBuildRejectReason finalBriefingRejectReason),
                Is.True,
                finalBriefingRejectReason.ToString());
            Assert.That(
                StageRunResultProgressionJoinSnapshot.TryCreate(
                    route,
                    out _,
                    out string snapshotError),
                Is.True,
                snapshotError);
            return route;
        }

        private static void DestroyPlayableStageDefinition(PlayableStageDefinition route)
        {
            StageProgressionNode node = route?.ResultProgressionJoin?.ProgressionNode;
            StageProgressionGraph graph = route?.ResultProgressionJoin?.ProgressionGraph;
            UnityEngine.Object.DestroyImmediate(route);
            if (graph != null && !EditorUtility.IsPersistent(graph))
            {
                UnityEngine.Object.DestroyImmediate(graph);
            }

            if (node != null && !EditorUtility.IsPersistent(node))
            {
                UnityEngine.Object.DestroyImmediate(node);
            }
        }

        private static int GetRuntimeListenerCount(UnityEventBase unityEvent)
        {
            FieldInfo callsField = typeof(UnityEventBase).GetField(
                "m_Calls",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(callsField, Is.Not.Null);
            object calls = callsField.GetValue(unityEvent);
            Assert.That(calls, Is.Not.Null);
            FieldInfo runtimeCallsField = calls.GetType().GetField(
                "m_RuntimeCalls",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(runtimeCallsField, Is.Not.Null);
            var runtimeCalls = runtimeCallsField.GetValue(calls) as ICollection;
            Assert.That(runtimeCalls, Is.Not.Null);
            return runtimeCalls.Count;
        }

        private static T LoadRequired<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Assert.That(asset, Is.Not.Null, $"Missing required asset {path}.");
            return asset;
        }

        private static Type RequireProductType(string fullName)
        {
            Type type = Type.GetType(fullName + ", DimensionBrawl.Runtime")
                ?? Type.GetType(fullName + ", Assembly-CSharp");
            Assert.That(type, Is.Not.Null, $"Missing product type {fullName}.");
            return type;
        }

        private static void TryResetUiTransitionRuntime(string fullName)
        {
            Type type = Type.GetType(fullName + ", DimensionBrawl.Runtime")
                ?? Type.GetType(fullName + ", Assembly-CSharp");
            MethodInfo reset = type?.GetMethod(
                "ResetForTests",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            reset?.Invoke(null, null);
        }

        private static object ResolveUiRouteId(int rawValue)
        {
            return Enum.ToObject(
                RequireProductType("DimensionBrawl.UI.UIRouteId"),
                rawValue);
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
            Assert.That(
                property,
                Is.Not.Null,
                $"Missing property {target.GetType().Name}.{propertyName}.");
            return property.GetValue(target);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(
                field,
                Is.Not.Null,
                $"Missing private field {target.GetType().Name}.{fieldName}.");
            field.SetValue(target, value);
        }

        private static T ReadPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(
                field,
                Is.Not.Null,
                $"Missing private field {target.GetType().Name}.{fieldName}.");
            object value = field.GetValue(target);
            Assert.That(value, Is.Not.Null);
            return (T)value;
        }

        private static T ReadPrivateFieldAllowNull<T>(object target, string fieldName)
            where T : class
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(
                field,
                Is.Not.Null,
                $"Missing private field {target.GetType().Name}.{fieldName}.");
            return field.GetValue(target) as T;
        }

        private enum InvalidBindingKind
        {
            MissingCatalogRow,
            UnknownStageId,
            DuplicateStageId,
            SharedButton,
            NullButton,
            ButtonTargetMismatch
        }
    }
}
