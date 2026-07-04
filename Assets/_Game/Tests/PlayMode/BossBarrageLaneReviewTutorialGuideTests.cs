using System;
using System.Reflection;
using DimensionBrawl.Test;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DimensionBrawl.Tests
{
    public sealed class BossBarrageLaneReviewTutorialGuideTests
    {
        private const string InvasionTutorialProfilePath =
            "Assets/_Game/DesignData/Profiles/ActionFoundation/DB_BossBarragePocketTutorial_Invasion.asset";
        private const string ProfileTypeName = "DimensionBrawl.UI.BossBarrageLaneReviewTutorialProfile";
        private const string StepTypeName = "DimensionBrawl.UI.BossBarrageLaneReviewTutorialProfile+Step";
        private const string GuideTypeName = "DimensionBrawl.UI.BossBarrageLaneReviewTutorialGuide";
        private const string ActionIdTypeName = "DimensionBrawl.UI.CombatHudActionId";
        private const string ConditionTypeName = "DimensionBrawl.UI.BossBarrageLaneReviewTutorialCondition";

        [Test]
        public void InputDuringCueReadDoesNotAdvanceTutorialStep()
        {
            GuideContext context = CreateGuideContext(
                CreateStep(
                    "dodge",
                    "Dodge step",
                    "Dodge prompt",
                    ParseActionId("Dodge"),
                    ParseCondition("DodgeStarted")),
                CreateStep(
                    "next",
                    "Next step",
                    "Next prompt",
                    ParseActionId("None"),
                    ParseCondition("None")));

            try
            {
                Assert.AreEqual("Dodge step", GetStringProperty(context.Guide, "CurrentObjective"));

                InvokePrivate(context.Guide, "HandleDodgeStarted");
                Tick(context.Guide, 2f);
                Assert.AreEqual(
                    "Dodge step",
                    GetStringProperty(context.Guide, "CurrentObjective"),
                    "A dodge fired during the cue/read lock must not complete the step later.");

                InvokePrivate(context.Guide, "HandleDodgeStarted");
                Tick(context.Guide, 0.1f);
                Assert.That(GetStringProperty(context.Guide, "CurrentPrompt"), Does.Contain("회피 확인"));

                Tick(context.Guide, 1f);
                Assert.AreEqual("Next step", GetStringProperty(context.Guide, "CurrentObjective"));
            }
            finally
            {
                context.Destroy();
            }
        }

        [Test]
        public void PocketRecordStateWaitsForCueReadBeforeCompleting()
        {
            GuideContext context = CreateGuideContext(
                CreateStep(
                    "close",
                    "Close step",
                    "Close prompt",
                    ParseActionId("BasicAttack"),
                    ParseCondition("CloseThreatDefeated")),
                CreateStep(
                    "next",
                    "Next step",
                    "Next prompt",
                    ParseActionId("None"),
                    ParseCondition("None")));

            try
            {
                SetPrivateField(context.PocketOwner, "closeThreatDefeated", true);
                InvokePublic(context.Guide, "RestartGuide");

                Assert.That(GetStringProperty(context.Guide, "CurrentPrompt"), Does.Contain("RECORD close:OK"));
                Tick(context.Guide, 0.8f);
                Assert.AreEqual(
                    "Close step",
                    GetStringProperty(context.Guide, "CurrentObjective"),
                    "A pre-existing pocket result should be readable before it completes the guide step.");

                Tick(context.Guide, 0.7f);
                Assert.That(GetStringProperty(context.Guide, "CurrentPrompt"), Does.Contain("근접 압박 처리 기록"));

                Tick(context.Guide, 1f);
                Assert.AreEqual("Next step", GetStringProperty(context.Guide, "CurrentObjective"));
            }
            finally
            {
                context.Destroy();
            }
        }

        [Test]
        public void InvasionTutorialProfileKeepsInputAndRecordFlow()
        {
            Type profileType = RequireType(ProfileTypeName);
            UnityEngine.Object profile = AssetDatabase.LoadAssetAtPath(InvasionTutorialProfilePath, profileType);
            Assert.IsNotNull(profile, "Invasion should keep an authored boss barrage tutorial profile.");
            Assert.AreEqual(7, GetIntProperty(profile, "StepCount"));

            AssertStep(profile, 0, "dodge_barrage", "DodgeStarted");
            AssertStep(profile, 1, "forward_en", "ForwardRiskEntered");
            AssertStep(profile, 2, "slot_read", "SummonSlot1Ready");
            AssertStep(profile, 3, "close_probe", "CloseThreatDefeated");
            AssertStep(profile, 4, "s1_block", "SummonSlot1PressureBlocked");
            AssertStep(profile, 5, "skill1_followup", "Skill1FollowupHit");
            AssertStep(profile, 6, "pocket_result", "PocketCleared");

            Assert.That(GetStringProperty(GetStep(profile, 2), "PromptText"), Does.Contain("S1"));
            Assert.That(GetStringProperty(GetStep(profile, 2), "PromptText"), Does.Contain("S2"));
            Assert.That(GetStringProperty(GetStep(profile, 2), "PromptText"), Does.Contain("S3"));
            Assert.That(GetStringProperty(GetStep(profile, 3), "PromptText"), Does.Contain("BasicDefenseFire"));
            Assert.That(GetStringProperty(GetStep(profile, 5), "PromptText"), Does.Contain("Skill1"));
            Assert.That(GetStringProperty(GetStep(profile, 6), "PromptText"), Does.Contain("close"));
            Assert.That(GetStringProperty(GetStep(profile, 6), "PromptText"), Does.Contain("summon"));
            Assert.That(GetStringProperty(GetStep(profile, 6), "PromptText"), Does.Contain("followup"));
        }

        private static GuideContext CreateGuideContext(params object[] steps)
        {
            Type profileType = RequireType(ProfileTypeName);
            var profile = ScriptableObject.CreateInstance(profileType);
            SetPrivateField(profile, "tutorialEnabled", true);
            SetPrivateField(profile, "clearObjective", "clear");
            SetPrivateField(profile, "failObjective", "fail");
            SetPrivateField(profile, "steps", CreateStepArray(steps));

            var ownerObject = new GameObject("TutorialGuideTest_PocketOwner");
            BossBarragePocketReviewOwner owner = ownerObject.AddComponent<BossBarragePocketReviewOwner>();

            Type guideType = RequireType(GuideTypeName);
            var guideObject = new GameObject("TutorialGuideTest_Guide");
            Component guide = guideObject.AddComponent(guideType);
            SetPrivateField(guide, "profile", profile);
            SetPrivateField(guide, "cueReadSeconds", 0.45f);
            SetPrivateField(guide, "minimumStepReadSeconds", 0.85f);
            SetPrivateField(guide, "completionHoldSeconds", 0.85f);
            InvokePublic(guide, "BindRuntimeContext", owner, null, null, null, null, null);
            InvokePublic(guide, "RestartGuide");

            return new GuideContext(profile, ownerObject, guideObject, owner, guide);
        }

        private static object CreateStep(
            string stepId,
            string objectiveText,
            string promptText,
            object focusAction,
            object completionCondition)
        {
            object step = Activator.CreateInstance(RequireType(StepTypeName));
            SetPrivateField(step, "stepId", stepId);
            SetPrivateField(step, "objectiveText", objectiveText);
            SetPrivateField(step, "promptText", promptText);
            SetPrivateField(step, "focusAction", focusAction);
            SetPrivateField(step, "dimUnfocusedActions", true);
            SetPrivateField(step, "completionCondition", completionCondition);
            SetPrivateField(step, "requiredTier", 1);
            SetPrivateField(step, "requiredMana", 0f);
            SetPrivateField(step, "minimumSeconds", 0.1f);
            return step;
        }

        private static Array CreateStepArray(object[] steps)
        {
            Array array = Array.CreateInstance(RequireType(StepTypeName), steps.Length);
            for (int i = 0; i < steps.Length; i++)
            {
                array.SetValue(steps[i], i);
            }

            return array;
        }

        private static void AssertStep(UnityEngine.Object profile, int index, string stepId, string condition)
        {
            object step = GetStep(profile, index);
            Assert.IsNotNull(step, $"Missing tutorial step {index}.");
            Assert.AreEqual(stepId, GetStringProperty(step, "StepId"));
            Assert.AreEqual(condition, GetPropertyValue(step, "CompletionCondition").ToString());
        }

        private static object GetStep(UnityEngine.Object profile, int index)
        {
            MethodInfo method = profile.GetType().GetMethod("GetStep", BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(method, "Profile should expose GetStep.");
            return method.Invoke(profile, new object[] { index });
        }

        private static object ParseActionId(string value)
        {
            return Enum.Parse(RequireType(ActionIdTypeName), value);
        }

        private static object ParseCondition(string value)
        {
            return Enum.Parse(RequireType(ConditionTypeName), value);
        }

        private static void Tick(Component guide, float seconds)
        {
            float remaining = Mathf.Max(0f, seconds);
            while (remaining > 0f)
            {
                float deltaTime = Mathf.Min(0.1f, remaining);
                InvokePublic(guide, "TickTutorial", deltaTime);
                remaining -= deltaTime;
            }
        }

        private static string GetStringProperty(object target, string propertyName)
        {
            object value = GetPropertyValue(target, propertyName);
            return value != null ? value.ToString() : null;
        }

        private static int GetIntProperty(object target, string propertyName)
        {
            object value = GetPropertyValue(target, propertyName);
            Assert.IsInstanceOf<int>(value);
            return (int)value;
        }

        private static object GetPropertyValue(object target, string propertyName)
        {
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.IsNotNull(property, $"{target.GetType().Name} should define property {propertyName}.");
            return property.GetValue(target);
        }

        private static void InvokePublic(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(method, $"{target.GetType().Name} should define public method {methodName}.");
            method.Invoke(target, args);
        }

        private static void InvokePrivate(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"{target.GetType().Name} should define {methodName}.");
            method.Invoke(target, args);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"{target.GetType().Name} should define {fieldName}.");
            field.SetValue(target, value);
        }

        private static Type RequireType(string fullName)
        {
            Type type = Type.GetType(fullName);
            if (type != null)
            {
                return type;
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            Assert.Fail($"Missing runtime type {fullName}.");
            return null;
        }

        private readonly struct GuideContext
        {
            private readonly ScriptableObject profile;
            private readonly GameObject ownerObject;
            private readonly GameObject guideObject;

            public GuideContext(
                ScriptableObject profile,
                GameObject ownerObject,
                GameObject guideObject,
                BossBarragePocketReviewOwner pocketOwner,
                Component guide)
            {
                this.profile = profile;
                this.ownerObject = ownerObject;
                this.guideObject = guideObject;
                PocketOwner = pocketOwner;
                Guide = guide;
            }

            public BossBarragePocketReviewOwner PocketOwner { get; }
            public Component Guide { get; }

            public void Destroy()
            {
                if (guideObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(guideObject);
                }

                if (ownerObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(ownerObject);
                }

                if (profile != null)
                {
                    UnityEngine.Object.DestroyImmediate(profile);
                }
            }
        }
    }
}
