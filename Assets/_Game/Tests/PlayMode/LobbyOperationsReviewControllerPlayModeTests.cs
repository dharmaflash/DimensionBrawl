using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DimensionBrawl.LevelDesign;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class LobbyOperationsReviewControllerPlayModeTests
    {
        private const string ControllerTypeName =
            "DimensionBrawl.UI.LobbyOperationsReview.LobbyOperationsReviewController";
        private const string ProfileTypeName =
            "DimensionBrawl.UI.LobbyOperationsReview.LobbyOperationsReviewProfile";
        private const string RowKindTypeName =
            "DimensionBrawl.UI.LobbyOperationsReview.LobbyOperationsReviewDispositionRowKind";

        private static readonly string[] EntryIds =
        {
            "review.operations.notice",
            "review.operations.mailbox",
            "review.operations.missions",
            "review.operations.event-calendar"
        };

        private static readonly string[] SourceStatusConstants =
        {
            "NoticeSourceStatus",
            "MailboxSourceStatus",
            "MissionsSourceStatus",
            "EventCalendarSourceStatus"
        };

        private static readonly string[] DetailStatusConstants =
        {
            "NoticeDetailStatus",
            "MailboxDetailStatus",
            "MissionsDetailStatus",
            "EventCalendarDetailStatus"
        };

        private static readonly string[] DispositionKinds =
        {
            "Production",
            "Service",
            "Account",
            "ServerClock",
            "Schedule",
            "Progress",
            "Attention",
            "Action"
        };

        private static readonly string[] DispositionProperties =
        {
            "ProductionDisposition",
            "ServiceDisposition",
            "AccountDisposition",
            "ServerClockDisposition",
            "ScheduleDisposition",
            "ProgressDisposition",
            "AttentionDisposition",
            "ActionDisposition"
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
        public IEnumerator PanelsAndDirectoryRowsFollowTheExactFourEntryContract()
        {
            using var fixture = new ControllerFixture();
            fixture.Activate();
            yield return null;

            AssertOnlyPanel(fixture, "Closed");
            AssertPhase(fixture.Controller, "Closed");
            AssertFocused(fixture.OpenButton);
            Assert.That(ReadProperty<bool>(fixture.Controller, "HasExactEntryBindings"), Is.True);
            Assert.That(ReadProperty<int>(fixture.Controller, "EntryBindingCount"), Is.EqualTo(4));

            fixture.OpenButton.onClick.Invoke();
            AssertOnlyPanel(fixture, "Directory");
            AssertPhase(fixture.Controller, "Directory");
            AssertFocused(fixture.EntryButtons[0]);

            for (int index = 0; index < EntryIds.Length; index++)
            {
                object binding = Invoke(fixture.Controller, "GetEntryBinding", index);
                object entry = Invoke(fixture.Profile, "GetEntry", index);
                Assert.That(ReadProperty<string>(binding, "EntryId"), Is.EqualTo(EntryIds[index]));
                Assert.That(fixture.EntryButtons[index].interactable, Is.True);
                Assert.That(fixture.EntryGroups[index].alpha, Is.EqualTo(1f));
                Assert.That(fixture.EntryGroups[index].interactable, Is.True);
                Assert.That(fixture.EntryGroups[index].blocksRaycasts, Is.True);
                Assert.That(
                    ReadText(fixture.EntryTitles[index]),
                    Is.EqualTo(ReadProperty<string>(entry, "TitleFallback")));
                Assert.That(
                    ReadText(fixture.EntryStatuses[index]),
                    Is.EqualTo(ReadControllerConstant(SourceStatusConstants[index])));
            }

            fixture.ReplaceDirectoryBindingsWithMissingStatusText();
            Assert.That(
                ReadProperty<bool>(fixture.Controller, "HasExactEntryBindings"),
                Is.False,
                "A row without visible textual source status must fail closed.");
            Assert.That(fixture.EntryButtons.All(button => !button.interactable), Is.True);
            fixture.EntryButtons[0].onClick.Invoke();
            AssertPhase(
                fixture.Controller,
                "Directory",
                "Runtime reconfiguration must remove the old row delegate.");
            Assert.That(ReadProperty<string>(fixture.Controller, "SelectedEntryId"), Is.Empty);
            AssertNoStageRunOrSceneChange(fixture);
        }

        [UnityTest]
        public IEnumerator EveryEntryRendersExactDispositionsAndOnlyNoticeShowsTheCta()
        {
            using var fixture = new ControllerFixture();
            fixture.Activate();
            yield return null;
            fixture.OpenButton.onClick.Invoke();

            for (int entryIndex = 0; entryIndex < EntryIds.Length; entryIndex++)
            {
                object entry = Invoke(fixture.Profile, "GetEntry", entryIndex);
                fixture.EntryButtons[entryIndex].onClick.Invoke();

                AssertOnlyPanel(fixture, "Detail");
                AssertPhase(fixture.Controller, "EntryDetail");
                AssertFocused(fixture.DetailBackButton);
                Assert.That(
                    ReadText(fixture.DetailTitle),
                    Is.EqualTo(ReadProperty<string>(entry, "TitleFallback")));
                Assert.That(
                    ReadText(fixture.DetailExplanation),
                    Is.EqualTo(ReadProperty<string>(entry, "ExplanationFallback")));
                Assert.That(
                    ReadText(fixture.DetailStatus),
                    Is.EqualTo(ReadControllerConstant(DetailStatusConstants[entryIndex])));
                Assert.That(
                    ReadProperty<string>(fixture.Controller, "CurrentDetailStatus"),
                    Is.EqualTo(ReadControllerConstant(DetailStatusConstants[entryIndex])));

                for (int rowIndex = 0; rowIndex < DispositionKinds.Length; rowIndex++)
                {
                    Assert.That(fixture.DispositionRoots[rowIndex].activeSelf, Is.True);
                    Assert.That(
                        ReadText(fixture.DispositionLabels[rowIndex]),
                        Is.EqualTo(ResolveRowLabel(DispositionKinds[rowIndex])));
                    object disposition = ReadProperty(entry, DispositionProperties[rowIndex]);
                    Assert.That(
                        ReadText(fixture.DispositionValues[rowIndex]),
                        Is.EqualTo(ResolveDispositionLabel(disposition.ToString())));
                }

                bool isNotice = entryIndex == 0;
                Assert.That(fixture.DetailReviewButton.gameObject.activeSelf, Is.EqualTo(isNotice));
                Assert.That(fixture.DetailReviewButton.interactable, Is.EqualTo(isNotice));
                Assert.That(
                    ReadProperty<bool>(fixture.Controller, "IsReviewCtaVisible"),
                    Is.EqualTo(isNotice));
                if (!isNotice)
                {
                    Assert.That(
                        (bool)Invoke(fixture.Controller, "OpenReviewConfirm"),
                        Is.False);
                    AssertPhase(fixture.Controller, "EntryDetail");
                }

                fixture.DetailBackButton.onClick.Invoke();
                AssertOnlyPanel(fixture, "Directory");
                Assert.That(ReadText(fixture.DetailTitle), Is.Empty);
                Assert.That(ReadText(fixture.DetailExplanation), Is.Empty);
                Assert.That(ReadText(fixture.DetailStatus), Is.Empty);
                Assert.That(
                    fixture.DispositionRoots.All(row => !row.activeSelf),
                    Is.True,
                    "Hidden detail content must be cleared between entries.");
                Assert.That(
                    fixture.DispositionValues.All(value => ReadText(value) == string.Empty),
                    Is.True);
            }

            fixture.ReplaceDispositionRowsWithMissingValueText();
            Assert.That(
                ReadProperty<bool>(fixture.Controller, "HasExactDispositionRows"),
                Is.False,
                "A disposition row without textual value output must fail closed.");
            AssertNoStageRunOrSceneChange(fixture);
        }

        [UnityTest]
        public IEnumerator BackCloseAndFocusFollowTheDocumentedHierarchy()
        {
            using var fixture = new ControllerFixture();
            fixture.Activate();
            yield return null;

            AssertFocused(fixture.OpenButton);
            fixture.OpenButton.onClick.Invoke();
            AssertFocused(fixture.EntryButtons[0]);
            fixture.EntryButtons[0].onClick.Invoke();
            AssertFocused(fixture.DetailBackButton);
            fixture.DetailReviewButton.onClick.Invoke();
            AssertOnlyPanel(fixture, "Confirm");
            AssertFocused(fixture.ConfirmAcknowledgeButton);

            fixture.ConfirmBackButton.onClick.Invoke();
            AssertOnlyPanel(fixture, "Detail");
            AssertFocused(fixture.DetailBackButton);
            fixture.DetailCloseButton.onClick.Invoke();
            AssertOnlyPanel(fixture, "Closed");
            AssertFocused(fixture.OpenButton);

            fixture.OpenButton.onClick.Invoke();
            fixture.EntryButtons[1].onClick.Invoke();
            fixture.DetailBackButton.onClick.Invoke();
            AssertOnlyPanel(fixture, "Directory");
            AssertFocused(fixture.EntryButtons[0]);
            fixture.DirectoryBackButton.onClick.Invoke();
            AssertOnlyPanel(fixture, "Closed");
            AssertFocused(fixture.OpenButton);
            Assert.That((bool)Invoke(fixture.Controller, "Back"), Is.False);
            AssertOnlyPanel(fixture, "Closed");
            AssertNoStageRunOrSceneChange(fixture);
        }

        [UnityTest]
        public IEnumerator MissingSourceDetailCannotRetainNoticeFixtureContent()
        {
            using var fixture = new ControllerFixture();
            fixture.Activate();
            yield return null;
            fixture.OpenButton.onClick.Invoke();
            fixture.EntryButtons[0].onClick.Invoke();

            string noticeTitle = ReadText(fixture.DetailTitle);
            string noticeExplanation = ReadText(fixture.DetailExplanation);
            Assert.That(noticeTitle, Is.Not.Empty);
            Assert.That(noticeExplanation, Is.Not.Empty);
            Assert.That(fixture.DetailReviewButton.gameObject.activeSelf, Is.True);

            fixture.DetailBackButton.onClick.Invoke();
            Assert.That(ReadProperty<string>(fixture.Controller, "SelectedEntryId"), Is.Empty);
            Assert.That(ReadProperty<string>(fixture.Controller, "CurrentDetailTitle"), Is.Empty);
            Assert.That(ReadProperty<string>(fixture.Controller, "CurrentDetailExplanation"), Is.Empty);
            Assert.That(ReadText(fixture.ConfirmTitle), Is.Empty);
            Assert.That(ReadText(fixture.ConfirmSummary), Is.Empty);

            fixture.EntryButtons[1].onClick.Invoke();
            Assert.That(ReadText(fixture.DetailTitle), Is.Not.EqualTo(noticeTitle));
            Assert.That(ReadText(fixture.DetailExplanation), Is.Not.EqualTo(noticeExplanation));
            Assert.That(ReadText(fixture.DetailStatus), Does.Contain("NO VERIFIED SERVICE"));
            Assert.That(fixture.DetailReviewButton.gameObject.activeSelf, Is.False);
            Assert.That(fixture.DetailReviewButton.interactable, Is.False);
            Assert.That((bool)Invoke(fixture.Controller, "OpenReviewConfirm"), Is.False);
            fixture.DetailCloseButton.onClick.Invoke();
            AssertOnlyPanel(fixture, "Closed");
            Assert.That(ReadText(fixture.DetailTitle), Is.Empty);
            Assert.That(ReadText(fixture.DetailExplanation), Is.Empty);
            AssertNoStageRunOrSceneChange(fixture);
        }

        [UnityTest]
        public IEnumerator DisableEnableKeepsBalancedListenersAndExactOnceAcknowledgement()
        {
            using var fixture = new ControllerFixture();
            int publicEventCount = 0;
            int serializedEventCount = 0;
            bool callbackSawLatchedState = false;
            bool callbackSawDispatchCount = false;
            string acknowledgedId = string.Empty;
            Action<string> handler = id =>
            {
                publicEventCount++;
                acknowledgedId = id;
            };
            RequireEvent(fixture.Controller.GetType(), "ReviewAcknowledged")
                .AddEventHandler(fixture.Controller, handler);
            var acknowledgedEvent =
                (UnityEvent<string>)ReadProperty(fixture.Controller, "ReviewAcknowledgedEvent");
            acknowledgedEvent.AddListener(_ =>
            {
                serializedEventCount++;
                callbackSawLatchedState =
                    ReadProperty<bool>(fixture.Controller, "IsReviewAcknowledged");
                callbackSawDispatchCount =
                    ReadProperty<int>(fixture.Controller, "AcknowledgementDispatchCount") == 1;
            });

            fixture.Activate();
            yield return null;
            fixture.OpenButton.onClick.Invoke();
            fixture.EntryButtons[0].onClick.Invoke();
            fixture.DetailReviewButton.onClick.Invoke();
            object sessionBeforeDisable = ReadProperty(fixture.Controller, "Session");

            fixture.Root.SetActive(false);
            fixture.Root.SetActive(true);
            yield return null;
            fixture.Root.SetActive(false);
            fixture.Root.SetActive(true);
            yield return null;
            Assert.That(ReadProperty(fixture.Controller, "Session"), Is.SameAs(sessionBeforeDisable));
            AssertOnlyPanel(fixture, "Confirm");

            fixture.ConfirmBackButton.onClick.Invoke();
            AssertPhase(
                fixture.Controller,
                "EntryDetail",
                "A duplicate runtime listener would consume two back levels.");
            fixture.DetailReviewButton.onClick.Invoke();
            fixture.ConfirmAcknowledgeButton.onClick.Invoke();
            fixture.ConfirmAcknowledgeButton.onClick.Invoke();

            Assert.That(ReadProperty<bool>(fixture.Controller, "IsReviewAcknowledged"), Is.True);
            Assert.That(
                ReadProperty<int>(fixture.Controller, "AcknowledgementDispatchCount"),
                Is.EqualTo(1));
            Assert.That(publicEventCount, Is.EqualTo(1));
            Assert.That(serializedEventCount, Is.EqualTo(1));
            Assert.That(callbackSawLatchedState, Is.True, "The latch must precede dispatch.");
            Assert.That(callbackSawDispatchCount, Is.True, "The count must precede dispatch.");
            Assert.That(acknowledgedId, Is.EqualTo(EntryIds[0]));
            Assert.That(
                ReadProperty<string>(fixture.Controller, "LastAcknowledgedEntryId"),
                Is.EqualTo(EntryIds[0]));
            Assert.That((bool)Invoke(fixture.Controller, "AcknowledgeReview"), Is.False);
            Assert.That(ReadText(fixture.ConfirmStatus), Is.EqualTo(
                ReadControllerConstant("ConfirmedReviewStatus")));

            fixture.ConfirmCloseButton.onClick.Invoke();
            AssertOnlyPanel(fixture, "Closed");
            fixture.OpenButton.onClick.Invoke();
            fixture.EntryButtons[0].onClick.Invoke();
            Assert.That(ReadProperty<bool>(fixture.Controller, "IsReviewAcknowledged"), Is.True);
            Assert.That(fixture.DetailReviewButton.gameObject.activeSelf, Is.False);
            Assert.That((bool)Invoke(fixture.Controller, "OpenReviewConfirm"), Is.False);

            fixture.Root.SetActive(false);
            fixture.Root.SetActive(true);
            yield return null;
            Assert.That(ReadProperty<bool>(fixture.Controller, "IsReviewAcknowledged"), Is.True);
            Assert.That(
                ReadProperty<int>(fixture.Controller, "AcknowledgementDispatchCount"),
                Is.EqualTo(1));
            Assert.That(
                fixture.AllButtons.All(button => button.onClick.GetPersistentEventCount() == 0),
                Is.True,
                "OPS-01 owns runtime listeners only.");
            Assert.That(
                acknowledgedEvent.GetPersistentEventCount(),
                Is.Zero,
                "The local acknowledgement event must not contain authored callbacks.");
            AssertNoStageRunOrSceneChange(fixture);
        }

        [UnityTest]
        public IEnumerator FullReviewFlowDoesNotMutateStageRunOrLoadAScene()
        {
            using var fixture = new ControllerFixture();
            int activeSceneHandle = SceneManager.GetActiveScene().handle;
            fixture.Activate();
            yield return null;
            fixture.OpenButton.onClick.Invoke();

            for (int index = 0; index < EntryIds.Length; index++)
            {
                fixture.EntryButtons[index].onClick.Invoke();
                if (index == 0)
                {
                    Assert.That((bool)Invoke(fixture.Controller, "OpenReviewConfirm"), Is.True);
                    Assert.That((bool)Invoke(fixture.Controller, "AcknowledgeReview"), Is.True);
                    Assert.That((bool)Invoke(fixture.Controller, "Back"), Is.True);
                }
                else
                {
                    Assert.That((bool)Invoke(fixture.Controller, "OpenReviewConfirm"), Is.False);
                }

                Assert.That((bool)Invoke(fixture.Controller, "Back"), Is.True);
            }

            Assert.That((bool)Invoke(fixture.Controller, "Close"), Is.True);
            AssertOnlyPanel(fixture, "Closed");
            int finalSceneHandle = SceneManager.GetActiveScene().handle;
            Assert.That(
                finalSceneHandle == activeSceneHandle,
                Is.True,
                $"Active scene handle changed from {activeSceneHandle} to {finalSceneHandle}.");
            Assert.That(StageRunRuntime.HasActiveContext, Is.False);
            Assert.That(StageRunRuntime.ActiveContext, Is.Null);

            string[] forbiddenTypeFragments =
            {
                "UISceneFlowRouter",
                "UISceneRouteLoader",
                "UIPanelRouter",
                "StageRun",
                "Network"
            };
            FieldInfo[] fields = fixture.Controller.GetType().GetFields(
                BindingFlags.Instance | BindingFlags.Static
                | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                Assert.That(
                    forbiddenTypeFragments.Any(fragment =>
                        (field.FieldType.FullName ?? string.Empty).Contains(fragment)),
                    Is.False,
                    $"Forbidden runtime owner field: {field.Name} ({field.FieldType.FullName}).");
            }
        }

        private static void AssertOnlyPanel(ControllerFixture fixture, string expected)
        {
            Assert.That(
                ReadProperty(fixture.Controller, "CurrentPanel").ToString(),
                Is.EqualTo(expected));
            CanvasGroup[] panels =
            {
                fixture.ClosedPanel,
                fixture.DirectoryPanel,
                fixture.DetailPanel,
                fixture.ConfirmPanel
            };
            string[] names = { "Closed", "Directory", "Detail", "Confirm" };
            for (int index = 0; index < panels.Length; index++)
            {
                bool visible = names[index] == expected;
                Assert.That(panels[index].alpha, Is.EqualTo(visible ? 1f : 0f));
                Assert.That(panels[index].interactable, Is.EqualTo(visible));
                Assert.That(panels[index].blocksRaycasts, Is.EqualTo(visible));
            }
        }

        private static void AssertPhase(
            Component controller,
            string expected,
            string message = null)
        {
            Assert.That(
                ReadProperty(controller, "CurrentPhase").ToString(),
                Is.EqualTo(expected),
                message);
        }

        private static void AssertFocused(Button expected)
        {
            Assert.That(EventSystem.current, Is.Not.Null);
            Assert.That(
                EventSystem.current.currentSelectedGameObject,
                Is.EqualTo(expected.gameObject));
        }

        private static void AssertNoStageRunOrSceneChange(ControllerFixture fixture)
        {
            int currentSceneHandle = SceneManager.GetActiveScene().handle;
            Assert.That(
                currentSceneHandle == fixture.InitialSceneHandle,
                Is.True,
                $"Active scene handle changed from {fixture.InitialSceneHandle} "
                + $"to {currentSceneHandle}.");
            Assert.That(StageRunRuntime.HasActiveContext, Is.False);
            Assert.That(StageRunRuntime.ActiveContext, Is.Null);
        }

        private static string ResolveRowLabel(string rowKind)
        {
            return rowKind switch
            {
                "ServerClock" => "SERVER CLOCK",
                _ => rowKind.ToUpperInvariant()
            };
        }

        private static string ResolveDispositionLabel(string disposition)
        {
            return disposition switch
            {
                "LocalReviewFixture" => "LOCAL REVIEW FIXTURE",
                "ReviewShellNoProductCommitment" =>
                    "REVIEW SHELL / NO PRODUCT COMMITMENT",
                "DefinitionOnlyReviewShell" => "DEFINITION-ONLY REVIEW SHELL",
                "NotRequiredForReview" => "NOT REQUIRED FOR REVIEW",
                "NoVerifiedSource" => "NO VERIFIED SOURCE",
                "DefinitionOnlyNoVerdict" => "DEFINITION ONLY / NO VERDICT",
                "LocalReviewConfirm" => "LOCAL REVIEW CONFIRM",
                "ExplanationOnly" => "EXPLANATION ONLY",
                _ => string.Empty
            };
        }

        private sealed class ControllerFixture : IDisposable
        {
            private readonly Type entryBindingType;
            private readonly Type dispositionBindingType;
            private readonly Type rowKindType;
            private readonly GameObject ownedEventSystem;

            public ControllerFixture()
            {
                InitialSceneHandle = SceneManager.GetActiveScene().handle;
                if (EventSystem.current == null)
                {
                    ownedEventSystem = new GameObject(
                        "LobbyOperationsReviewEventSystem",
                        typeof(EventSystem));
                }

                EventSystem.current?.SetSelectedGameObject(null);

                Type profileType = RequireProductType(ProfileTypeName);
                Profile = ScriptableObject.CreateInstance(profileType);
                Array defaultEntries =
                    (Array)InvokeStatic(profileType, "CreateDefaultEntries");
                Invoke(Profile, "Configure", defaultEntries);
                object[] validationArguments = { null };
                Assert.That(
                    (bool)Invoke(Profile, "TryValidate", validationArguments),
                    Is.True,
                    validationArguments[0]?.ToString());

                Root = new GameObject("LobbyOperationsReviewControllerTest");
                Root.SetActive(false);
                Controller = Root.AddComponent(RequireProductType(ControllerTypeName));
                ClosedPanel = CreateGroup(Root.transform, "ClosedPanel");
                DirectoryPanel = CreateGroup(Root.transform, "DirectoryPanel");
                DetailPanel = CreateGroup(Root.transform, "DetailPanel");
                ConfirmPanel = CreateGroup(Root.transform, "ConfirmPanel");

                Component closedLabel = CreateText(ClosedPanel.transform, "ClosedLabel");
                Component closedStatus = CreateText(ClosedPanel.transform, "ClosedStatus");
                OpenButton = CreateButton(ClosedPanel.transform, "OpenButton", true);

                Component directoryTitle = CreateText(DirectoryPanel.transform, "DirectoryTitle");
                Component directoryStatus = CreateText(DirectoryPanel.transform, "DirectoryStatus");
                DirectoryBackButton = CreateButton(DirectoryPanel.transform, "DirectoryBack");
                DirectoryCloseButton = CreateButton(DirectoryPanel.transform, "DirectoryClose");

                Type controllerType = Controller.GetType();
                entryBindingType = controllerType.GetNestedType(
                    "EntryButtonBinding",
                    BindingFlags.Public);
                dispositionBindingType = controllerType.GetNestedType(
                    "DispositionRowBinding",
                    BindingFlags.Public);
                rowKindType = RequireProductType(RowKindTypeName);
                Assert.That(entryBindingType, Is.Not.Null);
                Assert.That(dispositionBindingType, Is.Not.Null);

                EntryButtons = new Button[EntryIds.Length];
                EntryGroups = new CanvasGroup[EntryIds.Length];
                EntryTitles = new Component[EntryIds.Length];
                EntryStatuses = new Component[EntryIds.Length];
                Array entryBindings = Array.CreateInstance(entryBindingType, EntryIds.Length);
                for (int index = 0; index < EntryIds.Length; index++)
                {
                    Button button = CreateButton(
                        DirectoryPanel.transform,
                        $"Entry{index}");
                    CanvasGroup group = button.gameObject.AddComponent<CanvasGroup>();
                    Component title = CreateText(button.transform, $"Entry{index}Title");
                    Component sourceStatus = CreateText(
                        button.transform,
                        $"Entry{index}SourceStatus");
                    EntryButtons[index] = button;
                    EntryGroups[index] = group;
                    EntryTitles[index] = title;
                    EntryStatuses[index] = sourceStatus;
                    entryBindings.SetValue(
                        CreateEntryBinding(
                            EntryIds[index],
                            button,
                            group,
                            title,
                            sourceStatus),
                        index);
                }

                DetailKind = CreateText(DetailPanel.transform, "DetailKind");
                DetailTitle = CreateText(DetailPanel.transform, "DetailTitle");
                DetailExplanation = CreateText(DetailPanel.transform, "DetailExplanation");
                DetailStatus = CreateText(DetailPanel.transform, "DetailStatus");
                DetailBackButton = CreateButton(DetailPanel.transform, "DetailBack");
                DetailCloseButton = CreateButton(DetailPanel.transform, "DetailClose");
                DetailReviewButton = CreateButton(
                    DetailPanel.transform,
                    "DetailReview",
                    true);

                DispositionRoots = new GameObject[DispositionKinds.Length];
                DispositionLabels = new Component[DispositionKinds.Length];
                DispositionValues = new Component[DispositionKinds.Length];
                Array dispositionBindings = Array.CreateInstance(
                    dispositionBindingType,
                    DispositionKinds.Length);
                for (int index = 0; index < DispositionKinds.Length; index++)
                {
                    GameObject row = CreateRow(
                        DetailPanel.transform,
                        $"{DispositionKinds[index]}Row");
                    Component label = CreateText(row.transform, "Label");
                    Component value = CreateText(row.transform, "Value");
                    DispositionRoots[index] = row;
                    DispositionLabels[index] = label;
                    DispositionValues[index] = value;
                    dispositionBindings.SetValue(
                        CreateDispositionBinding(
                            DispositionKinds[index],
                            row,
                            label,
                            value),
                        index);
                }

                ConfirmTitle = CreateText(ConfirmPanel.transform, "ConfirmTitle");
                ConfirmSummary = CreateText(ConfirmPanel.transform, "ConfirmSummary");
                ConfirmStatus = CreateText(ConfirmPanel.transform, "ConfirmStatus");
                ConfirmBackButton = CreateButton(ConfirmPanel.transform, "ConfirmBack");
                ConfirmCloseButton = CreateButton(ConfirmPanel.transform, "ConfirmClose");
                ConfirmAcknowledgeButton = CreateButton(
                    ConfirmPanel.transform,
                    "ConfirmAcknowledge",
                    true);

                Invoke(Controller, "ConfigureCore", Profile);
                Invoke(
                    Controller,
                    "ConfigurePanels",
                    ClosedPanel,
                    DirectoryPanel,
                    DetailPanel,
                    ConfirmPanel);
                Invoke(
                    Controller,
                    "ConfigureClosedView",
                    closedLabel,
                    closedStatus,
                    OpenButton);
                Invoke(
                    Controller,
                    "ConfigureDirectoryView",
                    directoryTitle,
                    directoryStatus,
                    DirectoryBackButton,
                    DirectoryCloseButton,
                    entryBindings);
                Invoke(
                    Controller,
                    "ConfigureDetailView",
                    DetailKind,
                    DetailTitle,
                    DetailExplanation,
                    DetailStatus,
                    dispositionBindings,
                    DetailBackButton,
                    DetailCloseButton,
                    DetailReviewButton);
                Invoke(
                    Controller,
                    "ConfigureConfirmationView",
                    ConfirmTitle,
                    ConfirmSummary,
                    ConfirmStatus,
                    ConfirmBackButton,
                    ConfirmCloseButton,
                    ConfirmAcknowledgeButton);

                AllButtons = new List<Button>
                {
                    OpenButton,
                    DirectoryBackButton,
                    DirectoryCloseButton,
                    DetailBackButton,
                    DetailCloseButton,
                    DetailReviewButton,
                    ConfirmBackButton,
                    ConfirmCloseButton,
                    ConfirmAcknowledgeButton
                };
                AllButtons.AddRange(EntryButtons);
            }

            public int InitialSceneHandle { get; }
            public GameObject Root { get; }
            public Component Controller { get; }
            public ScriptableObject Profile { get; }
            public CanvasGroup ClosedPanel { get; }
            public CanvasGroup DirectoryPanel { get; }
            public CanvasGroup DetailPanel { get; }
            public CanvasGroup ConfirmPanel { get; }
            public Button OpenButton { get; }
            public Button DirectoryBackButton { get; }
            public Button DirectoryCloseButton { get; }
            public Button[] EntryButtons { get; }
            public CanvasGroup[] EntryGroups { get; }
            public Component[] EntryTitles { get; }
            public Component[] EntryStatuses { get; }
            public Component DetailKind { get; }
            public Component DetailTitle { get; }
            public Component DetailExplanation { get; }
            public Component DetailStatus { get; }
            public Button DetailBackButton { get; }
            public Button DetailCloseButton { get; }
            public Button DetailReviewButton { get; }
            public GameObject[] DispositionRoots { get; }
            public Component[] DispositionLabels { get; }
            public Component[] DispositionValues { get; }
            public Component ConfirmTitle { get; }
            public Component ConfirmSummary { get; }
            public Component ConfirmStatus { get; }
            public Button ConfirmBackButton { get; }
            public Button ConfirmCloseButton { get; }
            public Button ConfirmAcknowledgeButton { get; }
            public List<Button> AllButtons { get; }

            public void Activate()
            {
                Root.SetActive(true);
            }

            public void ReplaceDirectoryBindingsWithMissingStatusText()
            {
                Array bindings = Array.CreateInstance(entryBindingType, EntryIds.Length);
                for (int index = 0; index < EntryIds.Length; index++)
                {
                    bindings.SetValue(
                        CreateEntryBinding(
                            EntryIds[index],
                            EntryButtons[index],
                            EntryGroups[index],
                            EntryTitles[index],
                            index == 0 ? null : EntryStatuses[index]),
                        index);
                }

                Invoke(
                    Controller,
                    "ConfigureDirectoryView",
                    CreateText(DirectoryPanel.transform, "ReplacementTitle"),
                    CreateText(DirectoryPanel.transform, "ReplacementStatus"),
                    DirectoryBackButton,
                    DirectoryCloseButton,
                    bindings);
            }

            public void ReplaceDispositionRowsWithMissingValueText()
            {
                Array bindings = Array.CreateInstance(
                    dispositionBindingType,
                    DispositionKinds.Length);
                for (int index = 0; index < DispositionKinds.Length; index++)
                {
                    bindings.SetValue(
                        CreateDispositionBinding(
                            DispositionKinds[index],
                            DispositionRoots[index],
                            DispositionLabels[index],
                            index == 0 ? null : DispositionValues[index]),
                        index);
                }

                Invoke(
                    Controller,
                    "ConfigureDetailView",
                    DetailKind,
                    DetailTitle,
                    DetailExplanation,
                    DetailStatus,
                    bindings,
                    DetailBackButton,
                    DetailCloseButton,
                    DetailReviewButton);
            }

            public void Dispose()
            {
                if (EventSystem.current != null
                    && EventSystem.current.currentSelectedGameObject != null
                    && EventSystem.current.currentSelectedGameObject.transform.IsChildOf(
                        Root.transform))
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }

                UnityEngine.Object.DestroyImmediate(Root);
                UnityEngine.Object.DestroyImmediate(Profile);
                if (ownedEventSystem != null)
                {
                    UnityEngine.Object.DestroyImmediate(ownedEventSystem);
                }
            }

            private object CreateEntryBinding(
                string entryId,
                Button button,
                CanvasGroup group,
                Component title,
                Component sourceStatus)
            {
                object binding = Activator.CreateInstance(entryBindingType);
                Invoke(binding, "Configure", entryId, button, group, title, sourceStatus);
                return binding;
            }

            private object CreateDispositionBinding(
                string rowKind,
                GameObject row,
                Component label,
                Component value)
            {
                object binding = Activator.CreateInstance(dispositionBindingType);
                Invoke(
                    binding,
                    "Configure",
                    Enum.Parse(rowKindType, rowKind),
                    row,
                    label,
                    value);
                return binding;
            }
        }

        private static CanvasGroup CreateGroup(Transform parent, string name)
        {
            var owner = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            owner.transform.SetParent(parent, false);
            return owner.GetComponent<CanvasGroup>();
        }

        private static Button CreateButton(
            Transform parent,
            string name,
            bool createLabel = false)
        {
            var owner = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            owner.transform.SetParent(parent, false);
            Button button = owner.GetComponent<Button>();
            if (createLabel)
            {
                CreateText(owner.transform, "Label");
            }

            return button;
        }

        private static GameObject CreateRow(Transform parent, string name)
        {
            var owner = new GameObject(name, typeof(RectTransform));
            owner.transform.SetParent(parent, false);
            return owner;
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

        private static string ReadText(Component text)
        {
            return ReadProperty<string>(text, "text");
        }

        private static string ReadControllerConstant(string name)
        {
            FieldInfo field = RequireProductType(ControllerTypeName).GetField(
                name,
                BindingFlags.Public | BindingFlags.Static);
            Assert.That(field, Is.Not.Null, name);
            return (string)field.GetRawConstantValue();
        }

        private static Type RequireProductType(string fullName)
        {
            Type type = Type.GetType(fullName + ", Assembly-CSharp")
                ?? Type.GetType(fullName + ", DimensionBrawl.Runtime");
            Assert.That(type, Is.Not.Null, $"Missing product type {fullName}.");
            return type;
        }

        private static EventInfo RequireEvent(Type type, string eventName)
        {
            EventInfo eventInfo = type.GetEvent(
                eventName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(eventInfo, Is.Not.Null, $"Missing event {type.Name}.{eventName}.");
            return eventInfo;
        }

        private static MethodInfo RequireMethod(
            Type type,
            string methodName,
            int parameterCount,
            bool isStatic)
        {
            MethodInfo[] methods = type.GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic
                    | (isStatic ? BindingFlags.Static : BindingFlags.Instance))
                .Where(method => method.Name == methodName
                    && method.GetParameters().Length == parameterCount)
                .ToArray();
            Assert.That(
                methods.Length,
                Is.EqualTo(1),
                $"Expected one {type.Name}.{methodName}/{parameterCount} overload.");
            return methods[0];
        }

        private static object Invoke(object target, string methodName, params object[] arguments)
        {
            Assert.That(target, Is.Not.Null);
            object[] safeArguments = arguments ?? Array.Empty<object>();
            return RequireMethod(target.GetType(), methodName, safeArguments.Length, false)
                .Invoke(target, safeArguments);
        }

        private static object InvokeStatic(
            Type type,
            string methodName,
            params object[] arguments)
        {
            object[] safeArguments = arguments ?? Array.Empty<object>();
            return RequireMethod(type, methodName, safeArguments.Length, true)
                .Invoke(null, safeArguments);
        }

        private static object ReadProperty(object target, string propertyName)
        {
            Assert.That(target, Is.Not.Null);
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(
                property,
                Is.Not.Null,
                $"Missing property {target.GetType().Name}.{propertyName}.");
            return property.GetValue(target);
        }

        private static T ReadProperty<T>(object target, string propertyName)
        {
            object value = ReadProperty(target, propertyName);
            return value == null ? default : (T)value;
        }
    }
}
