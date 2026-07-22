using DimensionBrawl.Presentation.Narrative;
using DimensionBrawl.UI.NarrativeReview;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace DimensionBrawl.Tests
{
    public sealed class NarrativeVisualNovelPresenterPlayModeTests
    {
        [Test]
        public void OrderedPortraitCommandsPersistMoveRefocusAndExposeInteractionState()
        {
            var root = new GameObject("NarrativeVisualNovelPresenterTest");
            var texture = new Texture2D(2, 2) { name = "VN02PresenterTexture" };
            Sprite fieldNeutral = CreateSprite(texture, "FieldNeutral");
            Sprite fieldResolve = CreateSprite(texture, "FieldResolve");
            Sprite operatorAlert = CreateSprite(texture, "OperatorAlert");
            NarrativeSpeakerPresentationCatalog catalog =
                ScriptableObject.CreateInstance<NarrativeSpeakerPresentationCatalog>();

            try
            {
                catalog.Configure(
                    "test.vn02.presentations",
                    new[]
                    {
                        new NarrativeSpeakerPresentationCatalog.SpeakerEntry(
                            "field_agent",
                            "Field Agent",
                            NarrativePortraitSlot.Center,
                            new[]
                            {
                                new NarrativeSpeakerPresentationCatalog.ExpressionEntry(
                                    "neutral",
                                    fieldNeutral),
                                new NarrativeSpeakerPresentationCatalog.ExpressionEntry(
                                    "resolve",
                                    fieldResolve)
                            }),
                        new NarrativeSpeakerPresentationCatalog.SpeakerEntry(
                            "operator",
                            "Operator",
                            NarrativePortraitSlot.Right,
                            new[]
                            {
                                new NarrativeSpeakerPresentationCatalog.ExpressionEntry(
                                    "alert",
                                    operatorAlert)
                            })
                    });
                Assert.That(catalog.TryValidate(out string error), Is.True, error);

                CanvasGroup leftGroup = CreateGroup(root.transform, "Left");
                CanvasGroup centerGroup = CreateGroup(root.transform, "Center");
                CanvasGroup rightGroup = CreateGroup(root.transform, "Right");
                Image leftImage = CreateImage(leftGroup.transform, "LeftImage");
                Image centerImage = CreateImage(centerGroup.transform, "CenterImage");
                Image rightImage = CreateImage(rightGroup.transform, "RightImage");
                NarrativeVisualNovelPresenter presenter =
                    root.AddComponent<NarrativeVisualNovelPresenter>();
                presenter.Configure(
                    catalog,
                    leftGroup,
                    centerGroup,
                    rightGroup,
                    leftImage,
                    centerImage,
                    rightImage,
                    null,
                    null,
                    null);
                presenter.ResetPresentation();

                presenter.PresentLine(Line(
                    "line.field.center",
                    "field_agent",
                    Present("field_agent", NarrativePortraitSlot.None, "neutral")));
                NarrativeVisualNovelPresentationSnapshot first = presenter.Snapshot;
                Assert.That(first.Center.SpeakerId, Is.EqualTo("field_agent"));
                Assert.That(first.Center.ExpressionId, Is.EqualTo("neutral"));
                Assert.That(first.Center.PortraitSprite, Is.SameAs(fieldNeutral));
                Assert.That(first.Center.IsFocused, Is.True);
                Assert.That(centerGroup.alpha, Is.EqualTo(1f));

                presenter.PresentLine(Line(
                    "line.field.fallback",
                    "field_agent",
                    Present("field_agent", NarrativePortraitSlot.None, "missing-expression")));
                NarrativeVisualNovelPresentationSnapshot fallback = presenter.Snapshot;
                Assert.That(fallback.Center.ExpressionId, Is.EqualTo("neutral"));
                Assert.That(fallback.Center.PortraitSprite, Is.SameAs(fieldNeutral));

                presenter.PresentLine(Line(
                    "line.operator.right",
                    "operator",
                    Present("operator", NarrativePortraitSlot.Right, "alert")));
                NarrativeVisualNovelPresentationSnapshot second = presenter.Snapshot;
                Assert.That(second.Center.SpeakerId, Is.EqualTo("field_agent"));
                Assert.That(second.Center.IsFocused, Is.False);
                Assert.That(second.Right.SpeakerId, Is.EqualTo("operator"));
                Assert.That(second.Right.IsFocused, Is.True);
                Assert.That(centerGroup.alpha, Is.EqualTo(0.48f).Within(0.001f));
                Assert.That(rightGroup.alpha, Is.EqualTo(1f));

                presenter.PresentLine(Line(
                    "line.hide.field",
                    "operator",
                    new NarrativeSequenceProfile.PortraitCommandEntry(
                        NarrativePortraitCommandType.HideSpeaker,
                        "field_agent")));
                NarrativeVisualNovelPresentationSnapshot hidden = presenter.Snapshot;
                Assert.That(hidden.Center.IsOccupied, Is.False);
                Assert.That(hidden.Right.SpeakerId, Is.EqualTo("operator"));
                Assert.That(hidden.Right.IsFocused, Is.True);
                Assert.That(rightGroup.alpha, Is.EqualTo(1f));

                presenter.PresentLine(Line(
                    "line.field.left",
                    "field_agent",
                    Present("field_agent", NarrativePortraitSlot.Left, "resolve")));
                NarrativeVisualNovelPresentationSnapshot moved = presenter.Snapshot;
                Assert.That(moved.Left.SpeakerId, Is.EqualTo("field_agent"));
                Assert.That(moved.Left.ExpressionId, Is.EqualTo("resolve"));
                Assert.That(moved.Left.PortraitSprite, Is.SameAs(fieldResolve));
                Assert.That(moved.Left.IsFocused, Is.True);
                Assert.That(moved.Center.IsOccupied, Is.False);
                Assert.That(moved.Right.SpeakerId, Is.EqualTo("operator"));
                Assert.That(moved.Right.IsFocused, Is.False);

                presenter.SetLineFullyRevealed(true);
                presenter.SetAutoAdvanceEnabled(true);
                presenter.SetChoicesVisible(true);
                presenter.SetLogVisible(true);
                presenter.SetSkipConfirmationVisible(true);
                NarrativeVisualNovelPresentationSnapshot inspectable = presenter.Snapshot;
                Assert.That(inspectable.LineFullyRevealed, Is.True);
                Assert.That(inspectable.AutoAdvanceEnabled, Is.True);
                Assert.That(inspectable.ChoicesVisible, Is.True);
                Assert.That(inspectable.LogVisible, Is.True);
                Assert.That(inspectable.SkipConfirmationVisible, Is.True);
                Assert.That(
                    presenter.TryResolveDisplayName("operator", out string displayName),
                    Is.True);
                Assert.That(displayName, Is.EqualTo("Operator"));

                presenter.PresentLine(Line(
                    "line.system",
                    "system",
                    new NarrativeSequenceProfile.PortraitCommandEntry(
                        NarrativePortraitCommandType.ClearFocus)));
                NarrativeVisualNovelPresentationSnapshot unfocused = presenter.Snapshot;
                Assert.That(unfocused.Left.IsOccupied, Is.True);
                Assert.That(unfocused.Right.IsOccupied, Is.True);
                Assert.That(unfocused.Left.IsFocused, Is.False);
                Assert.That(unfocused.Right.IsFocused, Is.False);
                Assert.That(leftGroup.alpha, Is.EqualTo(0.48f).Within(0.001f));
                Assert.That(rightGroup.alpha, Is.EqualTo(0.48f).Within(0.001f));

                presenter.PresentLine(Line(
                    "line.clear",
                    "system",
                    new NarrativeSequenceProfile.PortraitCommandEntry(
                        NarrativePortraitCommandType.ClearStage)));
                Assert.That(presenter.Snapshot.Left.IsOccupied, Is.False);
                Assert.That(presenter.Snapshot.Center.IsOccupied, Is.False);
                Assert.That(presenter.Snapshot.Right.IsOccupied, Is.False);
                Assert.That(leftGroup.alpha, Is.Zero);
                Assert.That(centerGroup.alpha, Is.Zero);
                Assert.That(rightGroup.alpha, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(fieldNeutral);
                Object.DestroyImmediate(fieldResolve);
                Object.DestroyImmediate(operatorAlert);
                Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void ValidationRejectsUndefinedPresentationEnumsAndRuntimeFailsClosed()
        {
            var root = new GameObject("NarrativeVisualNovelInvalidEnumTest");
            var texture = new Texture2D(2, 2) { name = "InvalidEnumTexture" };
            Sprite portrait = CreateSprite(texture, "InvalidEnumPortrait");
            NarrativeSpeakerPresentationCatalog catalog =
                ScriptableObject.CreateInstance<NarrativeSpeakerPresentationCatalog>();
            NarrativeSequenceProfile profile =
                ScriptableObject.CreateInstance<NarrativeSequenceProfile>();

            try
            {
                catalog.Configure(
                    "test.vn02.invalid-slot",
                    new[]
                    {
                        new NarrativeSpeakerPresentationCatalog.SpeakerEntry(
                            "field_agent",
                            "Field Agent",
                            (NarrativePortraitSlot)99,
                            new[]
                            {
                                new NarrativeSpeakerPresentationCatalog.ExpressionEntry(
                                    "neutral",
                                    portrait)
                            })
                    });
                Assert.That(catalog.TryValidate(out string catalogError), Is.False);
                StringAssert.Contains("undefined default portrait slot '99'", catalogError);

                NarrativeSequenceProfile.LineEntry invalidLine = Line(
                    "line.invalid-enums",
                    "field_agent",
                    new NarrativeSequenceProfile.PortraitCommandEntry(
                        (NarrativePortraitCommandType)99,
                        "field_agent",
                        (NarrativePortraitSlot)99,
                        "neutral"));
                profile.Configure("test.vn02.invalid-enums", 0.04f, new[] { invalidLine });
                Assert.That(profile.TryValidate(out string profileError), Is.False);
                StringAssert.Contains("undefined command type '99'", profileError);
                StringAssert.Contains("undefined portrait slot '99'", profileError);

                NarrativeVisualNovelPresenter presenter =
                    root.AddComponent<NarrativeVisualNovelPresenter>();
                presenter.Configure(
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null);
                Assert.DoesNotThrow(() => presenter.PresentLine(invalidLine));
                Assert.That(presenter.Snapshot.Left.IsOccupied, Is.False);
                Assert.That(presenter.Snapshot.Center.IsOccupied, Is.False);
                Assert.That(presenter.Snapshot.Right.IsOccupied, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(profile);
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(portrait);
                Object.DestroyImmediate(texture);
            }
        }

        private static NarrativeSequenceProfile.LineEntry Line(
            string lineId,
            string speakerId,
            NarrativeSequenceProfile.PortraitCommandEntry command)
        {
            return new NarrativeSequenceProfile.LineEntry(
                lineId,
                lineId + ".text",
                "staging",
                speakerId,
                NarrativePortraitSlot.None,
                string.Empty,
                portraitCommands: new[] { command });
        }

        private static NarrativeSequenceProfile.PortraitCommandEntry Present(
            string speakerId,
            NarrativePortraitSlot slot,
            string expressionId)
        {
            return new NarrativeSequenceProfile.PortraitCommandEntry(
                NarrativePortraitCommandType.Present,
                speakerId,
                slot,
                expressionId);
        }

        private static Sprite CreateSprite(Texture2D texture, string name)
        {
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            sprite.name = name;
            return sprite;
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
    }
}
