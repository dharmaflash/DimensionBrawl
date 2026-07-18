using System;
using System.Collections;
using System.Reflection;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class OlympusStagePreparationReviewControllerPlayModeTests
    {
        private const string ProductNamespace =
            "DimensionBrawl.UI.StagePreparationReview.";
        private const string ControllerTypeName = ProductNamespace
            + "OlympusStagePreparationReviewController";
        private const string ProfileTypeName = ProductNamespace
            + "StagePreparationReviewProfile";
        private const string CatalogPath =
            "Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset";
        private const string CanonicalCatalogEntryId = "story_v1_training_route";
        private const string BoundaryPhrase = "NOT A STAGE RECOMMENDATION";
        private static readonly string[] ActionProfilePaths =
        {
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot1_ChargeBruiser.asset",
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot2_LaserSoldier.asset",
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot3_FireDragon.asset"
        };

        [SetUp]
        public void ResetStageRunBeforeTest()
        {
            StageRunRuntime.ResetForTests();
        }

        [TearDown]
        public void ResetStageRunAfterTest()
        {
            StageRunRuntime.ResetForTests();
        }

        [UnityTest]
        public IEnumerator CanonicalFlowUsesNeutralBriefingAndDispatchesDigestExactlyOnce()
        {
            using var fixture = new ControllerFixture();
            int activeSceneHandle = SceneManager.GetActiveScene().handle;
            int publicEventCount = 0;
            int serializedEventCount = 0;
            string publicDigest = string.Empty;
            string serializedDigest = string.Empty;
            Action<string> handler = digest =>
            {
                publicEventCount++;
                publicDigest = digest;
            };
            RequireEvent(fixture.Controller.GetType(), "ReviewConfirmed")
                .AddEventHandler(fixture.Controller, handler);
            var confirmationEvent = (UnityEvent<string>)ReadProperty(
                fixture.Controller,
                "ConfirmationEvent");
            confirmationEvent.AddListener(digest =>
            {
                serializedEventCount++;
                serializedDigest = digest;
            });

            fixture.Activate();
            yield return null;

            AssertPhase(fixture.Controller, "StageIntel");
            AssertPanel(fixture.Controller, "StageIntel");
            object projection = ReadProperty(fixture.Controller, "CurrentProjection");
            Assert.That(projection, Is.Not.Null);
            Assert.That(ReadProperty(projection, "UiRouteId").ToString(), Is.EqualTo("Combat"));
            Assert.That(
                ReadBool(fixture.Controller, "HasNeutralStageRecommendationBoundary"),
                Is.True);
            object briefing = ReadProperty(projection, "Briefing");
            Assert.That(
                ReadProperty(briefing, "FeaturedThreatDisposition").ToString(),
                Is.EqualTo("NoVerifiedSource"));
            Assert.That(
                ReadProperty(briefing, "RecommendedLoadoutDisposition").ToString(),
                Is.EqualTo("NoVerifiedSource"));
            Assert.That(
                ReadProperty(briefing, "FeaturedSummonNeedDisposition").ToString(),
                Is.EqualTo("NoVerifiedSource"));

            object catalogEntry = Invoke(fixture.Catalog, "GetStage", 0);
            string legacyThreat = ReadString(catalogEntry, "ThreatTags");
            string legacyRecommendation = ReadString(catalogEntry, "RecommendedSummonRole");
            Assert.That(
                legacyThreat,
                Is.Not.Empty,
                "The regression fixture must contain the legacy threat copy it proves is hidden.");
            Assert.That(
                legacyRecommendation,
                Is.Not.Empty,
                "The regression fixture must contain the legacy recommendation copy it proves is hidden.");
            Assert.That(ReadText(fixture.IntelThreat), Is.EqualTo(ReadControllerConstant(
                "NeutralThreatPreviewStatus")));
            Assert.That(ReadText(fixture.IntelRecommendedRole), Is.EqualTo(ReadControllerConstant(
                "NeutralRuntimePresetStatus")));
            Assert.That(ReadText(fixture.IntelThreat), Does.Not.Contain(legacyThreat));
            Assert.That(ReadText(fixture.IntelRecommendedRole), Does.Not.Contain(legacyRecommendation));
            Assert.That(
                ReadText(fixture.IntelSummary),
                Is.EqualTo(ReadString(briefing, "CombatLesson")));
            Assert.That(
                ReadText(fixture.IntelObjective),
                Is.EqualTo(ReadString(briefing, "Objective")));
            AssertBoundaryVisible(fixture.IntelStatus);

            fixture.IntelContinueButton.onClick.Invoke();
            AssertPhase(fixture.Controller, "LoadoutOverview");
            AssertPanel(fixture.Controller, "LoadoutOverview");
            Assert.That(
                ReadText(fixture.LoadoutStatus),
                Is.EqualTo(ReadControllerConstant("PresetBoundaryStatus")));
            AssertBoundaryVisible(fixture.LoadoutStatus);

            fixture.SlotButtons[1].onClick.Invoke();
            AssertPhase(fixture.Controller, "SummonDetail");
            Assert.That(
                ReadString(fixture.Controller, "SelectedSlotId"),
                Is.EqualTo("SummonSlot2"));
            fixture.TierButtons[2].onClick.Invoke();
            Assert.That(ReadInt(fixture.Controller, "SelectedTier"), Is.EqualTo(3));
            AssertSelectedTierPresentation(fixture, 2);
            Assert.That(
                ReadText(fixture.DetailSelectedTier),
                Is.EqualTo("LV3 Prism Burst"));
            AssertBoundaryVisible(fixture.DetailStatus);
            fixture.DetailBackButton.onClick.Invoke();
            Assert.That(
                ReadProperty(fixture.Controller, "LastFocusTarget"),
                Is.SameAs(fixture.SlotButtons[1].gameObject));

            fixture.SlotButtons[0].onClick.Invoke();
            fixture.TierButtons[1].onClick.Invoke();
            AssertSelectedTierPresentation(fixture, 1);
            fixture.DetailBackButton.onClick.Invoke();
            Assert.That(
                ReadProperty(fixture.Controller, "LastFocusTarget"),
                Is.SameAs(fixture.SlotButtons[0].gameObject));
            fixture.LoadoutReviewButton.onClick.Invoke();
            AssertPhase(fixture.Controller, "ReviewConfirm");
            AssertPanel(fixture.Controller, "ReviewConfirm");
            string selectionDigest = ReadString(fixture.Controller, "CurrentSelectionDigest");
            Assert.That(selectionDigest, Has.Length.EqualTo(64));
            Assert.That(ReadText(fixture.ConfirmDigest), Is.EqualTo(selectionDigest));
            AssertBoundaryVisible(fixture.ConfirmStatus);
            Assert.That(fixture.ConfirmAcceptButton.gameObject.activeSelf, Is.True);
            Assert.That(fixture.ConfirmRestartButton.gameObject.activeSelf, Is.False);

            Array firstSnapshot = (Array)ReadProperty(fixture.Controller, "SelectionSnapshot");
            firstSnapshot.SetValue(firstSnapshot.GetValue(1), 0);
            Array freshSnapshot = (Array)ReadProperty(fixture.Controller, "SelectionSnapshot");
            Assert.That(
                ReadString(freshSnapshot.GetValue(0), "SlotId"),
                Is.EqualTo("SummonSlot1"));
            Assert.That(
                ReadInt(freshSnapshot.GetValue(0), "SelectedTier"),
                Is.EqualTo(2));

            fixture.ConfirmAcceptButton.onClick.Invoke();
            fixture.ConfirmAcceptButton.onClick.Invoke();
            Assert.That(ReadBool(fixture.Controller, "IsReviewAccepted"), Is.True);
            Assert.That(ReadInt(fixture.Controller, "ConfirmationDispatchCount"), Is.EqualTo(1));
            Assert.That(publicEventCount, Is.EqualTo(1));
            Assert.That(serializedEventCount, Is.EqualTo(1));
            Assert.That(publicDigest, Is.EqualTo(selectionDigest));
            Assert.That(serializedDigest, Is.EqualTo(selectionDigest));
            Assert.That(fixture.ConfirmAcceptButton.gameObject.activeSelf, Is.False);
            Assert.That(fixture.ConfirmRestartButton.gameObject.activeSelf, Is.True);
            Assert.That(
                ReadString(fixture.Controller, "LastConfirmedSelectionDigest"),
                Is.EqualTo(selectionDigest));
            Assert.That((bool)Invoke(fixture.Controller, "ConfirmReview"), Is.False);
            AssertBoundaryVisible(fixture.ConfirmStatus);
            Assert.That(StageRunRuntime.HasActiveContext, Is.False);
            Assert.That(StageRunRuntime.ActiveContext, Is.Null);
            Assert.That(
                SceneManager.GetActiveScene().handle == activeSceneHandle,
                Is.True,
                "PREP-01 acknowledgement must not change the active scene.");

            fixture.ConfirmRestartButton.onClick.Invoke();
            AssertPhase(fixture.Controller, "StageIntel");
            Assert.That(ReadBool(fixture.Controller, "IsReviewAccepted"), Is.False);
            Assert.That(ReadInt(fixture.Controller, "ConfirmationDispatchCount"), Is.Zero);
            Assert.That(
                ReadString(fixture.Controller, "LastConfirmedSelectionDigest"),
                Is.Empty);
            Array resetSnapshot = (Array)ReadProperty(fixture.Controller, "SelectionSnapshot");
            for (int i = 0; i < resetSnapshot.Length; i++)
            {
                Assert.That(
                    ReadInt(resetSnapshot.GetValue(i), "SelectedTier"),
                    Is.EqualTo(1));
            }

            Assert.That(publicEventCount, Is.EqualTo(1));
            Assert.That(serializedEventCount, Is.EqualTo(1));
            Assert.That(ReadProperty(fixture.Controller, "CurrentProjection"), Is.Not.Null);
            Assert.That(StageRunRuntime.HasActiveContext, Is.False);
        }

        [UnityTest]
        public IEnumerator ReenableBalancesListenersAndStaleProjectionFailsClosed()
        {
            using var fixture = new ControllerFixture(cloneCatalog: true);
            int activeSceneHandle = SceneManager.GetActiveScene().handle;
            fixture.Activate();
            yield return null;

            fixture.IntelContinueButton.onClick.Invoke();
            fixture.SlotButtons[0].onClick.Invoke();
            fixture.TierButtons[2].onClick.Invoke();
            AssertPhase(fixture.Controller, "SummonDetail");
            Assert.That(ReadInt(fixture.Controller, "SelectedTier"), Is.EqualTo(3));

            fixture.Root.SetActive(false);
            fixture.Root.SetActive(true);
            yield return null;
            AssertPhase(fixture.Controller, "SummonDetail");
            Assert.That(ReadInt(fixture.Controller, "SelectedTier"), Is.EqualTo(3));

            fixture.DetailBackButton.onClick.Invoke();
            AssertPhase(
                fixture.Controller,
                "LoadoutOverview",
                "A duplicated back listener would consume LoadoutOverview and land on StageIntel.");
            Assert.That(TryGetSelectedTier(fixture.Controller, "SummonSlot1"), Is.EqualTo(3));

            var serializedCatalog = new SerializedObject(fixture.Catalog);
            SerializedProperty generation = serializedCatalog.FindProperty(
                "catalogProjectionGeneration");
            Assert.That(generation, Is.Not.Null);
            generation.intValue++;
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();

            Assert.That((bool)Invoke(fixture.Controller, "OpenReviewConfirm"), Is.False);
            AssertPhase(fixture.Controller, "LoadoutOverview");
            Assert.That(ReadProperty(fixture.Controller, "CurrentProjection"), Is.Null);
            Assert.That(
                ReadProperty(fixture.Controller, "LastProjectionRejectReason").ToString(),
                Is.Not.EqualTo("None"));
            Assert.That(fixture.LoadoutReviewButton.interactable, Is.False);
            Assert.That(ReadText(fixture.LoadoutStatus), Does.Contain("CANONICAL DATA UNAVAILABLE"));
            AssertBoundaryVisible(fixture.LoadoutStatus);
            Assert.That(ReadInt(fixture.Controller, "ConfirmationDispatchCount"), Is.Zero);
            Assert.That(StageRunRuntime.HasActiveContext, Is.False);
            Assert.That(
                SceneManager.GetActiveScene().handle == activeSceneHandle,
                Is.True,
                "PREP-01 stale projection handling must not change the active scene.");
        }

        private static Type ControllerType => RequireProductType(ControllerTypeName);
        private static Type ProfileType => RequireProductType(ProfileTypeName);
        private static Type SlotDefinitionType => ProfileType.GetNestedType(
            "SlotDefinition",
            BindingFlags.Public | BindingFlags.NonPublic);
        private static Type SlotBindingType => ControllerType.GetNestedType(
            "SlotBinding",
            BindingFlags.Public | BindingFlags.NonPublic);

        private static void AssertBoundaryVisible(Component text)
        {
            Assert.That(text.gameObject.activeInHierarchy, Is.True);
            Assert.That(ReadText(text), Does.Contain(BoundaryPhrase));
        }

        private static void AssertPhase(object controller, string expected, string message = null)
        {
            Assert.That(
                ReadProperty(controller, "CurrentPhase").ToString(),
                Is.EqualTo(expected),
                message);
        }

        private static void AssertPanel(object controller, string expected)
        {
            Assert.That(ReadProperty(controller, "CurrentPanel").ToString(), Is.EqualTo(expected));
        }

        private static void AssertSelectedTierPresentation(
            ControllerFixture fixture,
            int selectedIndex)
        {
            Assert.That(selectedIndex, Is.InRange(0, fixture.TierButtons.Length - 1));
            Color selected = fixture.TierButtons[selectedIndex].targetGraphic.color;
            Color? firstUnselected = null;
            for (int i = 0; i < fixture.TierButtons.Length; i++)
            {
                if (i == selectedIndex)
                {
                    continue;
                }

                Color unselected = fixture.TierButtons[i].targetGraphic.color;
                if (firstUnselected.HasValue)
                {
                    Assert.That(unselected, Is.EqualTo(firstUnselected.Value));
                }
                else
                {
                    firstUnselected = unselected;
                }
            }

            Assert.That(firstUnselected.HasValue, Is.True);
            Assert.That(selected, Is.Not.EqualTo(firstUnselected.Value));
        }

        private static int TryGetSelectedTier(object controller, string slotId)
        {
            object[] arguments = { slotId, 0 };
            bool found = (bool)RequireMethod(
                    controller.GetType(),
                    "TryGetSelectedTier",
                    2)
                .Invoke(controller, arguments);
            Assert.That(found, Is.True);
            return Convert.ToInt32(arguments[1]);
        }

        private static string ReadControllerConstant(string name)
        {
            FieldInfo field = ControllerType.GetField(name, BindingFlags.Public | BindingFlags.Static);
            Assert.That(field, Is.Not.Null, name);
            return (string)field.GetRawConstantValue();
        }

        private static EventInfo RequireEvent(Type type, string eventName)
        {
            EventInfo eventInfo = type.GetEvent(
                eventName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(eventInfo, Is.Not.Null, eventName);
            return eventInfo;
        }

        private static object Invoke(object target, string methodName, params object[] arguments)
        {
            object[] safeArguments = arguments ?? Array.Empty<object>();
            return RequireMethod(target.GetType(), methodName, safeArguments.Length)
                .Invoke(target, safeArguments);
        }

        private static MethodInfo RequireMethod(Type type, string methodName, int parameterCount = 0)
        {
            MethodInfo match = null;
            foreach (MethodInfo method in type.GetMethods(
                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (method.Name != methodName
                    || method.GetParameters().Length != parameterCount)
                {
                    continue;
                }

                Assert.That(match, Is.Null, $"Ambiguous {type.Name}.{methodName}/{parameterCount}.");
                match = method;
            }

            Assert.That(match, Is.Not.Null, $"Missing {type.Name}.{methodName}/{parameterCount}.");
            return match;
        }

        private static object ReadProperty(object target, string propertyName)
        {
            Assert.That(target, Is.Not.Null);
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null, propertyName);
            return property.GetValue(target);
        }

        private static string ReadString(object target, string propertyName)
        {
            return ReadProperty(target, propertyName) as string ?? string.Empty;
        }

        private static int ReadInt(object target, string propertyName)
        {
            return Convert.ToInt32(ReadProperty(target, propertyName));
        }

        private static bool ReadBool(object target, string propertyName)
        {
            return Convert.ToBoolean(ReadProperty(target, propertyName));
        }

        private static string ReadText(Component text)
        {
            return text != null ? ReadString(text, "text") : string.Empty;
        }

        private static Type RequireProductType(string fullName)
        {
            Type type = Type.GetType(fullName + ", Assembly-CSharp")
                ?? Type.GetType(fullName + ", DimensionBrawl.Runtime");
            Assert.That(type, Is.Not.Null, fullName);
            return type;
        }

        private sealed class ControllerFixture : IDisposable
        {
            private readonly bool ownsCatalog;
            private readonly Texture2D texture;
            private readonly Sprite icon;

            public ControllerFixture(bool cloneCatalog = false)
            {
                Root = new GameObject("StagePreparationReviewFixture", typeof(RectTransform));
                Root.SetActive(false);
                Controller = Root.AddComponent(ControllerType);

                ScriptableObject sourceCatalog = AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                    CatalogPath);
                Assert.That(sourceCatalog, Is.Not.Null, CatalogPath);
                Catalog = cloneCatalog
                    ? UnityEngine.Object.Instantiate(sourceCatalog)
                    : sourceCatalog;
                ownsCatalog = cloneCatalog;

                ActionProfiles = new SummonSlotActionProfile[ActionProfilePaths.Length];
                for (int i = 0; i < ActionProfiles.Length; i++)
                {
                    ActionProfiles[i] = AssetDatabase.LoadAssetAtPath<SummonSlotActionProfile>(
                        ActionProfilePaths[i]);
                    Assert.That(ActionProfiles[i], Is.Not.Null, ActionProfilePaths[i]);
                }

                texture = new Texture2D(2, 2);
                icon = Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.one * 0.5f);
                Profile = CreateProfile(ActionProfiles, icon);

                StageIntelPanel = CreateGroup(Root.transform, "StageIntelPanel");
                LoadoutPanel = CreateGroup(Root.transform, "LoadoutPanel");
                DetailPanel = CreateGroup(Root.transform, "DetailPanel");
                ConfirmPanel = CreateGroup(Root.transform, "ConfirmPanel");

                IntelTitle = CreateText(StageIntelPanel.transform, "IntelTitle");
                IntelStageCode = CreateText(StageIntelPanel.transform, "IntelStageCode");
                IntelStageTitle = CreateText(StageIntelPanel.transform, "IntelStageTitle");
                IntelSummary = CreateText(StageIntelPanel.transform, "IntelSummary");
                IntelObjective = CreateText(StageIntelPanel.transform, "IntelObjective");
                IntelThreat = CreateText(StageIntelPanel.transform, "IntelThreat");
                IntelRecommendedRole = CreateText(
                    StageIntelPanel.transform,
                    "IntelRecommendedRole");
                IntelStatus = CreateText(StageIntelPanel.transform, "IntelStatus");
                IntelContinueButton = CreateButton(
                    StageIntelPanel.transform,
                    "IntelContinueButton");

                LoadoutPilotTitle = CreateText(LoadoutPanel.transform, "LoadoutPilotTitle");
                LoadoutBoundary = CreateText(LoadoutPanel.transform, "LoadoutBoundary");
                LoadoutStatus = CreateText(LoadoutPanel.transform, "LoadoutStatus");
                LoadoutBackButton = CreateButton(LoadoutPanel.transform, "LoadoutBackButton");
                LoadoutReviewButton = CreateButton(LoadoutPanel.transform, "LoadoutReviewButton");
                SlotButtons = new Button[3];
                Array slotBindings = Array.CreateInstance(SlotBindingType, 3);
                for (int i = 0; i < 3; i++)
                {
                    SlotButtons[i] = CreateButton(
                        LoadoutPanel.transform,
                        $"Slot{i + 1}Button");
                    object binding = Activator.CreateInstance(SlotBindingType);
                    Invoke(
                        binding,
                        "Configure",
                        $"SummonSlot{i + 1}",
                        SlotButtons[i],
                        CreateImage(SlotButtons[i].transform, $"Slot{i + 1}Icon"),
                        CreateText(SlotButtons[i].transform, $"Slot{i + 1}Title"),
                        CreateText(SlotButtons[i].transform, $"Slot{i + 1}Role"),
                        CreateText(SlotButtons[i].transform, $"Slot{i + 1}Tier"));
                    slotBindings.SetValue(binding, i);
                }

                DetailIcon = CreateImage(DetailPanel.transform, "DetailIcon");
                DetailTitle = CreateText(DetailPanel.transform, "DetailTitle");
                DetailRole = CreateText(DetailPanel.transform, "DetailRole");
                DetailSelectedTier = CreateText(DetailPanel.transform, "DetailSelectedTier");
                DetailStageRole = CreateText(DetailPanel.transform, "DetailStageRole");
                DetailPlayerUse = CreateText(DetailPanel.transform, "DetailPlayerUse");
                DetailSummonRead = CreateText(DetailPanel.transform, "DetailSummonRead");
                DetailStatus = CreateText(DetailPanel.transform, "DetailStatus");
                TierButtons = new[]
                {
                    CreateButton(DetailPanel.transform, "Tier1Button"),
                    CreateButton(DetailPanel.transform, "Tier2Button"),
                    CreateButton(DetailPanel.transform, "Tier3Button")
                };
                DetailBackButton = CreateButton(DetailPanel.transform, "DetailBackButton");

                ConfirmTitle = CreateText(ConfirmPanel.transform, "ConfirmTitle");
                ConfirmSummary = CreateText(ConfirmPanel.transform, "ConfirmSummary");
                ConfirmDigest = CreateText(ConfirmPanel.transform, "ConfirmDigest");
                ConfirmStatus = CreateText(ConfirmPanel.transform, "ConfirmStatus");
                ConfirmBackButton = CreateButton(ConfirmPanel.transform, "ConfirmBackButton");
                ConfirmAcceptButton = CreateButton(ConfirmPanel.transform, "ConfirmAcceptButton");
                ConfirmRestartButton = CreateButton(ConfirmPanel.transform, "ConfirmRestartButton");

                Invoke(Controller, "ConfigureCore", Profile, Catalog);
                Invoke(
                    Controller,
                    "ConfigurePanels",
                    StageIntelPanel,
                    LoadoutPanel,
                    DetailPanel,
                    ConfirmPanel);
                Invoke(
                    Controller,
                    "ConfigureIntelView",
                    IntelTitle,
                    IntelStageCode,
                    IntelStageTitle,
                    IntelSummary,
                    IntelObjective,
                    IntelThreat,
                    IntelRecommendedRole,
                    IntelStatus,
                    IntelContinueButton);
                Invoke(
                    Controller,
                    "ConfigureLoadoutView",
                    LoadoutPilotTitle,
                    LoadoutBoundary,
                    LoadoutStatus,
                    LoadoutBackButton,
                    LoadoutReviewButton,
                    slotBindings);
                Invoke(
                    Controller,
                    "ConfigureDetailView",
                    DetailIcon,
                    DetailTitle,
                    DetailRole,
                    DetailSelectedTier,
                    DetailStageRole,
                    DetailPlayerUse,
                    DetailSummonRead,
                    DetailStatus,
                    TierButtons[0],
                    TierButtons[1],
                    TierButtons[2],
                    DetailBackButton);
                Invoke(
                    Controller,
                    "ConfigureConfirmationView",
                    ConfirmTitle,
                    ConfirmSummary,
                    ConfirmDigest,
                    ConfirmStatus,
                    ConfirmBackButton,
                    ConfirmAcceptButton,
                    ConfirmRestartButton);
            }

            public GameObject Root { get; }
            public Component Controller { get; }
            public ScriptableObject Profile { get; }
            public ScriptableObject Catalog { get; }
            public SummonSlotActionProfile[] ActionProfiles { get; }
            public CanvasGroup StageIntelPanel { get; }
            public CanvasGroup LoadoutPanel { get; }
            public CanvasGroup DetailPanel { get; }
            public CanvasGroup ConfirmPanel { get; }
            public Component IntelTitle { get; }
            public Component IntelStageCode { get; }
            public Component IntelStageTitle { get; }
            public Component IntelSummary { get; }
            public Component IntelObjective { get; }
            public Component IntelThreat { get; }
            public Component IntelRecommendedRole { get; }
            public Component IntelStatus { get; }
            public Button IntelContinueButton { get; }
            public Component LoadoutPilotTitle { get; }
            public Component LoadoutBoundary { get; }
            public Component LoadoutStatus { get; }
            public Button LoadoutBackButton { get; }
            public Button LoadoutReviewButton { get; }
            public Button[] SlotButtons { get; }
            public Image DetailIcon { get; }
            public Component DetailTitle { get; }
            public Component DetailRole { get; }
            public Component DetailSelectedTier { get; }
            public Component DetailStageRole { get; }
            public Component DetailPlayerUse { get; }
            public Component DetailSummonRead { get; }
            public Component DetailStatus { get; }
            public Button[] TierButtons { get; }
            public Button DetailBackButton { get; }
            public Component ConfirmTitle { get; }
            public Component ConfirmSummary { get; }
            public Component ConfirmDigest { get; }
            public Component ConfirmStatus { get; }
            public Button ConfirmBackButton { get; }
            public Button ConfirmAcceptButton { get; }
            public Button ConfirmRestartButton { get; }

            public void Activate()
            {
                Root.SetActive(true);
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(Root);
                UnityEngine.Object.DestroyImmediate(Profile);
                UnityEngine.Object.DestroyImmediate(icon);
                UnityEngine.Object.DestroyImmediate(texture);
                if (ownsCatalog)
                {
                    UnityEngine.Object.DestroyImmediate(Catalog);
                }
            }

            private static ScriptableObject CreateProfile(
                SummonSlotActionProfile[] actionProfiles,
                Sprite slotIcon)
            {
                ScriptableObject profile = ScriptableObject.CreateInstance(ProfileType);
                Array slots = Array.CreateInstance(SlotDefinitionType, 3);
                for (int i = 0; i < 3; i++)
                {
                    string slotId = $"SummonSlot{i + 1}";
                    object slot = Activator.CreateInstance(
                        SlotDefinitionType,
                        slotId,
                        $"ui.stage-preparation.{slotId}.title",
                        $"Canonical Slot {i + 1}",
                        $"Runtime preset role {i + 1}",
                        actionProfiles[i],
                        slotIcon);
                    slots.SetValue(slot, i);
                }

                Invoke(
                    profile,
                    "Configure",
                    "PREP-01",
                    CanonicalCatalogEntryId,
                    "ui.stage-preparation.title",
                    "Stage Preparation Review",
                    "pilot.fixed.review",
                    "ui.stage-preparation.pilot",
                    "Fixed Pilot Presentation",
                    "CANONICAL RUNTIME PRESET / NOT A STAGE RECOMMENDATION",
                    slots);
                object[] validationArguments = { string.Empty };
                Assert.That(
                    (bool)RequireMethod(ProfileType, "TryValidate", 1)
                        .Invoke(profile, validationArguments),
                    Is.True,
                    validationArguments[0]?.ToString());
                return profile;
            }
        }

        private static CanvasGroup CreateGroup(Transform parent, string name)
        {
            var owner = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            owner.transform.SetParent(parent, false);
            return owner.GetComponent<CanvasGroup>();
        }

        private static Button CreateButton(Transform parent, string name)
        {
            var owner = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            owner.transform.SetParent(parent, false);
            return owner.GetComponent<Button>();
        }

        private static Image CreateImage(Transform parent, string name)
        {
            var owner = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            owner.transform.SetParent(parent, false);
            return owner.GetComponent<Image>();
        }

        private static Component CreateText(Transform parent, string name)
        {
            Type textType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            Assert.That(textType, Is.Not.Null, "TextMeshProUGUI type is unavailable.");
            var owner = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer));
            owner.transform.SetParent(parent, false);
            return owner.AddComponent(textType);
        }
    }
}
