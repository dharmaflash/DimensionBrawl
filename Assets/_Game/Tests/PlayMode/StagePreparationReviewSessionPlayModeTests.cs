using System;
using System.Reflection;
using DimensionBrawl.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Tests
{
    public sealed class StagePreparationReviewSessionPlayModeTests
    {
        private const string ProductNamespace =
            "DimensionBrawl.UI.StagePreparationReview.";
        private const string CanonicalCatalogEntryId = "story_v1_training_route";
        private static readonly string[] ActionProfilePaths =
        {
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot1_ChargeBruiser.asset",
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot2_LaserSoldier.asset",
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_SummonSlot3_FireDragon.asset"
        };

        [Test]
        public void CanonicalSlotsAndTierSelectionsRemainSessionLocalAndDefensive()
        {
            using var fixture = new ProfileFixture();
            object session = CreateSession(fixture.Profile);

            AssertPhase(session, "StageIntel");
            Assert.That(ReadInt(session, "SlotCount"), Is.EqualTo(3));
            for (int i = 0; i < 3; i++)
            {
                object slot = Invoke(session, "GetSlot", i);
                Assert.That(
                    ReadString(slot, "SlotId"),
                    Is.EqualTo($"SummonSlot{i + 1}"));
                Assert.That(
                    ReadProperty(slot, "ActionProfile"),
                    Is.SameAs(fixture.ActionProfiles[i]));
                Assert.That(ReadInt(slot, "TierCount"), Is.EqualTo(3));
                Assert.That(ReadInt(slot, "TierReadoutCount"), Is.EqualTo(3));
                Assert.That(TryGetTier(session, $"SummonSlot{i + 1}"), Is.EqualTo(1));
            }

            object copiedSlot = Invoke(session, "GetSlot", 0);
            Invoke(
                copiedSlot,
                "Configure",
                "MutatedSlot",
                "mutated.key",
                "MUTATED",
                "MUTATED",
                fixture.ActionProfiles[1],
                fixture.Icon);
            object freshSlot = Invoke(session, "GetSlot", 0);
            Assert.That(ReadString(freshSlot, "SlotId"), Is.EqualTo("SummonSlot1"));
            Assert.That(
                ReadProperty(freshSlot, "ActionProfile"),
                Is.SameAs(fixture.ActionProfiles[0]));

            Assert.That(InvokeBool(session, "TryOpenLoadout"), Is.True);
            Assert.That(InvokeBool(session, "TryInspectSlot", "SummonSlot2"), Is.True);
            Assert.That(InvokeBool(session, "TrySelectTier", 3), Is.True);
            Assert.That(InvokeBool(session, "TryReturnToLoadout"), Is.True);
            Assert.That(InvokeBool(session, "TryInspectSlot", "SummonSlot1"), Is.True);
            Assert.That(InvokeBool(session, "TrySelectTier", 2), Is.True);

            Array firstSnapshot = (Array)Invoke(session, "CreateSelectionSnapshot");
            AssertSelection(firstSnapshot.GetValue(0), "SummonSlot1", fixture.ActionProfiles[0].ActionId, 2);
            AssertSelection(firstSnapshot.GetValue(1), "SummonSlot2", fixture.ActionProfiles[1].ActionId, 3);
            AssertSelection(firstSnapshot.GetValue(2), "SummonSlot3", fixture.ActionProfiles[2].ActionId, 1);

            object replacement = Activator.CreateInstance(
                SelectionType,
                "MutatedSlot",
                "MutatedAction",
                99);
            firstSnapshot.SetValue(replacement, 0);
            Array freshSnapshot = (Array)Invoke(session, "CreateSelectionSnapshot");
            AssertSelection(freshSnapshot.GetValue(0), "SummonSlot1", fixture.ActionProfiles[0].ActionId, 2);
            Assert.That(firstSnapshot, Is.Not.SameAs(freshSnapshot));
        }

        [Test]
        public void NavigationAndReviewAcceptanceAreGuardedAndExactOnce()
        {
            using var fixture = new ProfileFixture();
            object session = CreateSession(fixture.Profile);

            Assert.That(InvokeBool(session, "TryOpenReviewConfirm"), Is.False);
            Assert.That(InvokeBool(session, "TrySelectTier", 2), Is.False);
            Assert.That(InvokeBool(session, "TryOpenLoadout"), Is.True);
            AssertPhase(session, "LoadoutOverview");
            Assert.That(InvokeBool(session, "TryInspectSlot", "missing"), Is.False);
            Assert.That(InvokeBool(session, "TryInspectSlot", "SummonSlot3"), Is.True);
            AssertPhase(session, "SummonDetail");
            Assert.That(InvokeBool(session, "TrySelectTier", 0), Is.False);
            Assert.That(InvokeBool(session, "TrySelectTier", 4), Is.False);
            Assert.That(InvokeBool(session, "TrySelectTier", 3), Is.True);
            Assert.That(ReadInt(session, "SelectedTier"), Is.EqualTo(3));
            Assert.That(InvokeBool(session, "TryReturnToLoadout"), Is.True);
            Assert.That(InvokeBool(session, "TryOpenReviewConfirm"), Is.True);
            AssertPhase(session, "ReviewConfirm");

            Assert.That(InvokeBool(session, "TryAcceptReview"), Is.True);
            Assert.That(ReadBool(session, "IsReviewAccepted"), Is.True);
            Assert.That(InvokeBool(session, "TryAcceptReview"), Is.False);
            Assert.That(InvokeBool(session, "TryReturnToLoadout"), Is.True);
            AssertPhase(session, "LoadoutOverview");
            Assert.That(ReadBool(session, "IsReviewAccepted"), Is.True);
            Assert.That(TryGetTier(session, "SummonSlot3"), Is.EqualTo(3));
        }

        private static Type ProfileType => RequireProductType(
            ProductNamespace + "StagePreparationReviewProfile");
        private static Type SlotDefinitionType => ProfileType.GetNestedType(
            "SlotDefinition",
            BindingFlags.Public | BindingFlags.NonPublic);
        private static Type SessionType => RequireProductType(
            ProductNamespace + "StagePreparationReviewSession");
        private static Type SelectionType => RequireProductType(
            ProductNamespace + "StagePreparationReviewSelection");

        private static object CreateSession(ScriptableObject profile)
        {
            return Activator.CreateInstance(SessionType, new object[] { profile });
        }

        private static int TryGetTier(object session, string slotId)
        {
            object[] arguments = { slotId, 0 };
            bool found = (bool)RequireMethod(
                    session.GetType(),
                    "TryGetSelectedTier",
                    2)
                .Invoke(session, arguments);
            Assert.That(found, Is.True);
            return Convert.ToInt32(arguments[1]);
        }

        private static void AssertSelection(
            object selection,
            string slotId,
            string actionId,
            int tier)
        {
            Assert.That(ReadString(selection, "SlotId"), Is.EqualTo(slotId));
            Assert.That(ReadString(selection, "ActionId"), Is.EqualTo(actionId));
            Assert.That(ReadInt(selection, "SelectedTier"), Is.EqualTo(tier));
        }

        private static void AssertPhase(object session, string expected)
        {
            Assert.That(ReadProperty(session, "Phase").ToString(), Is.EqualTo(expected));
        }

        private static bool InvokeBool(object target, string methodName, params object[] arguments)
        {
            return (bool)Invoke(target, methodName, arguments);
        }

        private static object Invoke(object target, string methodName, params object[] arguments)
        {
            object[] safeArguments = arguments ?? Array.Empty<object>();
            return RequireMethod(target.GetType(), methodName, safeArguments.Length)
                .Invoke(target, safeArguments);
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
            Assert.That(target, Is.Not.Null);
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null, propertyName);
            return property.GetValue(target);
        }

        private static MethodInfo RequireMethod(Type type, string name, int parameterCount)
        {
            foreach (MethodInfo method in type.GetMethods(
                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (method.Name == name && method.GetParameters().Length == parameterCount)
                {
                    return method;
                }
            }

            Assert.Fail($"Missing {type.Name}.{name}/{parameterCount}.");
            return null;
        }

        private static Type RequireProductType(string fullName)
        {
            Type type = Type.GetType(fullName + ", Assembly-CSharp")
                ?? Type.GetType(fullName + ", DimensionBrawl.Runtime");
            Assert.That(type, Is.Not.Null, fullName);
            return type;
        }

        private sealed class ProfileFixture : IDisposable
        {
            private readonly Texture2D texture;

            public ProfileFixture()
            {
                ActionProfiles = new SummonSlotActionProfile[ActionProfilePaths.Length];
                for (int i = 0; i < ActionProfiles.Length; i++)
                {
                    ActionProfiles[i] = AssetDatabase.LoadAssetAtPath<SummonSlotActionProfile>(
                        ActionProfilePaths[i]);
                    Assert.That(ActionProfiles[i], Is.Not.Null, ActionProfilePaths[i]);
                }

                texture = new Texture2D(2, 2);
                Icon = Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.one * 0.5f);
                Profile = ScriptableObject.CreateInstance(ProfileType);
                Array slots = Array.CreateInstance(SlotDefinitionType, 3);
                for (int i = 0; i < 3; i++)
                {
                    string slotId = $"SummonSlot{i + 1}";
                    object slot = Activator.CreateInstance(
                        SlotDefinitionType,
                        slotId,
                        $"ui.stage-preparation.{slotId}.title",
                        $"Canonical Slot {i + 1}",
                        $"Runtime role {i + 1}",
                        ActionProfiles[i],
                        Icon);
                    slots.SetValue(slot, i);
                }

                RequireMethod(ProfileType, "Configure", 9).Invoke(
                    Profile,
                    new object[]
                    {
                        "PREP-01",
                        CanonicalCatalogEntryId,
                        "ui.stage-preparation.title",
                        "Stage Preparation Review",
                        "pilot.fixed.review",
                        "ui.stage-preparation.pilot",
                        "Fixed Pilot Presentation",
                        "CANONICAL RUNTIME PRESET / NOT A STAGE RECOMMENDATION",
                        slots
                    });

                object[] validationArguments = { string.Empty };
                bool valid = (bool)RequireMethod(ProfileType, "TryValidate", 1)
                    .Invoke(Profile, validationArguments);
                Assert.That(valid, Is.True, validationArguments[0]?.ToString());
            }

            public ScriptableObject Profile { get; }
            public SummonSlotActionProfile[] ActionProfiles { get; }
            public Sprite Icon { get; }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(Profile);
                UnityEngine.Object.DestroyImmediate(Icon);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}
