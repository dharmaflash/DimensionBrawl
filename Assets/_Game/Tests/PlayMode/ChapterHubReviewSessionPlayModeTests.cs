using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace DimensionBrawl.Tests
{
    public sealed class ChapterHubReviewSessionPlayModeTests
    {
        private const string ProductNamespace = "DimensionBrawl.UI.ChapterHubReview.";
        private const string ChapterAId = "review.chapter.olympus";
        private const string ChapterBId = "review.chapter.frontier";
        private const string CanonicalStageId = "review.stage.memory-corridor";
        private const string InProductionStageId = "review.stage.courtyard";
        private const string AnnouncedStageId = "review.stage.frontier-gate";
        private const string CanonicalCatalogEntryId = "story_v1_training_route";

        [Test]
        public void ValidationRejectsEmptyDuplicateAndUnknownStableIds()
        {
            AssertInvalid(
                "has no chapter id",
                new[] { CreateChapter(string.Empty, "EP.00") },
                new[]
                {
                    CreateStage(
                        CanonicalStageId,
                        string.Empty,
                        "00-01",
                        "CanonicalPlayable",
                        CanonicalCatalogEntryId)
                });
            AssertInvalid(
                "has no stage id",
                new[] { CreateChapter(ChapterAId, "EP.01") },
                new[]
                {
                    CreateStage(
                        string.Empty,
                        ChapterAId,
                        "01-01",
                        "CanonicalPlayable",
                        CanonicalCatalogEntryId)
                });
            AssertInvalid(
                "duplicate chapter id",
                new[]
                {
                    CreateChapter(ChapterAId, "EP.01"),
                    CreateChapter(ChapterAId, "EP.02")
                },
                new[]
                {
                    CreateStage(
                        CanonicalStageId,
                        ChapterAId,
                        "01-01",
                        "CanonicalPlayable",
                        CanonicalCatalogEntryId)
                });
            AssertInvalid(
                "duplicate stage id",
                new[] { CreateChapter(ChapterAId, "EP.01") },
                new[]
                {
                    CreateStage(
                        CanonicalStageId,
                        ChapterAId,
                        "01-01",
                        "CanonicalPlayable",
                        CanonicalCatalogEntryId),
                    CreateStage(
                        CanonicalStageId,
                        ChapterAId,
                        "01-02",
                        "InProduction")
                });
            AssertInvalid(
                "references unknown chapter",
                new[] { CreateChapter(ChapterAId, "EP.01") },
                new[]
                {
                    CreateStage(
                        CanonicalStageId,
                        "review.chapter.unknown",
                        "01-01",
                        "CanonicalPlayable",
                        CanonicalCatalogEntryId)
                });
        }

        [Test]
        public void ValidationRejectsStatusCatalogMismatchAndInvalidMapCoordinates()
        {
            object[] chapters = { CreateChapter(ChapterAId, "EP.01") };
            AssertInvalid(
                "has no canonical catalog entry id",
                chapters,
                new[]
                {
                    CreateStage(
                        CanonicalStageId,
                        ChapterAId,
                        "01-01",
                        "CanonicalPlayable")
                });
            AssertInvalid(
                "must not declare a canonical catalog entry id",
                chapters,
                new[]
                {
                    CreateStage(
                        InProductionStageId,
                        ChapterAId,
                        "01-02",
                        "InProduction",
                        CanonicalCatalogEntryId)
                });
            AssertInvalid(
                "map position must be finite and within 0..1",
                chapters,
                new[]
                {
                    CreateStage(
                        InProductionStageId,
                        ChapterAId,
                        "01-02",
                        "InProduction",
                        mapPosition: new Vector2(-0.01f, 1.01f))
                });
            AssertInvalid(
                "map position must be finite and within 0..1",
                chapters,
                new[]
                {
                    CreateStage(
                        InProductionStageId,
                        ChapterAId,
                        "01-02",
                        "InProduction",
                        mapPosition: new Vector2(float.NaN, 0.5f))
                });
            AssertInvalid(
                "unsupported content status",
                chapters,
                new[]
                {
                    CreateStage(
                        InProductionStageId,
                        ChapterAId,
                        "01-02",
                        "None")
                });
        }

        [Test]
        public void ValidationRequiresFallbackTitlesUntilLocalizationIsConnected()
        {
            AssertInvalid(
                "has no fallback title for local review",
                new[] { CreateChapter(ChapterAId, "EP.01", string.Empty) },
                new[]
                {
                    CreateStage(
                        CanonicalStageId,
                        ChapterAId,
                        "01-01",
                        "CanonicalPlayable",
                        CanonicalCatalogEntryId)
                });
            AssertInvalid(
                "has no fallback title for local review",
                new[] { CreateChapter(ChapterAId, "EP.01") },
                new[]
                {
                    CreateStage(
                        InProductionStageId,
                        ChapterAId,
                        "01-02",
                        "InProduction",
                        titleFallback: string.Empty)
                });
        }

        [Test]
        public void SessionDeepCopiesProfileAndEveryExposedDefinition()
        {
            object authoredChapter = CreateChapter(ChapterAId, "EP.01");
            object authoredStage = CreateStage(
                CanonicalStageId,
                ChapterAId,
                "01-01",
                "CanonicalPlayable",
                CanonicalCatalogEntryId);
            ScriptableObject profile = CreateProfile(
                new[] { authoredChapter },
                new[] { authoredStage });
            try
            {
                object session = CreateSession(profile);

                ConfigureChapter(
                    authoredChapter,
                    "mutated.external.chapter",
                    "MUTATED");
                ConfigureStage(
                    authoredStage,
                    "mutated.external.stage",
                    "mutated.external.chapter",
                    "MUTATED",
                    "Announced");
                ConfigureProfile(
                    profile,
                    new[] { CreateChapter(ChapterBId, "EP.99") },
                    new[]
                    {
                        CreateStage(
                            AnnouncedStageId,
                            ChapterBId,
                            "99-01",
                            "Announced")
                    });

                Assert.That(InvokeBool(session, "TrySelectChapter", ChapterAId), Is.True);
                Assert.That(InvokeBool(session, "TrySelectStage", CanonicalStageId), Is.True);

                object exposedStage = ReadProperty(session, "SelectedStage");
                ConfigureStage(
                    exposedStage,
                    "mutated.session.view",
                    ChapterAId,
                    "MUTATED",
                    "Announced");
                Array exposedList = (Array)RequireMethod(
                        SessionType,
                        "GetStagesForChapter")
                    .Invoke(session, new object[] { ChapterAId });
                ConfigureStage(
                    exposedList.GetValue(0),
                    "mutated.session.list",
                    ChapterAId,
                    "MUTATED",
                    "Announced");

                object selectedChapter = ReadProperty(session, "SelectedChapter");
                object selectedStage = ReadProperty(session, "SelectedStage");
                Assert.That(ReadString(selectedChapter, "ChapterId"), Is.EqualTo(ChapterAId));
                Assert.That(ReadString(selectedChapter, "EpisodeCode"), Is.EqualTo("EP.01"));
                Assert.That(ReadString(selectedStage, "StageId"), Is.EqualTo(CanonicalStageId));
                Assert.That(ReadString(selectedStage, "StageCode"), Is.EqualTo("01-01"));
                Assert.That(
                    ReadString(selectedStage, "CanonicalCatalogEntryId"),
                    Is.EqualTo(CanonicalCatalogEntryId));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void StableIdSelectionIsIndependentOfDefinitionOrder()
        {
            ScriptableObject profileForward = CreateValidProfile(false);
            ScriptableObject profileReversed = CreateValidProfile(true);
            try
            {
                object forward = CreateSession(profileForward);
                object reversed = CreateSession(profileReversed);

                AssertCanonicalSelection(forward);
                AssertCanonicalSelection(reversed);

                Assert.That(
                    ReadString(forward, "SelectedChapterId"),
                    Is.EqualTo(ReadString(reversed, "SelectedChapterId")));
                Assert.That(
                    ReadString(forward, "SelectedStageId"),
                    Is.EqualTo(ReadString(reversed, "SelectedStageId")));
                Assert.That(
                    (Vector2)ReadProperty(ReadProperty(forward, "SelectedStage"), "NormalizedMapPosition"),
                    Is.EqualTo(
                        (Vector2)ReadProperty(
                            ReadProperty(reversed, "SelectedStage"),
                            "NormalizedMapPosition")));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profileForward);
                UnityEngine.Object.DestroyImmediate(profileReversed);
            }
        }

        [Test]
        public void UnknownAndCrossChapterSelectionsFailClosed()
        {
            ScriptableObject profile = CreateValidProfile(false);
            try
            {
                object session = CreateSession(profile);

                Assert.That(
                    InvokeBool(session, "TrySelectChapter", "review.chapter.unknown"),
                    Is.False);
                AssertPhase(session, "Overview");
                Assert.That(ReadString(session, "SelectedChapterId"), Is.Empty);
                Assert.That(ReadString(session, "SelectedStageId"), Is.Empty);

                Assert.That(InvokeBool(session, "TrySelectChapter", ChapterAId), Is.True);
                Assert.That(
                    InvokeBool(session, "TrySelectStage", "review.stage.unknown"),
                    Is.False);
                Assert.That(InvokeBool(session, "TrySelectStage", AnnouncedStageId), Is.False);
                AssertPhase(session, "StageMap");
                Assert.That(ReadString(session, "SelectedChapterId"), Is.EqualTo(ChapterAId));
                Assert.That(ReadString(session, "SelectedStageId"), Is.Empty);

                Assert.That(InvokeBool(session, "TrySelectStage", CanonicalStageId), Is.True);
                Assert.That(InvokeBool(session, "TrySelectChapter", ChapterBId), Is.False);
                Assert.That(TryConfirm(session, out string prematureCatalogEntryId), Is.False);
                Assert.That(prematureCatalogEntryId, Is.Empty);
                AssertPhase(session, "StageDetail");
                Assert.That(ReadString(session, "SelectedStageId"), Is.EqualTo(CanonicalStageId));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void BackTraversesConfirmDetailMapAndOverviewWithoutInventingState()
        {
            ScriptableObject profile = CreateValidProfile(false);
            try
            {
                object session = CreateSession(profile);
                Assert.That(InvokeBool(session, "TrySelectChapter", ChapterAId), Is.True);
                Assert.That(InvokeBool(session, "TrySelectStage", CanonicalStageId), Is.True);
                Assert.That(InvokeBool(session, "TryOpenReviewConfirm"), Is.True);

                Assert.That(InvokeBool(session, "TryBack"), Is.True);
                AssertPhase(session, "StageDetail");
                Assert.That(ReadString(session, "SelectedChapterId"), Is.EqualTo(ChapterAId));
                Assert.That(ReadString(session, "SelectedStageId"), Is.EqualTo(CanonicalStageId));

                Assert.That(InvokeBool(session, "TryBack"), Is.True);
                AssertPhase(session, "StageMap");
                Assert.That(ReadString(session, "SelectedChapterId"), Is.EqualTo(ChapterAId));
                Assert.That(ReadString(session, "SelectedStageId"), Is.Empty);

                Assert.That(InvokeBool(session, "TrySelectStage", InProductionStageId), Is.True);
                Assert.That(InvokeBool(session, "TryBack"), Is.True);
                AssertPhase(session, "StageMap");
                Assert.That(ReadString(session, "SelectedStageId"), Is.Empty);

                Assert.That(InvokeBool(session, "TryBack"), Is.True);
                AssertPhase(session, "Overview");
                Assert.That(ReadString(session, "SelectedChapterId"), Is.Empty);
                Assert.That(ReadString(session, "SelectedStageId"), Is.Empty);
                Assert.That(InvokeBool(session, "TryBack"), Is.False);
                AssertPhase(session, "Overview");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [TestCase(InProductionStageId)]
        [TestCase(AnnouncedStageId)]
        public void PlaceholderStageCannotEnterOrDispatchReviewConfirm(string placeholderStageId)
        {
            ScriptableObject profile = CreateValidProfile(false);
            try
            {
                object session = CreateSession(profile);
                string chapterId = string.Equals(
                    placeholderStageId,
                    AnnouncedStageId,
                    StringComparison.Ordinal)
                    ? ChapterBId
                    : ChapterAId;
                Assert.That(InvokeBool(session, "TrySelectChapter", chapterId), Is.True);
                Assert.That(InvokeBool(session, "TrySelectStage", placeholderStageId), Is.True);
                AssertPhase(session, "StageDetail");

                Assert.That(InvokeBool(session, "TryOpenReviewConfirm"), Is.False);
                Assert.That(TryConfirm(session, out string canonicalCatalogEntryId), Is.False);
                Assert.That(canonicalCatalogEntryId, Is.Empty);
                AssertPhase(session, "StageDetail");
                Assert.That(ReadString(session, "SelectedChapterId"), Is.EqualTo(chapterId));
                Assert.That(ReadString(session, "SelectedStageId"), Is.EqualTo(placeholderStageId));
                Assert.That(ReadBool(session, "IsConfirmationAccepted"), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void CanonicalConfirmReturnsCatalogEntryExactlyOnceWhileBackRemainsLocal()
        {
            ScriptableObject profile = CreateValidProfile(false);
            try
            {
                object session = CreateSession(profile);
                AssertCanonicalSelection(session);

                Assert.That(InvokeBool(session, "TryOpenReviewConfirm"), Is.True);
                AssertPhase(session, "ReviewConfirm");
                Assert.That(TryConfirm(session, out string acceptedCatalogEntryId), Is.True);
                Assert.That(acceptedCatalogEntryId, Is.EqualTo(CanonicalCatalogEntryId));
                Assert.That(ReadBool(session, "IsConfirmationAccepted"), Is.True);

                Assert.That(TryConfirm(session, out string duplicateCatalogEntryId), Is.False);
                Assert.That(duplicateCatalogEntryId, Is.Empty);
                Assert.That(InvokeBool(session, "TryBack"), Is.True);
                AssertPhase(session, "StageDetail");
                Assert.That(InvokeBool(session, "TrySelectChapter", ChapterBId), Is.False);
                Assert.That(InvokeBool(session, "TrySelectStage", InProductionStageId), Is.False);
                Assert.That(InvokeBool(session, "TryOpenReviewConfirm"), Is.True);
                AssertPhase(session, "ReviewConfirm");
                Assert.That(TryConfirm(session, out string reopenedCatalogEntryId), Is.False);
                Assert.That(reopenedCatalogEntryId, Is.Empty);
                Assert.That(InvokeBool(session, "TryBack"), Is.True);
                Assert.That(InvokeBool(session, "TryBack"), Is.True);
                AssertPhase(session, "StageMap");
                Assert.That(InvokeBool(session, "TrySelectStage", CanonicalStageId), Is.True);
                Assert.That(InvokeBool(session, "TryOpenReviewConfirm"), Is.True);
                Assert.That(TryConfirm(session, out string laterCatalogEntryId), Is.False);
                Assert.That(laterCatalogEntryId, Is.Empty);
                AssertPhase(session, "ReviewConfirm");
                Assert.That(ReadString(session, "SelectedStageId"), Is.EqualTo(CanonicalStageId));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void SessionIsPlainStateWithNoUnityEnableLifecycle()
        {
            const BindingFlags LifecycleFlags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            Assert.That(typeof(UnityEngine.Object).IsAssignableFrom(SessionType), Is.False);
            Assert.That(typeof(MonoBehaviour).IsAssignableFrom(SessionType), Is.False);
            Assert.That(SessionType.GetMethod("OnEnable", LifecycleFlags), Is.Null);
            Assert.That(SessionType.GetMethod("OnDisable", LifecycleFlags), Is.Null);
        }

        private static Type ProfileType => RequireProductType(ProductNamespace + "ChapterHubReviewProfile");
        private static Type ChapterDefinitionType =>
            RequireNestedType(ProfileType, "ChapterDefinition");
        private static Type StageDefinitionType => RequireNestedType(ProfileType, "StageDefinition");
        private static Type ContentStatusType =>
            RequireProductType(ProductNamespace + "ChapterHubReviewContentStatus");
        private static Type SessionType => RequireProductType(ProductNamespace + "ChapterHubReviewSession");

        private static void AssertCanonicalSelection(object session)
        {
            Assert.That(InvokeBool(session, "TrySelectChapter", ChapterAId), Is.True);
            Assert.That(InvokeBool(session, "TrySelectStage", CanonicalStageId), Is.True);
            AssertPhase(session, "StageDetail");
            Assert.That(ReadString(session, "SelectedChapterId"), Is.EqualTo(ChapterAId));
            Assert.That(ReadString(session, "SelectedStageId"), Is.EqualTo(CanonicalStageId));
            Assert.That(
                ReadString(ReadProperty(session, "SelectedStage"), "CanonicalCatalogEntryId"),
                Is.EqualTo(CanonicalCatalogEntryId));
        }

        private static void AssertInvalid(
            string expectedError,
            object[] chapters,
            object[] stages)
        {
            ScriptableObject profile = CreateUnvalidatedProfile(chapters, stages);
            try
            {
                Assert.That(TryValidate(profile, out string validationError), Is.False);
                Assert.That(validationError, Does.Contain(expectedError));
                TargetInvocationException exception = Assert.Throws<TargetInvocationException>(
                    () => Activator.CreateInstance(SessionType, new object[] { profile }));
                Assert.That(exception.InnerException, Is.TypeOf<ArgumentException>());
                Assert.That(exception.InnerException.Message, Does.Contain(expectedError));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        private static ScriptableObject CreateValidProfile(bool reverseOrder)
        {
            object chapterA = CreateChapter(ChapterAId, "EP.01");
            object chapterB = CreateChapter(ChapterBId, "EP.02");
            object canonical = CreateStage(
                CanonicalStageId,
                ChapterAId,
                "01-01",
                "CanonicalPlayable",
                CanonicalCatalogEntryId,
                new Vector2(0.25f, 0.4f));
            object inProduction = CreateStage(
                InProductionStageId,
                ChapterAId,
                "01-02",
                "InProduction",
                mapPosition: new Vector2(0.72f, 0.36f));
            object announced = CreateStage(
                AnnouncedStageId,
                ChapterBId,
                "02-01",
                "Announced",
                mapPosition: new Vector2(0.46f, 0.68f));

            return reverseOrder
                ? CreateProfile(
                    new[] { chapterB, chapterA },
                    new[] { announced, inProduction, canonical })
                : CreateProfile(
                    new[] { chapterA, chapterB },
                    new[] { canonical, inProduction, announced });
        }

        private static ScriptableObject CreateProfile(object[] chapters, object[] stages)
        {
            ScriptableObject profile = CreateUnvalidatedProfile(chapters, stages);
            Assert.That(TryValidate(profile, out string validationError), Is.True, validationError);
            return profile;
        }

        private static ScriptableObject CreateUnvalidatedProfile(object[] chapters, object[] stages)
        {
            ScriptableObject profile = ScriptableObject.CreateInstance(ProfileType);
            ConfigureProfile(profile, chapters, stages);
            return profile;
        }

        private static void ConfigureProfile(
            ScriptableObject profile,
            object[] chapters,
            object[] stages)
        {
            Array typedChapters = CreateTypedArray(ChapterDefinitionType, chapters);
            Array typedStages = CreateTypedArray(StageDefinitionType, stages);
            RequireMethod(ProfileType, "Configure").Invoke(
                profile,
                new object[] { typedChapters, typedStages });
        }

        private static object CreateChapter(
            string chapterId,
            string episodeCode,
            string titleFallback = null)
        {
            return Activator.CreateInstance(
                ChapterDefinitionType,
                chapterId,
                episodeCode,
                "ui.chapter-hub." + chapterId + ".title",
                titleFallback ?? episodeCode + " Review Chapter");
        }

        private static void ConfigureChapter(
            object chapter,
            string chapterId,
            string episodeCode)
        {
            RequireMethod(ChapterDefinitionType, "Configure").Invoke(
                chapter,
                new object[]
                {
                    chapterId,
                    episodeCode,
                    string.Empty,
                    "Mutated"
                });
        }

        private static object CreateStage(
            string stageId,
            string chapterId,
            string stageCode,
            string contentStatus,
            string canonicalCatalogEntryId = "",
            Vector2 mapPosition = default,
            string titleFallback = null)
        {
            return Activator.CreateInstance(
                StageDefinitionType,
                stageId,
                chapterId,
                stageCode,
                "ui.chapter-hub." + stageId + ".title",
                titleFallback ?? stageCode + " Review Stage",
                mapPosition,
                Enum.Parse(ContentStatusType, contentStatus),
                canonicalCatalogEntryId);
        }

        private static void ConfigureStage(
            object stage,
            string stageId,
            string chapterId,
            string stageCode,
            string contentStatus)
        {
            RequireMethod(StageDefinitionType, "Configure").Invoke(
                stage,
                new object[]
                {
                    stageId,
                    chapterId,
                    stageCode,
                    string.Empty,
                    "Mutated",
                    Vector2.zero,
                    Enum.Parse(ContentStatusType, contentStatus),
                    string.Empty
                });
        }

        private static object CreateSession(ScriptableObject profile)
        {
            return Activator.CreateInstance(SessionType, new object[] { profile });
        }

        private static bool TryValidate(ScriptableObject profile, out string validationError)
        {
            object[] arguments = { string.Empty };
            bool valid = (bool)RequireMethod(ProfileType, "TryValidate").Invoke(profile, arguments);
            validationError = arguments[0] as string ?? string.Empty;
            return valid;
        }

        private static bool TryConfirm(object session, out string canonicalCatalogEntryId)
        {
            object[] arguments = { string.Empty };
            bool confirmed = (bool)RequireMethod(
                    SessionType,
                    "TryConfirmSelectedStage")
                .Invoke(session, arguments);
            canonicalCatalogEntryId = arguments[0] as string ?? string.Empty;
            return confirmed;
        }

        private static bool InvokeBool(object target, string methodName, params object[] arguments)
        {
            return (bool)RequireMethod(target.GetType(), methodName).Invoke(target, arguments);
        }

        private static void AssertPhase(object session, string expectedPhase)
        {
            Assert.That(ReadProperty(session, "Phase").ToString(), Is.EqualTo(expectedPhase));
        }

        private static string ReadString(object target, string propertyName)
        {
            return ReadProperty(target, propertyName) as string ?? string.Empty;
        }

        private static bool ReadBool(object target, string propertyName)
        {
            return Convert.ToBoolean(ReadProperty(target, propertyName));
        }

        private static object ReadProperty(object target, string propertyName)
        {
            Assert.That(target, Is.Not.Null, $"Cannot read property '{propertyName}' from null.");
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null, $"Missing property {target.GetType().Name}.{propertyName}.");
            return property.GetValue(target);
        }

        private static MethodInfo RequireMethod(Type type, string methodName)
        {
            MethodInfo method = type.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Missing method {type.Name}.{methodName}.");
            return method;
        }

        private static Type RequireNestedType(Type declaringType, string typeName)
        {
            Type type = declaringType.GetNestedType(
                typeName,
                BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(type, Is.Not.Null, $"Missing nested type {declaringType.Name}.{typeName}.");
            return type;
        }

        private static Type RequireProductType(string fullName)
        {
            Type type = Type.GetType(fullName + ", DimensionBrawl.Runtime")
                ?? Type.GetType(fullName + ", Assembly-CSharp");
            Assert.That(type, Is.Not.Null, $"Missing product type {fullName}.");
            return type;
        }

        private static Array CreateTypedArray(Type elementType, object[] values)
        {
            object[] resolvedValues = values ?? Array.Empty<object>();
            Array array = Array.CreateInstance(elementType, resolvedValues.Length);
            for (int i = 0; i < resolvedValues.Length; i++)
            {
                array.SetValue(resolvedValues[i], i);
            }

            return array;
        }
    }
}
