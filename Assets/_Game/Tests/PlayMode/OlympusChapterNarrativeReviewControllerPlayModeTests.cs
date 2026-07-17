using System;
using System.Collections;
using System.Reflection;
using DimensionBrawl.LevelDesign;
using DimensionBrawl.Presentation.Narrative;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class OlympusChapterNarrativeReviewControllerPlayModeTests
    {
        private const string ControllerTypeName =
            "DimensionBrawl.UI.NarrativeReview.OlympusChapterNarrativeReviewController";

        [UnityTest]
        public IEnumerator RuntimeConfigurationBindsButtonsAndReenableResetsReviewState()
        {
            using var fixture = new ControllerFixture(withChoiceResponsePortrait: false);

            fixture.Root.SetActive(true);
            yield return null;

            AssertPhase(fixture.Controller, "ChapterEntry");
            fixture.ChapterEnterButton.onClick.Invoke();
            AssertPhase(fixture.Controller, "VisualNovel");
            Assert.That(ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession"), Is.Not.Null);

            fixture.NarrativeAutoButton.onClick.Invoke();
            Assert.That(ReadProperty<bool>(fixture.Controller, "AutoAdvanceEnabled"), Is.True);

            fixture.Root.SetActive(false);
            Assert.That(ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession"), Is.Null);

            fixture.Root.SetActive(true);
            yield return null;

            AssertPhase(fixture.Controller, "ChapterEntry");
            Assert.That(ReadProperty<bool>(fixture.Controller, "AutoAdvanceEnabled"), Is.False);
            Assert.That(ReadProperty<int>(fixture.Controller, "CompletionDispatchCount"), Is.Zero);
            Assert.That(fixture.ChapterEntryGroup.alpha, Is.EqualTo(1f));
            Assert.That(fixture.VisualNovelGroup.alpha, Is.Zero);

            fixture.ChapterEnterButton.onClick.Invoke();
            AssertPhase(fixture.Controller, "VisualNovel");
            Assert.That(ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession"), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator ChoiceResponseClearsStalePortraitAndSkipFinalizesExactlyOnce()
        {
            using var fixture = new ControllerFixture(withChoiceResponsePortrait: true);

            fixture.Root.SetActive(true);
            yield return null;
            fixture.ChapterEnterButton.onClick.Invoke();

            AssertPhase(fixture.Controller, "VisualNovel");
            Assert.That(fixture.LeftPortraitImage.sprite, Is.SameAs(fixture.MarkerSprite));
            Assert.That(fixture.LeftPortraitGroup.alpha, Is.EqualTo(1f));
            Assert.That(fixture.NarrativeChoiceGroup.alpha, Is.EqualTo(1f));

            fixture.FirstChoiceButton.onClick.Invoke();
            NarrativeSequenceSession session = ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession");
            Assert.That(session.SelectedChoiceIds, Is.EqualTo(new[]
            {
                "review.olympus.prologue.choice.verify"
            }));
            Assert.That(session.SeenLineIds, Does.Contain(
                "review.olympus.prologue.response.verify"));

            fixture.NarrativeNextButton.onClick.Invoke();

            Assert.That(session.CurrentLine.LineId, Is.EqualTo(
                "review.olympus.prologue.line.rejoin"));
            Assert.That(fixture.LeftPortraitImage.sprite, Is.Null);
            Assert.That(fixture.RightPortraitImage.sprite, Is.Null);
            Assert.That(fixture.LeftPortraitGroup.alpha, Is.Zero);
            Assert.That(fixture.RightPortraitGroup.alpha, Is.Zero);

            fixture.NarrativeSkipButton.onClick.Invoke();
            Assert.That(fixture.SkipConfirmGroup.alpha, Is.EqualTo(1f));
            fixture.SkipConfirmButton.onClick.Invoke();

            AssertPhase(fixture.Controller, "StageBriefing");
            Assert.That(session.IsCompleted, Is.True);
            Assert.That(
                session.CompletionReason,
                Is.EqualTo(NarrativeSequenceCompletionReason.Skipped));
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.EqualTo(1));

            fixture.SkipConfirmButton.onClick.Invoke();
            Invoke(fixture.Controller, "SkipCutscene");

            AssertPhase(fixture.Controller, "StageBriefing");
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator PortOnlyDirectorStopNaturallyFinalizesToBriefingOnce()
        {
            using var fixture = new ControllerFixture(
                withChoiceResponsePortrait: false,
                withCutscenePort: true);

            fixture.Root.SetActive(true);
            yield return null;

            Assert.That(ReadField<PlayableDirector>(
                fixture.Controller,
                "cutsceneDirector"), Is.Null);
            Assert.That(ReadProperty<bool>(
                fixture.Controller,
                "HasValidCutsceneBoundary"), Is.True);
            fixture.ChapterEnterButton.onClick.Invoke();
            fixture.NarrativeNextButton.onClick.Invoke();

            AssertPhase(fixture.Controller, "TutorialCutscene");
            Assert.That(ReadField<PlayableDirector>(
                fixture.Controller,
                "cutsceneDirector"), Is.SameAs(fixture.CutsceneDirector));
            Assert.That(fixture.CutsceneDirector.state, Is.EqualTo(PlayState.Playing));

            fixture.CutsceneDirector.Stop();
            yield return null;

            AssertPhase(fixture.Controller, "StageBriefing");
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.EqualTo(1));

            fixture.CutsceneDirector.Stop();
            Invoke(fixture.Controller, "SkipCutscene");

            AssertPhase(fixture.Controller, "StageBriefing");
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DisablingDuringCutsceneStopsDirectorAndFinalizesOnce()
        {
            using var fixture = new ControllerFixture(
                withChoiceResponsePortrait: false,
                withCutscenePort: true);

            fixture.Root.SetActive(true);
            yield return null;
            fixture.ChapterEnterButton.onClick.Invoke();
            fixture.NarrativeNextButton.onClick.Invoke();

            AssertPhase(fixture.Controller, "TutorialCutscene");
            Assert.That(fixture.CutsceneDirector.state, Is.EqualTo(PlayState.Playing));

            fixture.Root.SetActive(false);

            Assert.That(fixture.CutsceneDirector.state, Is.Not.EqualTo(PlayState.Playing));
            AssertPhase(fixture.Controller, "StageBriefing");
            Assert.That(ReadProperty<NarrativeSequenceSession>(
                fixture.Controller,
                "NarrativeSession"), Is.Null);
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.EqualTo(1));

            fixture.CutsceneDirector.Stop();
            Invoke(fixture.Controller, "SkipCutscene");

            AssertPhase(fixture.Controller, "StageBriefing");
            Assert.That(ReadProperty<int>(
                fixture.Controller,
                "CompletionDispatchCount"), Is.EqualTo(1));
        }

        private static void AssertPhase(Component controller, string expectedPhase)
        {
            object phase = ReadProperty(controller, "CurrentPhase");
            Assert.That(phase.ToString(), Is.EqualTo(expectedPhase));
        }

        private static Type RequireControllerType()
        {
            Type type = Type.GetType(ControllerTypeName + ", DimensionBrawl.Runtime")
                ?? Type.GetType(ControllerTypeName + ", Assembly-CSharp");
            Assert.That(type, Is.Not.Null, $"Missing product type {ControllerTypeName}.");
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

        private static void Invoke(Component target, string methodName, params object[] arguments)
        {
            RequireMethod(target.GetType(), methodName).Invoke(target, arguments);
        }

        private static object ReadProperty(Component target, string propertyName)
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

        private static T ReadProperty<T>(Component target, string propertyName)
        {
            object value = ReadProperty(target, propertyName);
            return value == null ? default : (T)value;
        }

        private static T ReadField<T>(Component target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(
                field,
                Is.Not.Null,
                $"Missing field {target.GetType().Name}.{fieldName}.");
            object value = field.GetValue(target);
            return value == null ? default : (T)value;
        }

        private sealed class ControllerFixture : IDisposable
        {
            public ControllerFixture(
                bool withChoiceResponsePortrait,
                bool withCutscenePort = false)
            {
                MarkerTexture = new Texture2D(2, 2)
                {
                    name = "NarrativeReviewPortraitMarkerTexture"
                };
                MarkerSprite = Sprite.Create(
                    MarkerTexture,
                    new Rect(0f, 0f, 2f, 2f),
                    new Vector2(0.5f, 0.5f));
                MarkerSprite.name = "NarrativeReviewPortraitMarker";
                Profile = withChoiceResponsePortrait
                    ? CreateChoiceResponseProfile(MarkerSprite)
                    : CreateSimpleProfile();

                Root = new GameObject("OlympusNarrativeReviewControllerTest");
                Root.SetActive(false);
                Controller = Root.AddComponent(RequireControllerType());
                if (withCutscenePort)
                {
                    GameObject cutsceneOwner = new GameObject("CutscenePayloadRoot");
                    cutsceneOwner.transform.SetParent(Root.transform, false);
                    CutsceneDirector = cutsceneOwner.AddComponent<PlayableDirector>();
                    CutsceneDirector.playOnAwake = false;
                    CutsceneAsset = ScriptableObject.CreateInstance<ReviewPlayableAsset>();
                    CutsceneAsset.name = "NarrativeReviewTestPlayable";
                    CutsceneDirector.playableAsset = CutsceneAsset;
                    CutscenePort = cutsceneOwner.AddComponent<StageCutscenePort>();
                    CutscenePort.Configure(
                        "review.olympus.prologue.intro",
                        StageCutscenePortKind.Intro,
                        "review.olympus.prologue.handoff",
                        "review.olympus.prologue.anchor",
                        "review.olympus.prologue.runtime",
                        cutsceneOwner.transform,
                        "Review-only port-bound director test.");
                    CutscenePort.ConfigurePresentationBinding(null, CutsceneDirector);
                }

                ChapterEntryGroup = CreateGroup(Root.transform, "ChapterEntryGroup");
                VisualNovelGroup = CreateGroup(Root.transform, "VisualNovelGroup");
                CutsceneGroup = CreateGroup(Root.transform, "CutsceneGroup");
                StageBriefingGroup = CreateGroup(Root.transform, "StageBriefingGroup");
                CompleteGroup = CreateGroup(Root.transform, "CompleteGroup");
                LeftPortraitGroup = CreateGroup(Root.transform, "LeftPortraitGroup");
                CenterPortraitGroup = CreateGroup(Root.transform, "CenterPortraitGroup");
                RightPortraitGroup = CreateGroup(Root.transform, "RightPortraitGroup");
                NarrativeChoiceGroup = CreateGroup(Root.transform, "NarrativeChoiceGroup");
                SkipConfirmGroup = CreateGroup(Root.transform, "SkipConfirmGroup");

                LeftPortraitImage = CreateImage(LeftPortraitGroup.transform, "LeftPortraitImage");
                CenterPortraitImage = CreateImage(
                    CenterPortraitGroup.transform,
                    "CenterPortraitImage");
                RightPortraitImage = CreateImage(
                    RightPortraitGroup.transform,
                    "RightPortraitImage");
                ChapterEnterButton = CreateButton(Root.transform, "ChapterEnterButton");
                NarrativeNextButton = CreateButton(Root.transform, "NarrativeNextButton");
                NarrativeAutoButton = CreateButton(Root.transform, "NarrativeAutoButton");
                NarrativeSkipButton = CreateButton(Root.transform, "NarrativeSkipButton");
                FirstChoiceButton = CreateButton(Root.transform, "FirstChoiceButton");
                SecondChoiceButton = CreateButton(Root.transform, "SecondChoiceButton");
                SkipConfirmButton = CreateButton(Root.transform, "SkipConfirmButton");
                SkipCancelButton = CreateButton(Root.transform, "SkipCancelButton");

                ConfigureController();
            }

            public GameObject Root { get; }
            public Component Controller { get; }
            public NarrativeSequenceProfile Profile { get; }
            public Texture2D MarkerTexture { get; }
            public Sprite MarkerSprite { get; }
            public PlayableAsset CutsceneAsset { get; }
            public PlayableDirector CutsceneDirector { get; }
            public StageCutscenePort CutscenePort { get; }
            public CanvasGroup ChapterEntryGroup { get; }
            public CanvasGroup VisualNovelGroup { get; }
            public CanvasGroup CutsceneGroup { get; }
            public CanvasGroup StageBriefingGroup { get; }
            public CanvasGroup CompleteGroup { get; }
            public CanvasGroup LeftPortraitGroup { get; }
            public CanvasGroup CenterPortraitGroup { get; }
            public CanvasGroup RightPortraitGroup { get; }
            public CanvasGroup NarrativeChoiceGroup { get; }
            public CanvasGroup SkipConfirmGroup { get; }
            public Image LeftPortraitImage { get; }
            public Image CenterPortraitImage { get; }
            public Image RightPortraitImage { get; }
            public Button ChapterEnterButton { get; }
            public Button NarrativeNextButton { get; }
            public Button NarrativeAutoButton { get; }
            public Button NarrativeSkipButton { get; }
            public Button FirstChoiceButton { get; }
            public Button SecondChoiceButton { get; }
            public Button SkipConfirmButton { get; }
            public Button SkipCancelButton { get; }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(Root);
                UnityEngine.Object.DestroyImmediate(Profile);
                UnityEngine.Object.DestroyImmediate(CutsceneAsset);
                UnityEngine.Object.DestroyImmediate(MarkerSprite);
                UnityEngine.Object.DestroyImmediate(MarkerTexture);
            }

            private void ConfigureController()
            {
                Invoke(Controller, "ConfigureCore", Profile, null, null, CutscenePort, null);
                Invoke(
                    Controller,
                    "ConfigureFlowGroups",
                    ChapterEntryGroup,
                    VisualNovelGroup,
                    CutsceneGroup,
                    StageBriefingGroup,
                    CompleteGroup);
                Invoke(
                    Controller,
                    "ConfigureChapterView",
                    null,
                    null,
                    null,
                    null,
                    null,
                    ChapterEnterButton);
                Invoke(
                    Controller,
                    "ConfigureNarrativeView",
                    null,
                    null,
                    null,
                    null,
                    LeftPortraitGroup,
                    CenterPortraitGroup,
                    RightPortraitGroup,
                    LeftPortraitImage,
                    CenterPortraitImage,
                    RightPortraitImage,
                    NarrativeNextButton,
                    NarrativeAutoButton,
                    null,
                    NarrativeSkipButton,
                    null,
                    NarrativeChoiceGroup,
                    FirstChoiceButton,
                    null,
                    SecondChoiceButton,
                    null);
                Invoke(
                    Controller,
                    "ConfigureUtilityPanels",
                    null,
                    null,
                    null,
                    SkipConfirmGroup,
                    SkipConfirmButton,
                    SkipCancelButton);
            }
        }

        private static NarrativeSequenceProfile CreateSimpleProfile()
        {
            return CreateProfile(new NarrativeSequenceProfile.LineEntry(
                "review.olympus.prologue.line.only",
                "narrative.review.olympus.prologue.line.only",
                "게이트 신호를 확인했다.",
                "operator",
                NarrativePortraitSlot.None,
                "neutral"));
        }

        private static NarrativeSequenceProfile CreateChoiceResponseProfile(Sprite portrait)
        {
            var choice = new NarrativeSequenceProfile.ChoiceEntry(
                "review.olympus.prologue.choice.verify",
                "narrative.review.olympus.prologue.choice.verify",
                "상황을 한 번 더 확인한다",
                "review.olympus.prologue.response.verify",
                "narrative.review.olympus.prologue.response.verify",
                "스캔을 한 번 더 돌릴게요. 결과는 같아요.");
            return CreateProfile(
                new NarrativeSequenceProfile.LineEntry(
                    "review.olympus.prologue.line.choice",
                    "narrative.review.olympus.prologue.line.choice",
                    "어떻게 진행할까?",
                    "operator",
                    NarrativePortraitSlot.Left,
                    "alert",
                    portraitSprite: portrait,
                    choices: new[] { choice }),
                new NarrativeSequenceProfile.LineEntry(
                    "review.olympus.prologue.line.rejoin",
                    "narrative.review.olympus.prologue.line.rejoin",
                    "진입 절차를 개시한다.",
                    "field_agent",
                    NarrativePortraitSlot.Right,
                    "neutral"));
        }

        private static NarrativeSequenceProfile CreateProfile(
            params NarrativeSequenceProfile.LineEntry[] lines)
        {
            NarrativeSequenceProfile profile =
                ScriptableObject.CreateInstance<NarrativeSequenceProfile>();
            profile.Configure("review.olympus.prologue", 0.04f, lines);
            Assert.That(profile.TryValidate(out string validationError), Is.True, validationError);
            return profile;
        }

        private static CanvasGroup CreateGroup(Transform parent, string name)
        {
            var owner = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            owner.transform.SetParent(parent, false);
            return owner.GetComponent<CanvasGroup>();
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

        private sealed class ReviewPlayableAsset : PlayableAsset
        {
            public override double duration => 5d;

            public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
            {
                return Playable.Create(graph);
            }
        }
    }
}
