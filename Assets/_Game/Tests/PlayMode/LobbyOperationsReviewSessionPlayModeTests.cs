using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace DimensionBrawl.Tests
{
    public sealed class LobbyOperationsReviewSessionPlayModeTests
    {
        private const string ProductNamespace =
            "DimensionBrawl.UI.LobbyOperationsReview.";
        private const string NoticeEntryId = "review.operations.notice";
        private const string MailboxEntryId = "review.operations.mailbox";
        private const string MissionsEntryId = "review.operations.missions";
        private const string EventCalendarEntryId = "review.operations.event-calendar";

        [Test]
        public void DefaultProfileMatchesTheExactFourEntryContract()
        {
            ScriptableObject profile = CreateValidProfile();
            try
            {
                Assert.That(ReadInt(profile, "EntryCount"), Is.EqualTo(4));

                object notice = GetEntry(profile, 0);
                object mailbox = GetEntry(profile, 1);
                object missions = GetEntry(profile, 2);
                object eventCalendar = GetEntry(profile, 3);
                AssertEntryContract(
                    notice,
                    NoticeEntryId,
                    "Notice",
                    "LocalReviewFixture",
                    "NotRequiredForReview",
                    "NotRequiredForReview",
                    "NotRequiredForReview",
                    "NotRequiredForReview",
                    "NotRequiredForReview",
                    "NotRequiredForReview",
                    "LocalReviewConfirm");
                AssertEntryContract(
                    mailbox,
                    MailboxEntryId,
                    "Mailbox",
                    "ReviewShellNoProductCommitment",
                    "NoVerifiedSource",
                    "NoVerifiedSource",
                    "NotRequiredForReview",
                    "NotRequiredForReview",
                    "NotRequiredForReview",
                    "NoVerifiedSource",
                    "ExplanationOnly");
                AssertEntryContract(
                    missions,
                    MissionsEntryId,
                    "Missions",
                    "ReviewShellNoProductCommitment",
                    "NotRequiredForReview",
                    "NoVerifiedSource",
                    "NotRequiredForReview",
                    "NotRequiredForReview",
                    "NoVerifiedSource",
                    "NoVerifiedSource",
                    "ExplanationOnly");
                AssertEntryContract(
                    eventCalendar,
                    EventCalendarEntryId,
                    "EventCalendar",
                    "DefinitionOnlyReviewShell",
                    "NoVerifiedSource",
                    "NotRequiredForReview",
                    "NoVerifiedSource",
                    "DefinitionOnlyNoVerdict",
                    "NotRequiredForReview",
                    "NoVerifiedSource",
                    "ExplanationOnly");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void ValidationRejectsWrongCountDuplicateAndWrongOrder()
        {
            AssertInvalid("exactly 4 entries are required", CreateEntryArray());

            Array duplicateEntries = CreateDefaultEntries();
            duplicateEntries.SetValue(duplicateEntries.GetValue(0), 1);
            AssertInvalid("duplicate entry id", duplicateEntries);
            AssertInvalid("duplicate entry kind", duplicateEntries);

            Array swappedEntries = CreateDefaultEntries();
            object first = swappedEntries.GetValue(0);
            swappedEntries.SetValue(swappedEntries.GetValue(1), 0);
            swappedEntries.SetValue(first, 1);
            AssertInvalid("required order", swappedEntries);
        }

        [Test]
        public void ValidationRequiresFallbackTitleAndExplanation()
        {
            Array noTitle = CreateDefaultEntries();
            object notice = noTitle.GetValue(0);
            ReconfigureEntry(
                notice,
                string.Empty,
                ReadString(notice, "ExplanationFallback"),
                ReadProperty(notice, "ServerClockDisposition"),
                ReadProperty(notice, "ScheduleDisposition"),
                ReadProperty(notice, "ProgressDisposition"),
                ReadProperty(notice, "ActionDisposition"));
            AssertInvalid("has no fallback title", noTitle);

            Array noExplanation = CreateDefaultEntries();
            object missions = noExplanation.GetValue(2);
            ReconfigureEntry(
                missions,
                ReadString(missions, "TitleFallback"),
                string.Empty,
                ReadProperty(missions, "ServerClockDisposition"),
                ReadProperty(missions, "ScheduleDisposition"),
                ReadProperty(missions, "ProgressDisposition"),
                ReadProperty(missions, "ActionDisposition"));
            AssertInvalid("has no fallback explanation", noExplanation);

            Array authoredFallbackDrift = CreateDefaultEntries();
            object authoredNotice = authoredFallbackDrift.GetValue(0);
            ReconfigureEntry(
                authoredNotice,
                "LIVE SERVICE NOTICE",
                ReadString(authoredNotice, "ExplanationFallback"),
                ReadProperty(authoredNotice, "ServerClockDisposition"),
                ReadProperty(authoredNotice, "ScheduleDisposition"),
                ReadProperty(authoredNotice, "ProgressDisposition"),
                ReadProperty(authoredNotice, "ActionDisposition"));
            AssertInvalid(
                "TitleFallback must match the authored review contract",
                authoredFallbackDrift);

            Array localizationKeyDrift = CreateDefaultEntries();
            object authoredMailbox = localizationKeyDrift.GetValue(1);
            FieldInfo titleKeyField = EntryDefinitionType.GetField(
                "titleLocalizationKey",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(titleKeyField, Is.Not.Null);
            titleKeyField.SetValue(authoredMailbox, "ui.product.mailbox.live");
            AssertInvalid(
                "TitleLocalizationKey must match the authored review contract",
                localizationKeyDrift);
        }

        [Test]
        public void ValidationRejectsDispositionDriftBetweenResponsibilities()
        {
            Array mailboxWithLocalAction = CreateDefaultEntries();
            object mailbox = mailboxWithLocalAction.GetValue(1);
            ReconfigureEntry(
                mailbox,
                ReadString(mailbox, "TitleFallback"),
                ReadString(mailbox, "ExplanationFallback"),
                ReadProperty(mailbox, "ServerClockDisposition"),
                ReadProperty(mailbox, "ScheduleDisposition"),
                ReadProperty(mailbox, "ProgressDisposition"),
                EnumValue(ActionDispositionType, "LocalReviewConfirm"));
            AssertInvalid(
                "ActionDisposition must be 'ExplanationOnly'",
                mailboxWithLocalAction);

            Array missionsWithInferredProgress = CreateDefaultEntries();
            object missions = missionsWithInferredProgress.GetValue(2);
            ReconfigureEntry(
                missions,
                ReadString(missions, "TitleFallback"),
                ReadString(missions, "ExplanationFallback"),
                ReadProperty(missions, "ServerClockDisposition"),
                ReadProperty(missions, "ScheduleDisposition"),
                EnumValue(ProgressDispositionType, "NotRequiredForReview"),
                ReadProperty(missions, "ActionDisposition"));
            AssertInvalid(
                "ProgressDisposition must be 'NoVerifiedSource'",
                missionsWithInferredProgress);

            Array eventWithInferredSchedule = CreateDefaultEntries();
            object eventCalendar = eventWithInferredSchedule.GetValue(3);
            ReconfigureEntry(
                eventCalendar,
                ReadString(eventCalendar, "TitleFallback"),
                ReadString(eventCalendar, "ExplanationFallback"),
                EnumValue(ServerClockDispositionType, "NotRequiredForReview"),
                EnumValue(ScheduleDispositionType, "NoVerifiedSource"),
                ReadProperty(eventCalendar, "ProgressDisposition"),
                ReadProperty(eventCalendar, "ActionDisposition"));
            AssertInvalid(
                "ServerClockDisposition must be 'NoVerifiedSource'",
                eventWithInferredSchedule);
            AssertInvalid(
                "ScheduleDisposition must be 'DefinitionOnlyNoVerdict'",
                eventWithInferredSchedule);
        }

        [Test]
        public void EntrySchemaContainsOnlyLabelsAndSeparatedDispositionFields()
        {
            FieldInfo[] fields = EntryDefinitionType.GetFields(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            Assert.That(fields, Is.Not.Empty);
            foreach (FieldInfo field in fields)
            {
                Assert.That(
                    field.FieldType == typeof(string) || field.FieldType.IsEnum,
                    Is.True,
                    $"Forbidden value-bearing schema field '{field.Name}' has type '{field.FieldType}'.");
            }

            string[] forbiddenMemberNames =
            {
                "IsAvailable",
                "IsLocked",
                "UnreadCount",
                "Counter",
                "Reward",
                "Attachment",
                "Cost",
                "Price",
                "Currency",
                "AccountId",
                "Timestamp",
                "Transaction",
                "Url",
                "Route",
                "Payload",
                "StartDate",
                "EndDate",
                "ReleaseDate",
                "ProgressValue",
                "ServerTimeValue"
            };
            MemberInfo[] members = EntryDefinitionType.GetMembers(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (MemberInfo member in members)
            {
                foreach (string forbiddenName in forbiddenMemberNames)
                {
                    Assert.That(
                        member.Name.IndexOf(forbiddenName, StringComparison.OrdinalIgnoreCase),
                        Is.EqualTo(-1),
                        $"Forbidden schema member '{member.Name}' contains '{forbiddenName}'.");
                }
            }
        }

        [Test]
        public void SessionRejectsNullAndInvalidProfiles()
        {
            TargetInvocationException nullException = Assert.Throws<TargetInvocationException>(
                () => Activator.CreateInstance(SessionType, new object[] { null }));
            Assert.That(nullException.InnerException, Is.TypeOf<ArgumentNullException>());

            ScriptableObject invalidProfile = ScriptableObject.CreateInstance(ProfileType);
            try
            {
                ConfigureProfile(invalidProfile, CreateEntryArray());
                TargetInvocationException exception = Assert.Throws<TargetInvocationException>(
                    () => Activator.CreateInstance(SessionType, new object[] { invalidProfile }));
                Assert.That(exception.InnerException, Is.TypeOf<ArgumentException>());
                Assert.That(
                    exception.InnerException.Message,
                    Does.Contain("exactly 4 entries are required"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(invalidProfile);
            }
        }

        [Test]
        public void EveryProfileAndSessionReadPathReturnsAnIndependentDefensiveCopy()
        {
            ScriptableObject profile = CreateValidProfile();
            try
            {
                object session = CreateSession(profile);

                AssertActionCopyIsIndependent(
                    () => ((Array)ReadProperty(profile, "Entries")).GetValue(0),
                    () => GetEntry(profile, 0),
                    "Profile.Entries");
                AssertActionCopyIsIndependent(
                    () => GetEntry(profile, 0),
                    () => GetEntry(profile, 0),
                    "Profile.GetEntry");
                AssertActionCopyIsIndependent(
                    () => GetEntry(session, 0),
                    () => GetEntry(session, 0),
                    "Session.GetEntry");
                AssertActionCopyIsIndependent(
                    () => GetSessionEntry(session, NoticeEntryId),
                    () => GetSessionEntry(session, NoticeEntryId),
                    "Session.TryGetEntry");

                Assert.That(InvokeBool(session, "TryOpenDrawer"), Is.True);
                Assert.That(InvokeBool(session, "TrySelectEntry", NoticeEntryId), Is.True);
                AssertActionCopyIsIndependent(
                    () => ReadProperty(session, "SelectedEntry"),
                    () => ReadProperty(session, "SelectedEntry"),
                    "Session.SelectedEntry");

                ConfigureProfile(profile, CreateEntryArray());
                Assert.That(
                    ReadEnumName(ReadProperty(session, "SelectedEntry"), "ActionDisposition"),
                    Is.EqualTo("LocalReviewConfirm"));
                Assert.That(InvokeBool(session, "TryOpenReviewConfirm"), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void NoticeFlowAndBackStackAreDeterministic()
        {
            using SessionFixture fixture = new();
            object session = fixture.Session;

            AssertPhase(session, "Closed");
            Assert.That(InvokeBool(session, "TryBack"), Is.False);
            Assert.That(InvokeBool(session, "TryOpenDrawer"), Is.True);
            AssertPhase(session, "Directory");
            Assert.That(InvokeBool(session, "TryOpenDrawer"), Is.False);
            Assert.That(InvokeBool(session, "TrySelectEntry", NoticeEntryId), Is.True);
            AssertPhase(session, "EntryDetail");
            Assert.That(InvokeBool(session, "TryOpenReviewConfirm"), Is.True);
            AssertPhase(session, "ReviewConfirm");

            Assert.That(InvokeBool(session, "TryBack"), Is.True);
            AssertPhase(session, "EntryDetail");
            Assert.That(InvokeBool(session, "TryBack"), Is.True);
            AssertPhase(session, "Directory");
            Assert.That(ReadString(session, "SelectedEntryId"), Is.Empty);
            Assert.That(InvokeBool(session, "TryBack"), Is.True);
            AssertPhase(session, "Closed");
            Assert.That(InvokeBool(session, "TryBack"), Is.False);
        }

        [Test]
        public void InvalidCommandsAndCloseTransitionsFailWithoutInventingState()
        {
            using SessionFixture fixture = new();
            object session = fixture.Session;

            Assert.That(InvokeBool(session, "TrySelectEntry", NoticeEntryId), Is.False);
            Assert.That(InvokeBool(session, "TryOpenReviewConfirm"), Is.False);
            Assert.That(TryAcknowledge(session, out string closedEntryId), Is.False);
            Assert.That(closedEntryId, Is.Empty);
            AssertPhase(session, "Closed");

            Assert.That(InvokeBool(session, "TryOpenDrawer"), Is.True);
            Assert.That(
                InvokeBool(session, "TrySelectEntry", "review.operations.unknown"),
                Is.False);
            AssertPhase(session, "Directory");
            Assert.That(InvokeBool(session, "TryClose"), Is.True);
            AssertPhase(session, "Closed");

            Assert.That(InvokeBool(session, "TryOpenDrawer"), Is.True);
            Assert.That(InvokeBool(session, "TrySelectEntry", MailboxEntryId), Is.True);
            Assert.That(InvokeBool(session, "TryClose"), Is.True);
            AssertPhase(session, "Closed");
            Assert.That(ReadString(session, "SelectedEntryId"), Is.Empty);
            Assert.That(ReadProperty(session, "SelectedEntry"), Is.Null);
        }

        [Test]
        public void MissingSourceDispositionsRemainDistinctAndFailClosed()
        {
            using SessionFixture fixture = new();
            object mailbox = GetSessionEntry(fixture.Session, MailboxEntryId);
            object missions = GetSessionEntry(fixture.Session, MissionsEntryId);
            object eventCalendar = GetSessionEntry(fixture.Session, EventCalendarEntryId);

                Assert.That(
                    ReadEnumName(mailbox, "ServiceDisposition"),
                    Is.EqualTo("NoVerifiedSource"));
                Assert.That(
                    ReadEnumName(mailbox, "AccountDisposition"),
                    Is.EqualTo("NoVerifiedSource"));
                Assert.That(
                    ReadEnumName(mailbox, "AttentionDisposition"),
                    Is.EqualTo("NoVerifiedSource"));

                Assert.That(
                    ReadEnumName(missions, "ServiceDisposition"),
                    Is.EqualTo("NotRequiredForReview"));
                Assert.That(
                    ReadEnumName(missions, "AccountDisposition"),
                    Is.EqualTo("NoVerifiedSource"));
                Assert.That(
                    ReadEnumName(missions, "ProgressDisposition"),
                    Is.EqualTo("NoVerifiedSource"));

                Assert.That(
                    ReadEnumName(eventCalendar, "ServiceDisposition"),
                    Is.EqualTo("NoVerifiedSource"));
                Assert.That(
                    ReadEnumName(eventCalendar, "AccountDisposition"),
                    Is.EqualTo("NotRequiredForReview"));
                Assert.That(
                    ReadEnumName(eventCalendar, "ServerClockDisposition"),
                    Is.EqualTo("NoVerifiedSource"));
                Assert.That(
                    ReadEnumName(eventCalendar, "ScheduleDisposition"),
                    Is.EqualTo("DefinitionOnlyNoVerdict"));
        }

        [TestCase(MailboxEntryId)]
        [TestCase(MissionsEntryId)]
        [TestCase(EventCalendarEntryId)]
        public void ExplanationOnlyEntriesCannotOpenReviewConfirm(string entryId)
        {
            using SessionFixture fixture = new();
            object session = fixture.Session;

            Assert.That(InvokeBool(session, "TryOpenDrawer"), Is.True);
            Assert.That(InvokeBool(session, "TrySelectEntry", entryId), Is.True);
            Assert.That(InvokeBool(session, "TryOpenReviewConfirm"), Is.False);
            AssertPhase(session, "EntryDetail");
            Assert.That(ReadBool(session, "IsReviewAcknowledged"), Is.False);
        }

        [Test]
        public void ReviewAcknowledgmentIsAcceptedExactlyOnce()
        {
            using SessionFixture fixture = new();
            object session = fixture.Session;
            OpenNoticeConfirm(session);

            Assert.That(TryAcknowledge(session, out string firstEntryId), Is.True);
            Assert.That(firstEntryId, Is.EqualTo(NoticeEntryId));
            Assert.That(ReadBool(session, "IsReviewAcknowledged"), Is.True);

            Assert.That(TryAcknowledge(session, out string repeatedEntryId), Is.False);
            Assert.That(repeatedEntryId, Is.Empty);
            Assert.That(ReadBool(session, "IsReviewAcknowledged"), Is.True);
        }

        [Test]
        public void NewSessionResetsAcknowledgmentAndAcceptsItsFirstNoticeReview()
        {
            ScriptableObject profile = CreateValidProfile();
            try
            {
                object firstSession = CreateSession(profile);
                OpenNoticeConfirm(firstSession);
                Assert.That(TryAcknowledge(firstSession, out string firstEntryId), Is.True);
                Assert.That(firstEntryId, Is.EqualTo(NoticeEntryId));
                Assert.That(ReadBool(firstSession, "IsReviewAcknowledged"), Is.True);

                object newSession = CreateSession(profile);
                AssertPhase(newSession, "Closed");
                Assert.That(ReadBool(newSession, "IsReviewAcknowledged"), Is.False);
                OpenNoticeConfirm(newSession);
                Assert.That(TryAcknowledge(newSession, out string newEntryId), Is.True);
                Assert.That(newEntryId, Is.EqualTo(NoticeEntryId));
                Assert.That(ReadBool(newSession, "IsReviewAcknowledged"), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void CloseAndReopenPreserveTheAcknowledgmentLatch()
        {
            using SessionFixture fixture = new();
            object session = fixture.Session;
            OpenNoticeConfirm(session);
            Assert.That(TryAcknowledge(session, out _), Is.True);

            Assert.That(InvokeBool(session, "TryClose"), Is.True);
            AssertPhase(session, "Closed");
            Assert.That(ReadString(session, "SelectedEntryId"), Is.Empty);
            Assert.That(ReadBool(session, "IsReviewAcknowledged"), Is.True);
            Assert.That(InvokeBool(session, "TryClose"), Is.False);

            OpenNoticeConfirm(session);
            Assert.That(TryAcknowledge(session, out string repeatedEntryId), Is.False);
            Assert.That(repeatedEntryId, Is.Empty);
            Assert.That(ReadBool(session, "IsReviewAcknowledged"), Is.True);
        }

        private static Type ProfileType =>
            RequireProductType(ProductNamespace + "LobbyOperationsReviewProfile");
        private static Type EntryDefinitionType =>
            RequireNestedType(ProfileType, "EntryDefinition");
        private static Type SessionType =>
            RequireProductType(ProductNamespace + "LobbyOperationsReviewSession");
        private static Type ActionDispositionType =>
            RequireProductType(ProductNamespace + "LobbyOperationsReviewActionDisposition");
        private static Type ServerClockDispositionType =>
            RequireProductType(ProductNamespace + "LobbyOperationsReviewServerClockDisposition");
        private static Type ScheduleDispositionType =>
            RequireProductType(ProductNamespace + "LobbyOperationsReviewScheduleDisposition");
        private static Type ProgressDispositionType =>
            RequireProductType(ProductNamespace + "LobbyOperationsReviewProgressDisposition");

        private static void AssertEntryContract(
            object entry,
            string entryId,
            string kind,
            string production,
            string service,
            string account,
            string serverClock,
            string schedule,
            string progress,
            string attention,
            string action)
        {
            Assert.That(ReadString(entry, "EntryId"), Is.EqualTo(entryId));
            Assert.That(ReadEnumName(entry, "Kind"), Is.EqualTo(kind));
            Assert.That(
                ReadEnumName(entry, "ProductionDisposition"),
                Is.EqualTo(production));
            Assert.That(ReadEnumName(entry, "ServiceDisposition"), Is.EqualTo(service));
            Assert.That(ReadEnumName(entry, "AccountDisposition"), Is.EqualTo(account));
            Assert.That(
                ReadEnumName(entry, "ServerClockDisposition"),
                Is.EqualTo(serverClock));
            Assert.That(ReadEnumName(entry, "ScheduleDisposition"), Is.EqualTo(schedule));
            Assert.That(ReadEnumName(entry, "ProgressDisposition"), Is.EqualTo(progress));
            Assert.That(ReadEnumName(entry, "AttentionDisposition"), Is.EqualTo(attention));
            Assert.That(ReadEnumName(entry, "ActionDisposition"), Is.EqualTo(action));
            Assert.That(ReadString(entry, "TitleFallback"), Is.Not.Empty);
            Assert.That(ReadString(entry, "ExplanationFallback"), Is.Not.Empty);
        }

        private static void AssertActionCopyIsIndependent(
            Func<object> readExposedCopy,
            Func<object> readFreshCopy,
            string readPath)
        {
            object exposed = readExposedCopy();
            ReconfigureEntry(
                exposed,
                ReadString(exposed, "TitleFallback"),
                ReadString(exposed, "ExplanationFallback"),
                ReadProperty(exposed, "ServerClockDisposition"),
                ReadProperty(exposed, "ScheduleDisposition"),
                ReadProperty(exposed, "ProgressDisposition"),
                EnumValue(ActionDispositionType, "ExplanationOnly"));

            Assert.That(
                ReadEnumName(exposed, "ActionDisposition"),
                Is.EqualTo("ExplanationOnly"),
                $"{readPath} did not return a mutable caller-owned copy.");
            Assert.That(
                ReadEnumName(readFreshCopy(), "ActionDisposition"),
                Is.EqualTo("LocalReviewConfirm"),
                $"Mutation escaped through {readPath}.");
        }

        private static ScriptableObject CreateValidProfile()
        {
            ScriptableObject profile = ScriptableObject.CreateInstance(ProfileType);
            ConfigureProfile(profile, CreateDefaultEntries());
            Assert.That(TryValidate(profile, out string validationError), Is.True, validationError);
            return profile;
        }

        private static object CreateSession(ScriptableObject profile)
        {
            return Activator.CreateInstance(SessionType, new object[] { profile });
        }

        private static Array CreateDefaultEntries()
        {
            MethodInfo method = ProfileType.GetMethod(
                "CreateDefaultEntries",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "Missing LobbyOperationsReviewProfile.CreateDefaultEntries.");
            return (Array)method.Invoke(null, null);
        }

        private static Array CreateEntryArray(params object[] values)
        {
            object[] resolvedValues = values ?? Array.Empty<object>();
            Array array = Array.CreateInstance(EntryDefinitionType, resolvedValues.Length);
            for (int index = 0; index < resolvedValues.Length; index++)
            {
                array.SetValue(resolvedValues[index], index);
            }

            return array;
        }

        private static void ConfigureProfile(ScriptableObject profile, Array entries)
        {
            RequireMethod(ProfileType, "Configure").Invoke(profile, new object[] { entries });
        }

        private static void ReconfigureEntry(
            object entry,
            string titleFallback,
            string explanationFallback,
            object serverClockDisposition,
            object scheduleDisposition,
            object progressDisposition,
            object actionDisposition)
        {
            RequireMethod(EntryDefinitionType, "Configure").Invoke(
                entry,
                new[]
                {
                    ReadProperty(entry, "EntryId"),
                    ReadProperty(entry, "Kind"),
                    ReadProperty(entry, "TitleLocalizationKey"),
                    titleFallback,
                    ReadProperty(entry, "ExplanationLocalizationKey"),
                    explanationFallback,
                    ReadProperty(entry, "ProductionDisposition"),
                    ReadProperty(entry, "ServiceDisposition"),
                    ReadProperty(entry, "AccountDisposition"),
                    serverClockDisposition,
                    scheduleDisposition,
                    progressDisposition,
                    ReadProperty(entry, "AttentionDisposition"),
                    actionDisposition
                });
        }

        private static void AssertInvalid(string expectedError, Array entries)
        {
            ScriptableObject profile = ScriptableObject.CreateInstance(ProfileType);
            try
            {
                ConfigureProfile(profile, entries);
                Assert.That(TryValidate(profile, out string validationError), Is.False);
                Assert.That(validationError, Does.Contain(expectedError));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(profile);
            }
        }

        private static bool TryValidate(
            ScriptableObject profile,
            out string validationError)
        {
            object[] arguments = { string.Empty };
            bool valid = (bool)RequireMethod(ProfileType, "TryValidate")
                .Invoke(profile, arguments);
            validationError = arguments[0] as string ?? string.Empty;
            return valid;
        }

        private static object GetEntry(object target, int index)
        {
            return RequireMethod(target.GetType(), "GetEntry")
                .Invoke(target, new object[] { index });
        }

        private static object GetSessionEntry(object session, string entryId)
        {
            object[] arguments = { entryId, null };
            bool found = (bool)RequireMethod(SessionType, "TryGetEntry")
                .Invoke(session, arguments);
            Assert.That(found, Is.True, $"Missing session entry '{entryId}'.");
            Assert.That(arguments[1], Is.Not.Null);
            return arguments[1];
        }

        private static bool TryAcknowledge(object session, out string entryId)
        {
            object[] arguments = { string.Empty };
            bool acknowledged = (bool)RequireMethod(SessionType, "TryAcknowledgeReview")
                .Invoke(session, arguments);
            entryId = arguments[0] as string ?? string.Empty;
            return acknowledged;
        }

        private static void OpenNoticeConfirm(object session)
        {
            Assert.That(InvokeBool(session, "TryOpenDrawer"), Is.True);
            Assert.That(InvokeBool(session, "TrySelectEntry", NoticeEntryId), Is.True);
            Assert.That(InvokeBool(session, "TryOpenReviewConfirm"), Is.True);
            AssertPhase(session, "ReviewConfirm");
        }

        private static bool InvokeBool(object target, string methodName, params object[] arguments)
        {
            return (bool)RequireMethod(target.GetType(), methodName).Invoke(target, arguments);
        }

        private static void AssertPhase(object session, string expectedPhase)
        {
            Assert.That(ReadProperty(session, "Phase").ToString(), Is.EqualTo(expectedPhase));
        }

        private static string ReadEnumName(object target, string propertyName)
        {
            return ReadProperty(target, propertyName).ToString();
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

        private static object ReadProperty(object target, string propertyName)
        {
            Assert.That(target, Is.Not.Null, $"Cannot read '{propertyName}' from null.");
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

        private static object EnumValue(Type enumType, string value)
        {
            return Enum.Parse(enumType, value);
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

        private sealed class SessionFixture : IDisposable
        {
            public SessionFixture()
            {
                Profile = CreateValidProfile();
                Session = CreateSession(Profile);
            }

            public ScriptableObject Profile { get; }
            public object Session { get; }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(Profile);
            }
        }
    }
}
