using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using DimensionBrawl.LevelDesign;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class OlympusChapterHubReviewControllerPlayModeTests
    {
        private const string ControllerTypeName =
            "DimensionBrawl.UI.ChapterHubReview.OlympusChapterHubReviewController";
        private const string ProfileTypeName =
            "DimensionBrawl.UI.ChapterHubReview.ChapterHubReviewProfile";
        private const string ContentStatusTypeName =
            "DimensionBrawl.UI.ChapterHubReview.ChapterHubReviewContentStatus";
        private const string UiRouteIdTypeName = "DimensionBrawl.UI.UIRouteId";
        private const string RouterTypeName = "DimensionBrawl.UI.UISceneFlowRouter";
        private const string StageCatalogPath =
            "Assets/_Game/DesignData/UI/DB_UIStageCatalog.asset";
        private const string ChapterId = "review.olympus.chapter";
        private const string ActualStageId = "review.olympus.actual";
        private const string PlannedStageId = "review.olympus.in-production";
        private const string AnnouncedStageId = "review.olympus.announced";

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
        public IEnumerator ActualNodeRendersFreshCanonicalBriefingAndOnlyVerifiedRows()
        {
            using var fixture = new ControllerFixture();
            fixture.Activate();
            yield return null;

            fixture.ChapterButton.onClick.Invoke();
            string canonicalMapTitle = ReadText(fixture.ActualNodeTitle);
            Assert.That(
                canonicalMapTitle,
                Is.Not.EqualTo("LOCAL FALLBACK MUST NOT REPLACE CANONICAL TITLE"));
            fixture.ActualNodeButton.onClick.Invoke();

            AssertPhase(fixture.Controller, "StageDetail");
            AssertPanel(fixture, "StageDetail");
            object firstProjection = ReadProperty(fixture.Controller, "CurrentProjection");
            Assert.That(firstProjection, Is.Not.Null);
            object[] currentArguments =
            {
                firstProjection,
                ResolveEnum(UiRouteIdTypeName, 40),
                null
            };
            Assert.That(
                (bool)Invoke(fixture.Catalog, "IsProjectionCurrent", currentArguments),
                Is.True,
                currentArguments[2]?.ToString());

            StageBriefingReadModel briefing =
                (StageBriefingReadModel)ReadProperty(firstProjection, "Briefing");
            Assert.That(canonicalMapTitle, Is.EqualTo(briefing.Title));
            Assert.That(briefing.TitleDisposition, Is.EqualTo(StageBriefingValueDisposition.Present));
            Assert.That(fixture.DetailTitle.gameObject.activeSelf, Is.True);
            Assert.That(ReadText(fixture.DetailTitle), Is.EqualTo(briefing.Title));
            Assert.That(fixture.ObjectiveRow.activeSelf, Is.True);
            Assert.That(ReadText(fixture.ObjectiveText), Is.EqualTo(briefing.Objective));
            Assert.That(fixture.CombatLessonRow.activeSelf, Is.True);
            Assert.That(ReadText(fixture.CombatLessonText), Is.EqualTo(briefing.CombatLesson));
            Assert.That(
                briefing.StoryEntryDisposition,
                Is.EqualTo(StageReferenceDisposition.Present));
            Assert.That(fixture.StoryRow.activeSelf, Is.True);
            Assert.That(ReadText(fixture.StoryText), Does.Contain(briefing.StoryEntrySegmentId));
            Assert.That(briefing.SegmentCount, Is.GreaterThan(0));
            Assert.That(fixture.SegmentRow.activeSelf, Is.True);
            Assert.That(
                ReadText(fixture.SegmentText),
                Does.Contain(briefing.GetSegment(0).RouteSegmentId));
            Assert.That(fixture.DetailAvailability.gameObject.activeSelf, Is.False);
            fixture.AssertEveryUnverifiedRowHidden();
            Assert.That(fixture.DetailReviewButton.gameObject.activeSelf, Is.True);
            Assert.That(fixture.DetailReviewButton.interactable, Is.True);

            int refreshCount = ReadProperty<int>(fixture.Controller, "ProjectionRefreshCount");
            Invoke(fixture.Controller, "RefreshCurrentView");
            object refreshedProjection = ReadProperty(fixture.Controller, "CurrentProjection");
            Assert.That(refreshedProjection, Is.Not.Null);
            Assert.That(refreshedProjection, Is.Not.SameAs(firstProjection));
            Assert.That(
                ReadProperty<int>(fixture.Controller, "ProjectionRefreshCount"),
                Is.GreaterThan(refreshCount));
        }

        [UnityTest]
        public IEnumerator PlannedAndAnnouncedNodesOpenDetailButCannotOpenConfirmation()
        {
            using var fixture = new ControllerFixture();
            fixture.Activate();
            yield return null;

            fixture.ChapterButton.onClick.Invoke();
            Assert.That(
                ReadText(fixture.PlannedNodeStatus),
                Is.EqualTo(ReadControllerConstant("PlannedReviewStatus")));
            Assert.That(
                ReadText(fixture.AnnouncedNodeStatus),
                Is.EqualTo(ReadControllerConstant("AnnouncedReviewStatus")));

            fixture.PlannedNodeButton.onClick.Invoke();
            AssertPhase(fixture.Controller, "StageDetail");
            Assert.That(
                ReadText(fixture.DetailStatus),
                Is.EqualTo(ReadControllerConstant("PlannedReviewStatus")));
            Assert.That(ReadText(fixture.DetailTitle), Is.EqualTo("LOCAL IN-PRODUCTION SAMPLE"));
            Assert.That(fixture.DetailAvailability.gameObject.activeSelf, Is.True);
            Assert.That(ReadText(fixture.DetailAvailability), Does.Contain("제작 중"));
            Assert.That(fixture.DetailReviewButton.gameObject.activeSelf, Is.False);
            Assert.That(fixture.DetailReviewButton.interactable, Is.False);
            Assert.That(ReadProperty(fixture.Controller, "CurrentProjection"), Is.Null);
            Assert.That((bool)Invoke(fixture.Controller, "OpenReviewConfirm"), Is.False);
            AssertPhase(fixture.Controller, "StageDetail");

            Assert.That((bool)Invoke(fixture.Controller, "NavigateBack"), Is.True);
            fixture.AnnouncedNodeButton.onClick.Invoke();
            Assert.That(
                ReadText(fixture.DetailStatus),
                Is.EqualTo(ReadControllerConstant("AnnouncedReviewStatus")));
            Assert.That(ReadText(fixture.DetailTitle), Is.EqualTo("LOCAL ANNOUNCED SAMPLE"));
            Assert.That(fixture.DetailAvailability.gameObject.activeSelf, Is.True);
            Assert.That(ReadText(fixture.DetailAvailability), Does.Contain("공지 슬롯"));
            Assert.That(fixture.DetailReviewButton.gameObject.activeSelf, Is.False);
            Assert.That(fixture.DetailReviewButton.interactable, Is.False);
            Assert.That((bool)Invoke(fixture.Controller, "OpenReviewConfirm"), Is.False);
            AssertPanel(fixture, "StageDetail");
        }

        [UnityTest]
        public IEnumerator BackNavigationFollowsConfirmDetailMapHubHierarchy()
        {
            using var fixture = new ControllerFixture();
            fixture.Activate();
            yield return null;
            fixture.OpenActualDetailWithButtons();

            fixture.DetailReviewButton.onClick.Invoke();
            AssertPhase(fixture.Controller, "ReviewConfirm");
            AssertPanel(fixture, "ReviewConfirm");
            fixture.ConfirmBackButton.onClick.Invoke();
            AssertPanel(fixture, "StageDetail");
            fixture.DetailBackButton.onClick.Invoke();
            AssertPanel(fixture, "StageMap");
            fixture.MapBackButton.onClick.Invoke();
            AssertPhase(fixture.Controller, "Overview");
            AssertPanel(fixture, "ChapterHub");
        }

        [UnityTest]
        public IEnumerator ReenableDoesNotDuplicateBackButtonListener()
        {
            using var fixture = new ControllerFixture();
            fixture.Activate();
            yield return null;
            fixture.OpenActualDetailWithButtons();

            fixture.Root.SetActive(false);
            fixture.Root.SetActive(true);
            yield return null;
            AssertPanel(fixture, "StageDetail");
            fixture.DetailBackButton.onClick.Invoke();

            AssertPhase(
                fixture.Controller,
                "StageMap",
                "A duplicated listener would consume two hierarchy levels in one click.");
            AssertPanel(fixture, "StageMap");
        }

        [UnityTest]
        public IEnumerator RapidConfirmationDispatchesOnceWithoutRouteOrStageRunSideEffects()
        {
            using var fixture = new ControllerFixture();
            int activeSceneHandle = SceneManager.GetActiveScene().handle;
            int publicEventCount = 0;
            int serializedEventCount = 0;
            Action<string> handler = _ => publicEventCount++;
            RequireEvent(fixture.Controller.GetType(), "ReviewConfirmed")
                .AddEventHandler(fixture.Controller, handler);
            var confirmationEvent =
                (UnityEvent<string>)ReadProperty(fixture.Controller, "ConfirmationEvent");
            confirmationEvent.AddListener(_ => serializedEventCount++);
            fixture.Activate();
            yield return null;

            fixture.OpenActualDetailWithButtons();
            fixture.DetailReviewButton.onClick.Invoke();
            fixture.ConfirmAcceptButton.onClick.Invoke();
            fixture.ConfirmAcceptButton.onClick.Invoke();

            Assert.That(
                ReadProperty<int>(fixture.Controller, "ConfirmationDispatchCount"),
                Is.EqualTo(1));
            Assert.That(publicEventCount, Is.EqualTo(1));
            Assert.That(serializedEventCount, Is.EqualTo(1));
            Assert.That((bool)Invoke(fixture.Controller, "ConfirmSelectedStage"), Is.False);
            Assert.That(
                ReadProperty<string>(fixture.Controller, "LastConfirmedCatalogEntryId"),
                Is.EqualTo(fixture.CanonicalCatalogEntryId));
            Assert.That(ReadProperty<int>(fixture.Router, "RouteRequestCount"), Is.Zero);
            Assert.That(StageRunRuntime.HasActiveContext, Is.False);
            Assert.That(StageRunRuntime.ActiveContext, Is.Null);
            Assert.That(
                SceneManager.GetActiveScene().handle == activeSceneHandle,
                Is.True,
                "Review confirmation must not load or replace the active scene.");
            Assert.That(
                ReadText(fixture.ConfirmStatus),
                Is.EqualTo(ReadControllerConstant("ConfirmedReviewStatus")));

            fixture.ConfirmBackButton.onClick.Invoke();
            AssertPhase(fixture.Controller, "Overview");
            AssertPanel(fixture, "ChapterHub");
            Assert.That(
                ReadProperty<int>(fixture.Controller, "ConfirmationDispatchCount"),
                Is.Zero);
            Assert.That(ReadProperty<int>(fixture.Router, "RouteRequestCount"), Is.Zero);
            Assert.That(StageRunRuntime.HasActiveContext, Is.False);
        }

        [UnityTest]
        public IEnumerator UnknownCanonicalCatalogEntryFailsClosedWithoutChangingPhase()
        {
            using var fixture = new ControllerFixture("missing.catalog.entry");
            fixture.Activate();
            yield return null;

            fixture.ChapterButton.onClick.Invoke();
            AssertPhase(fixture.Controller, "StageMap");
            Assert.That(fixture.ActualNodeButton.interactable, Is.False);
            Assert.That(ReadText(fixture.ActualNodeTitle), Is.Empty);
            Assert.That(
                ReadText(fixture.ActualNodeStatus),
                Is.EqualTo("CANONICAL DATA UNAVAILABLE"));
            Assert.That((bool)Invoke(fixture.Controller, "SelectStage", ActualStageId), Is.True);

            Assert.That(ReadProperty(fixture.Controller, "CurrentProjection"), Is.Null);
            Assert.That(fixture.DetailAvailability.gameObject.activeSelf, Is.True);
            Assert.That(fixture.DetailReviewButton.gameObject.activeSelf, Is.False);
            Assert.That(fixture.DetailReviewButton.interactable, Is.False);
            Assert.That((bool)Invoke(fixture.Controller, "OpenReviewConfirm"), Is.False);
            AssertPhase(fixture.Controller, "StageDetail");
            Assert.That(
                ReadProperty(fixture.Controller, "LastProjectionRejectReason").ToString(),
                Is.EqualTo("CatalogEntryNotFound"));
            Assert.That(ReadProperty<int>(fixture.Router, "RouteRequestCount"), Is.Zero);
            Assert.That(StageRunRuntime.HasActiveContext, Is.False);
        }

        [UnityTest]
        public IEnumerator StaleCatalogGenerationFailsClosedBeforeConfirmation()
        {
            using var fixture = new ControllerFixture(cloneCatalog: true);
            fixture.Activate();
            yield return null;
            fixture.OpenActualDetailWithButtons();
            Assert.That(ReadProperty(fixture.Controller, "CurrentProjection"), Is.Not.Null);

            var serializedCatalog = new SerializedObject(fixture.Catalog);
            SerializedProperty generation = serializedCatalog.FindProperty(
                "catalogProjectionGeneration");
            Assert.That(generation, Is.Not.Null);
            generation.intValue++;
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();

            Assert.That((bool)Invoke(fixture.Controller, "OpenReviewConfirm"), Is.False);
            Assert.That(ReadProperty(fixture.Controller, "CurrentProjection"), Is.Null);
            AssertPhase(fixture.Controller, "StageDetail");
            Assert.That(
                ReadProperty(fixture.Controller, "LastProjectionRejectReason").ToString(),
                Is.Not.EqualTo("None"));
            Assert.That(fixture.DetailTitle.gameObject.activeSelf, Is.False);
            Assert.That(fixture.DetailAvailability.gameObject.activeSelf, Is.True);
            Assert.That(fixture.ObjectiveRow.activeSelf, Is.False);
            Assert.That(fixture.CombatLessonRow.activeSelf, Is.False);
            Assert.That(
                ReadText(fixture.DetailStatus),
                Does.StartWith("CANONICAL DETAIL UNAVAILABLE /"));
            Assert.That(fixture.DetailReviewButton.gameObject.activeSelf, Is.False);
            Assert.That(fixture.DetailReviewButton.interactable, Is.False);
            Assert.That(ReadProperty<int>(fixture.Router, "RouteRequestCount"), Is.Zero);
            Assert.That(StageRunRuntime.HasActiveContext, Is.False);

            Assert.That((bool)Invoke(fixture.Controller, "NavigateBack"), Is.True);
            AssertPhase(fixture.Controller, "StageMap");
            Assert.That(fixture.ActualNodeButton.interactable, Is.False);
            Assert.That(ReadText(fixture.ActualNodeTitle), Is.Empty);
            Assert.That(
                ReadText(fixture.ActualNodeStatus),
                Is.EqualTo("CANONICAL DATA UNAVAILABLE"));
        }

        [UnityTest]
        public IEnumerator StaleCatalogGenerationInsideConfirmationClearsRenderedContent()
        {
            using var fixture = new ControllerFixture(cloneCatalog: true);
            fixture.Activate();
            yield return null;
            fixture.OpenActualDetailWithButtons();
            Assert.That((bool)Invoke(fixture.Controller, "OpenReviewConfirm"), Is.True);
            Assert.That(fixture.ConfirmTitle.gameObject.activeSelf, Is.True);

            var serializedCatalog = new SerializedObject(fixture.Catalog);
            SerializedProperty generation = serializedCatalog.FindProperty(
                "catalogProjectionGeneration");
            Assert.That(generation, Is.Not.Null);
            generation.intValue++;
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();

            Assert.That((bool)Invoke(fixture.Controller, "ConfirmSelectedStage"), Is.False);
            AssertPhase(fixture.Controller, "ReviewConfirm");
            Assert.That(ReadProperty(fixture.Controller, "CurrentProjection"), Is.Null);
            Assert.That(fixture.ConfirmTitle.gameObject.activeSelf, Is.False);
            Assert.That(fixture.ConfirmSummary.gameObject.activeSelf, Is.False);
            Assert.That(
                ReadText(fixture.ConfirmStatus),
                Does.StartWith("CANONICAL DETAIL UNAVAILABLE /"));
            Assert.That(
                ReadProperty<int>(fixture.Controller, "ConfirmationDispatchCount"),
                Is.Zero);
            Assert.That(ReadProperty<int>(fixture.Router, "RouteRequestCount"), Is.Zero);
            Assert.That(StageRunRuntime.HasActiveContext, Is.False);
        }

        private static void AssertPanel(ControllerFixture fixture, string expected)
        {
            Assert.That(
                ReadProperty(fixture.Controller, "CurrentPanel").ToString(),
                Is.EqualTo(expected));
            Assert.That(
                fixture.ChapterHubPanel.alpha,
                Is.EqualTo(expected == "ChapterHub" ? 1f : 0f));
            Assert.That(
                fixture.StageMapPanel.alpha,
                Is.EqualTo(expected == "StageMap" ? 1f : 0f));
            Assert.That(
                fixture.StageDetailPanel.alpha,
                Is.EqualTo(expected == "StageDetail" ? 1f : 0f));
            Assert.That(
                fixture.ReviewConfirmPanel.alpha,
                Is.EqualTo(expected == "ReviewConfirm" ? 1f : 0f));
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

        private sealed class ControllerFixture : IDisposable
        {
            private readonly bool ownsCatalog;

            public ControllerFixture(
                string canonicalCatalogEntryId = null,
                bool cloneCatalog = false)
            {
                Type profileType = RequireProductType(ProfileTypeName);
                Type chapterType = profileType.GetNestedType(
                    "ChapterDefinition",
                    BindingFlags.Public);
                Type stageType = profileType.GetNestedType(
                    "StageDefinition",
                    BindingFlags.Public);
                Assert.That(chapterType, Is.Not.Null);
                Assert.That(stageType, Is.Not.Null);

                ScriptableObject sourceCatalog =
                    AssetDatabase.LoadAssetAtPath<ScriptableObject>(StageCatalogPath);
                Assert.That(sourceCatalog, Is.Not.Null, StageCatalogPath);
                Catalog = cloneCatalog
                    ? UnityEngine.Object.Instantiate(sourceCatalog)
                    : sourceCatalog;
                ownsCatalog = cloneCatalog;
                Assert.That(ReadProperty<int>(Catalog, "StageCount"), Is.GreaterThanOrEqualTo(1));
                object catalogEntry = Invoke(Catalog, "GetStage", 0);
                Assert.That(
                    ReadProperty<string>(catalogEntry, "Id"),
                    Is.EqualTo("story_v1_training_route"));
                CanonicalCatalogEntryId = canonicalCatalogEntryId
                    ?? ReadProperty<string>(catalogEntry, "Id");

                Profile = ScriptableObject.CreateInstance(profileType);
                Profile.name = "OlympusChapterHubReviewControllerTestProfile";
                object chapter = Activator.CreateInstance(chapterType);
                Invoke(
                    chapter,
                    "Configure",
                    ChapterId,
                    "EPISODE 00",
                    "review.olympus.chapter.title",
                    "OLYMPUS REVIEW");
                Array chapters = Array.CreateInstance(chapterType, 1);
                chapters.SetValue(chapter, 0);

                Array stages = Array.CreateInstance(stageType, 3);
                stages.SetValue(
                    CreateStage(
                        stageType,
                        ActualStageId,
                        "00-01",
                        "LOCAL FALLBACK MUST NOT REPLACE CANONICAL TITLE",
                        new Vector2(0.22f, 0.58f),
                        "CanonicalPlayable",
                        CanonicalCatalogEntryId),
                    0);
                stages.SetValue(
                    CreateStage(
                        stageType,
                        PlannedStageId,
                        "00-02",
                        "LOCAL IN-PRODUCTION SAMPLE",
                        new Vector2(0.52f, 0.44f),
                        "InProduction",
                        string.Empty),
                    1);
                stages.SetValue(
                    CreateStage(
                        stageType,
                        AnnouncedStageId,
                        "00-03",
                        "LOCAL ANNOUNCED SAMPLE",
                        new Vector2(0.78f, 0.62f),
                        "Announced",
                        string.Empty),
                    2);
                Invoke(Profile, "Configure", chapters, stages);
                object[] validateArguments = { null };
                Assert.That(
                    (bool)Invoke(Profile, "TryValidate", validateArguments),
                    Is.True,
                    validateArguments[0]?.ToString());

                Root = new GameObject("OlympusChapterHubReviewControllerTest");
                Root.SetActive(false);
                Controller = Root.AddComponent(RequireProductType(ControllerTypeName));
                Router = Root.AddComponent(RequireProductType(RouterTypeName));
                ChapterHubPanel = CreateGroup(Root.transform, "ChapterHubPanel");
                StageMapPanel = CreateGroup(Root.transform, "StageMapPanel");
                StageDetailPanel = CreateGroup(Root.transform, "StageDetailPanel");
                ReviewConfirmPanel = CreateGroup(Root.transform, "ReviewConfirmPanel");

                ChapterButton = CreateButton(ChapterHubPanel.transform, "ChapterButton");
                CanvasGroup chapterBindingGroup = ChapterButton.gameObject.AddComponent<CanvasGroup>();
                Component chapterEpisode = CreateText(ChapterButton.transform, "ChapterEpisode");
                Component chapterTitle = CreateText(ChapterButton.transform, "ChapterTitle");

                MapBackButton = CreateButton(StageMapPanel.transform, "MapBackButton");
                ActualNodeButton = CreateButton(StageMapPanel.transform, "ActualNodeButton");
                PlannedNodeButton = CreateButton(StageMapPanel.transform, "PlannedNodeButton");
                AnnouncedNodeButton = CreateButton(StageMapPanel.transform, "AnnouncedNodeButton");
                StageNodeFixture actualNode = CreateStageNode(ActualNodeButton, "ActualNode");
                StageNodeFixture plannedNode = CreateStageNode(PlannedNodeButton, "PlannedNode");
                StageNodeFixture announcedNode = CreateStageNode(AnnouncedNodeButton, "AnnouncedNode");
                ActualNodeTitle = actualNode.Title;
                ActualNodeStatus = actualNode.Status;
                PlannedNodeStatus = plannedNode.Status;
                AnnouncedNodeStatus = announcedNode.Status;

                DetailStageCode = CreateText(StageDetailPanel.transform, "DetailStageCode");
                DetailTitle = CreateText(StageDetailPanel.transform, "DetailTitle");
                DetailStatus = CreateText(StageDetailPanel.transform, "DetailStatus");
                DetailAvailability = CreateText(
                    StageDetailPanel.transform,
                    "DetailAvailability");
                (ObjectiveRow, ObjectiveText) = CreateTextRow(
                    StageDetailPanel.transform,
                    "ObjectiveRow");
                (CombatLessonRow, CombatLessonText) = CreateTextRow(
                    StageDetailPanel.transform,
                    "CombatLessonRow");
                (StoryRow, StoryText) = CreateTextRow(
                    StageDetailPanel.transform,
                    "StoryRow");
                (SegmentRow, SegmentText) = CreateTextRow(
                    StageDetailPanel.transform,
                    "SegmentRow");
                RecommendedPowerRow = CreateRow(StageDetailPanel.transform, "PowerRow");
                LoadoutRow = CreateRow(StageDetailPanel.transform, "LoadoutRow");
                DurationRow = CreateRow(StageDetailPanel.transform, "DurationRow");
                ThreatRow = CreateRow(StageDetailPanel.transform, "ThreatRow");
                SummonRow = CreateRow(StageDetailPanel.transform, "SummonRow");
                RewardRow = CreateRow(StageDetailPanel.transform, "RewardRow");
                DetailBackButton = CreateButton(StageDetailPanel.transform, "DetailBackButton");
                DetailReviewButton = CreateButton(StageDetailPanel.transform, "DetailReviewButton");

                ConfirmTitle = CreateText(ReviewConfirmPanel.transform, "ConfirmTitle");
                ConfirmSummary = CreateText(ReviewConfirmPanel.transform, "ConfirmSummary");
                ConfirmStatus = CreateText(ReviewConfirmPanel.transform, "ConfirmStatus");
                ConfirmBackButton = CreateButton(ReviewConfirmPanel.transform, "ConfirmBackButton");
                ConfirmAcceptButton = CreateButton(
                    ReviewConfirmPanel.transform,
                    "ConfirmAcceptButton");

                Type controllerType = Controller.GetType();
                Type chapterBindingType = controllerType.GetNestedType(
                    "ChapterButtonBinding",
                    BindingFlags.Public);
                Type stageBindingType = controllerType.GetNestedType(
                    "StageNodeBinding",
                    BindingFlags.Public);
                Assert.That(chapterBindingType, Is.Not.Null);
                Assert.That(stageBindingType, Is.Not.Null);
                object chapterBinding = Activator.CreateInstance(chapterBindingType);
                Invoke(
                    chapterBinding,
                    "Configure",
                    ChapterId,
                    ChapterButton,
                    chapterBindingGroup,
                    chapterEpisode,
                    chapterTitle);
                Array chapterBindings = Array.CreateInstance(chapterBindingType, 1);
                chapterBindings.SetValue(chapterBinding, 0);
                Array stageBindings = Array.CreateInstance(stageBindingType, 3);
                stageBindings.SetValue(
                    CreateStageBinding(stageBindingType, ActualStageId, ActualNodeButton, actualNode),
                    0);
                stageBindings.SetValue(
                    CreateStageBinding(stageBindingType, PlannedStageId, PlannedNodeButton, plannedNode),
                    1);
                stageBindings.SetValue(
                    CreateStageBinding(stageBindingType, AnnouncedStageId, AnnouncedNodeButton, announcedNode),
                    2);

                Invoke(Controller, "ConfigureCore", Profile, Catalog);
                Invoke(
                    Controller,
                    "ConfigurePanels",
                    ChapterHubPanel,
                    StageMapPanel,
                    StageDetailPanel,
                    ReviewConfirmPanel);
                Invoke(
                    Controller,
                    "ConfigureChapterView",
                    CreateText(ChapterHubPanel.transform, "HubEpisode"),
                    CreateText(ChapterHubPanel.transform, "HubTitle"),
                    CreateText(ChapterHubPanel.transform, "HubStatus"),
                    chapterBindings);
                Invoke(
                    Controller,
                    "ConfigureStageMapView",
                    CreateText(StageMapPanel.transform, "MapEpisode"),
                    CreateText(StageMapPanel.transform, "MapTitle"),
                    CreateText(StageMapPanel.transform, "MapStatus"),
                    MapBackButton,
                    stageBindings);
                Invoke(
                    Controller,
                    "ConfigureStageDetailView",
                    DetailStageCode,
                    DetailTitle,
                    DetailStatus,
                    ObjectiveRow,
                    ObjectiveText,
                    CombatLessonRow,
                    CombatLessonText,
                    StoryRow,
                    StoryText,
                    SegmentRow,
                    SegmentText,
                    DetailBackButton,
                    DetailReviewButton);
                Invoke(
                    Controller,
                    "ConfigureUnverifiedDetailRows",
                    RecommendedPowerRow,
                    LoadoutRow,
                    DurationRow,
                    ThreatRow,
                    SummonRow,
                    RewardRow);
                Invoke(
                    Controller,
                    "ConfigureAvailabilityText",
                    DetailAvailability);
                Invoke(
                    Controller,
                    "ConfigureConfirmationView",
                    ConfirmTitle,
                    ConfirmSummary,
                    ConfirmStatus,
                    ConfirmBackButton,
                    ConfirmAcceptButton);
            }

            public GameObject Root { get; }
            public Component Controller { get; }
            public Component Router { get; }
            public ScriptableObject Profile { get; }
            public ScriptableObject Catalog { get; }
            public string CanonicalCatalogEntryId { get; }
            public CanvasGroup ChapterHubPanel { get; }
            public CanvasGroup StageMapPanel { get; }
            public CanvasGroup StageDetailPanel { get; }
            public CanvasGroup ReviewConfirmPanel { get; }
            public Button ChapterButton { get; }
            public Button MapBackButton { get; }
            public Button ActualNodeButton { get; }
            public Button PlannedNodeButton { get; }
            public Button AnnouncedNodeButton { get; }
            public Component ActualNodeTitle { get; }
            public Component ActualNodeStatus { get; }
            public Component PlannedNodeStatus { get; }
            public Component AnnouncedNodeStatus { get; }
            public Component DetailStageCode { get; }
            public Component DetailTitle { get; }
            public Component DetailStatus { get; }
            public Component DetailAvailability { get; }
            public GameObject ObjectiveRow { get; }
            public Component ObjectiveText { get; }
            public GameObject CombatLessonRow { get; }
            public Component CombatLessonText { get; }
            public GameObject StoryRow { get; }
            public Component StoryText { get; }
            public GameObject SegmentRow { get; }
            public Component SegmentText { get; }
            public GameObject RecommendedPowerRow { get; }
            public GameObject LoadoutRow { get; }
            public GameObject DurationRow { get; }
            public GameObject ThreatRow { get; }
            public GameObject SummonRow { get; }
            public GameObject RewardRow { get; }
            public Button DetailBackButton { get; }
            public Button DetailReviewButton { get; }
            public Component ConfirmTitle { get; }
            public Component ConfirmSummary { get; }
            public Component ConfirmStatus { get; }
            public Button ConfirmBackButton { get; }
            public Button ConfirmAcceptButton { get; }

            public void Activate()
            {
                Root.SetActive(true);
            }

            public void OpenActualDetailWithButtons()
            {
                ChapterButton.onClick.Invoke();
                ActualNodeButton.onClick.Invoke();
            }

            public void AssertEveryUnverifiedRowHidden()
            {
                Assert.That(RecommendedPowerRow.activeSelf, Is.False);
                Assert.That(LoadoutRow.activeSelf, Is.False);
                Assert.That(DurationRow.activeSelf, Is.False);
                Assert.That(ThreatRow.activeSelf, Is.False);
                Assert.That(SummonRow.activeSelf, Is.False);
                Assert.That(RewardRow.activeSelf, Is.False);
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(Root);
                UnityEngine.Object.DestroyImmediate(Profile);
                if (ownsCatalog)
                {
                    UnityEngine.Object.DestroyImmediate(Catalog);
                }
            }
        }

        private readonly struct StageNodeFixture
        {
            public StageNodeFixture(
                CanvasGroup canvasGroup,
                Component code,
                Component title,
                Component status)
            {
                CanvasGroup = canvasGroup;
                Code = code;
                Title = title;
                Status = status;
            }

            public CanvasGroup CanvasGroup { get; }
            public Component Code { get; }
            public Component Title { get; }
            public Component Status { get; }
        }

        private static object CreateStage(
            Type stageType,
            string stageId,
            string stageCode,
            string titleFallback,
            Vector2 mapPosition,
            string contentStatus,
            string catalogEntryId)
        {
            object stage = Activator.CreateInstance(stageType);
            Invoke(
                stage,
                "Configure",
                stageId,
                ChapterId,
                stageCode,
                stageId + ".title",
                titleFallback,
                mapPosition,
                ResolveEnum(ContentStatusTypeName, contentStatus),
                catalogEntryId);
            return stage;
        }

        private static object CreateStageBinding(
            Type bindingType,
            string stageId,
            Button button,
            StageNodeFixture node)
        {
            object binding = Activator.CreateInstance(bindingType);
            Invoke(
                binding,
                "Configure",
                stageId,
                button,
                node.CanvasGroup,
                button.transform as RectTransform,
                node.Code,
                node.Title,
                node.Status);
            return binding;
        }

        private static StageNodeFixture CreateStageNode(Button button, string name)
        {
            CanvasGroup canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
            return new StageNodeFixture(
                canvasGroup,
                CreateText(button.transform, name + "Code"),
                CreateText(button.transform, name + "Title"),
                CreateText(button.transform, name + "Status"));
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

        private static GameObject CreateRow(Transform parent, string name)
        {
            var owner = new GameObject(name, typeof(RectTransform));
            owner.transform.SetParent(parent, false);
            return owner;
        }

        private static (GameObject row, Component text) CreateTextRow(
            Transform parent,
            string name)
        {
            GameObject row = CreateRow(parent, name);
            return (row, CreateText(row.transform, name + "Text"));
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

        private static object ResolveEnum(string typeName, int value)
        {
            return Enum.ToObject(RequireProductType(typeName), value);
        }

        private static object ResolveEnum(string typeName, string value)
        {
            return Enum.Parse(RequireProductType(typeName), value);
        }

        private static EventInfo RequireEvent(Type type, string eventName)
        {
            EventInfo eventInfo = type.GetEvent(
                eventName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(eventInfo, Is.Not.Null, $"Missing event {type.Name}.{eventName}.");
            return eventInfo;
        }

        private static MethodInfo RequireMethod(Type type, string methodName, int parameterCount)
        {
            MethodInfo[] matches = type.GetMethods(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method => method.Name == methodName
                    && method.GetParameters().Length == parameterCount)
                .ToArray();
            Assert.That(
                matches.Length,
                Is.EqualTo(1),
                $"Expected one {type.Name}.{methodName}/{parameterCount} overload.");
            return matches[0];
        }

        private static object Invoke(object target, string methodName, params object[] arguments)
        {
            Assert.That(target, Is.Not.Null);
            object[] safeArguments = arguments ?? Array.Empty<object>();
            return RequireMethod(target.GetType(), methodName, safeArguments.Length)
                .Invoke(target, safeArguments);
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
