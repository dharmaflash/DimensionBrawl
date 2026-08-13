using System;
using System.Reflection;
using DimensionBrawl.Combat;
using DimensionBrawl.Presentation;
using DimensionBrawl.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DimensionBrawl.Tests
{
    public sealed class OlympusStationCombatIntroTutorialBridgePlayModeTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void CanonicalStationGuideRetainsPreOptimizationCopyAndStopsAfterEntryPrompts()
        {
            Type bridgeType = RequireType(
                "DimensionBrawl.LevelDesign.OlympusStationCombatIntroTutorialBridge");
            FieldInfo summonGuideLine = bridgeType.GetField(
                "SummonGuideLine",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(summonGuideLine, Is.Not.Null);
            Assert.That(
                summonGuideLine.GetRawConstantValue(),
                Is.EqualTo("코스트 수치를 만족하면 소환수를 소환할 수 있습니다."));

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Game/UI/Transitions/PF_UI_SceneEntryNoticeOverlay.prefab");
            Assert.That(prefab, Is.Not.Null);
            Component bridge = prefab.GetComponent(bridgeType);
            Assert.That(bridge, Is.Not.Null);
            var serializedBridge = new SerializedObject(bridge);
            Assert.That(
                serializedBridge.FindProperty("runCoreLoopCoachAfterRelease").boolValue,
                Is.False,
                "The restored Station presentation ends after its two voiced entry prompts.");
        }

        [Test]
        public void CoreCoachAdvancesOnlyFromAcceptedCombatOutcomes()
        {
            var root = new GameObject("Station Core Loop Coach Outcome Test");
            try
            {
                Type bridgeType = RequireType(
                    "DimensionBrawl.LevelDesign.OlympusStationCombatIntroTutorialBridge");
                OlympusTutorialOverlayPresenter presenter =
                    root.AddComponent<OlympusTutorialOverlayPresenter>();
                Component bridge = root.AddComponent(bridgeType);
                SetPrivateField(bridge, "overlayPresenter", presenter);
                SetPrivateField(bridge, "coreCoachObservedRunningEncounter", true);

                InvokePrivate(bridge, "CaptureRiskBand", SummonEnergyRiskBand.MidCharge);
                Assert.That(
                    CurrentCoreLoopCoachPhase(bridge),
                    Is.EqualTo("Inactive"));

                InvokePrivate(bridge, "CaptureRiskBand", SummonEnergyRiskBand.ForwardRisk);
                Assert.That(
                    CurrentCoreLoopCoachPhase(bridge),
                    Is.EqualTo("HoldFrontline"));
                Assert.That(
                    presenter.CurrentFocusKind,
                    Is.EqualTo(OlympusTutorialOverlayPresenter.FocusKind.RangedAttack));

                InvokePrivate(bridge, "CaptureSummonFollowupHit", 1, 0f);
                Assert.That(
                    CurrentCoreLoopCoachPhase(bridge),
                    Is.EqualTo("HoldFrontline"),
                    "A zero-damage callback is not an accepted combat outcome.");

                InvokePrivate(bridge, "CaptureSummonBlockOpportunity");
                Assert.That(
                    CurrentCoreLoopCoachPhase(bridge),
                    Is.EqualTo("AwaitingSummonBlock"));
                Assert.That(
                    presenter.CurrentFocusKind,
                    Is.EqualTo(OlympusTutorialOverlayPresenter.FocusKind.SummonSlots));

                InvokePrivate(bridge, "CaptureSummonFollowupHit", 1, 0f);
                Assert.That(
                    CurrentCoreLoopCoachPhase(bridge),
                    Is.EqualTo("AwaitingSummonBlock"),
                    "Firing Skill1 is not a completion outcome without confirmed damage.");

                InvokePrivate(bridge, "CaptureSummonFollowupWindow", 1);
                Assert.That(
                    CurrentCoreLoopCoachPhase(bridge),
                    Is.EqualTo("AwaitingSkill1Hit"));
                Assert.That(
                    presenter.CurrentFocusKind,
                    Is.EqualTo(OlympusTutorialOverlayPresenter.FocusKind.Skill1));

                InvokePrivate(bridge, "CaptureSummonFollowupMiss");
                Assert.That(
                    CurrentCoreLoopCoachPhase(bridge),
                    Is.EqualTo("AwaitingSummonBlock"));
                Assert.That(
                    presenter.CurrentFocusKind,
                    Is.EqualTo(OlympusTutorialOverlayPresenter.FocusKind.SummonSlots));

                InvokePrivate(bridge, "CaptureSummonFollowupWindow", 2);
                InvokePrivate(bridge, "CaptureSummonFollowupHit", 2, 0f);
                Assert.That(
                    CurrentCoreLoopCoachPhase(bridge),
                    Is.EqualTo("AwaitingSkill1Hit"),
                    "A zero-damage callback is not a confirmed hit.");

                InvokePrivate(bridge, "CaptureSummonFollowupHit", 2, 25f);
                Assert.That(
                    CurrentCoreLoopCoachPhase(bridge),
                    Is.EqualTo("Completed"));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void LateCanonicalHitLatchesUntilForwardRiskIsObserved()
        {
            var root = new GameObject("Station Core Loop Coach Late Outcome Test");
            try
            {
                Type bridgeType = RequireType(
                    "DimensionBrawl.LevelDesign.OlympusStationCombatIntroTutorialBridge");
                OlympusTutorialOverlayPresenter presenter =
                    root.AddComponent<OlympusTutorialOverlayPresenter>();
                Component bridge = root.AddComponent(bridgeType);
                SetPrivateField(bridge, "overlayPresenter", presenter);
                SetPrivateField(bridge, "coreCoachObservedRunningEncounter", true);

                InvokePrivate(bridge, "CaptureSummonFollowupHit", 3, 40f);

                Assert.That(CurrentCoreLoopCoachPhase(bridge), Is.EqualTo("SeekForwardRisk"));
                Assert.That(
                    presenter.CurrentFocusKind,
                    Is.EqualTo(OlympusTutorialOverlayPresenter.FocusKind.MoveStick));
                Assert.That(GetPrivateField<bool>(bridge, "summonFollowupHitObserved"), Is.True);

                InvokePrivate(bridge, "CaptureRiskBand", SummonEnergyRiskBand.ForwardRisk);

                Assert.That(CurrentCoreLoopCoachPhase(bridge), Is.EqualTo("Completed"));
                Assert.That(
                    presenter.CurrentFocusKind,
                    Is.EqualTo(OlympusTutorialOverlayPresenter.FocusKind.Skill1));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CompletedEncounterSnapshotCatchesUpWithoutWaitingForNewEvents()
        {
            var coachRoot = new GameObject("Station Core Loop Coach Snapshot Test");
            var energyRoot = new GameObject("Station Core Loop Coach Snapshot Energy");
            var encounterRoot = new GameObject("Station Core Loop Coach Snapshot Encounter");
            try
            {
                Type bridgeType = RequireType(
                    "DimensionBrawl.LevelDesign.OlympusStationCombatIntroTutorialBridge");
                OlympusTutorialOverlayPresenter presenter =
                    coachRoot.AddComponent<OlympusTutorialOverlayPresenter>();
                Component bridge = coachRoot.AddComponent(bridgeType);
                SummonEnergyLadder energyLadder = energyRoot.AddComponent<SummonEnergyLadder>();
                BossBarrageEncounterController encounter =
                    encounterRoot.AddComponent<BossBarrageEncounterController>();

                SetPrivateField(bridge, "overlayPresenter", presenter);
                SetPrivateField(bridge, "energyLadder", energyLadder);
                SetPrivateField(bridge, "bossEncounter", encounter);
                SetPrivateField(bridge, "coreCoachObservedRunningEncounter", true);
                SetPrivateField(energyLadder, "currentForwardRisk01", 1f);
                SetPrivateField(encounter, "closeThreatDefeated", true);
                SetPrivateField(encounter, "blockedBossPressureWithSummon", true);
                SetPrivateField(encounter, "skill1FollowupHitConfirmed", true);

                InvokePrivate(bridge, "CaptureCoreLoopSnapshot");

                Assert.That(CurrentCoreLoopCoachPhase(bridge), Is.EqualTo("Completed"));
                Assert.That(GetPrivateField<bool>(bridge, "forwardRiskObserved"), Is.True);
                Assert.That(GetPrivateField<bool>(bridge, "summonBlockOpportunityObserved"), Is.True);
                Assert.That(GetPrivateField<bool>(bridge, "summonFollowupHitObserved"), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(encounterRoot);
                Object.DestroyImmediate(energyRoot);
                Object.DestroyImmediate(coachRoot);
            }
        }

        [Test]
        public void ReleasedCoreCoachNeverReacquiresInputAndDisableCleansSubscriptions()
        {
            var coachRoot = new GameObject("Station Core Loop Coach Ownership Test");
            var energyRoot = new GameObject("Station Core Loop Coach Energy");
            var encounterRoot = new GameObject("Station Core Loop Coach Encounter");
            var actionRoot = new GameObject("Station Core Loop Coach Action", typeof(RectTransform));
            try
            {
                Type bridgeType = RequireType(
                    "DimensionBrawl.LevelDesign.OlympusStationCombatIntroTutorialBridge");
                Type pointerActionType = RequireType(
                    "DimensionBrawl.UI.CombatHudPointerActionInput");
                OlympusTutorialOverlayPresenter presenter =
                    coachRoot.AddComponent<OlympusTutorialOverlayPresenter>();
                Component bridge = coachRoot.AddComponent(bridgeType);
                SummonEnergyLadder energyLadder = energyRoot.AddComponent<SummonEnergyLadder>();
                BossBarrageEncounterController encounter =
                    encounterRoot.AddComponent<BossBarrageEncounterController>();
                Component pointerAction = actionRoot.AddComponent(pointerActionType);
                Array pointerActions = Array.CreateInstance(pointerActionType, 1);
                pointerActions.SetValue(pointerAction, 0);

                SetPrivateField(bridge, "overlayPresenter", presenter);
                SetPrivateField(bridge, "energyLadder", energyLadder);
                SetPrivateField(bridge, "bossEncounter", encounter);
                SetPrivateField(bridge, "combatHudPointerActions", pointerActions);
                SetPrivateField(
                    bridge,
                    "<State>k__BackingField",
                    CombatEntryGuideState.Released);

                InvokePrivate(bridge, "StartCoreLoopCoachAfterRelease");

                Assert.That(GetPublicProperty<bool>(bridge, "IsCoreLoopCoachRunning"), Is.True);
                Assert.That(
                    CurrentCoreLoopCoachPhase(bridge),
                    Is.EqualTo("SeekForwardRisk"));
                Assert.That(GetPrivateField<bool>(bridge, "gameplayInputLocked"), Is.False);
                Assert.That(GetPublicProperty<bool>(pointerAction, "IsInputBlocked"), Is.False);
                Assert.That(GetPrivateField<bool>(bridge, "coreCoachEventsSubscribed"), Is.True);
                Assert.That(presenter.Visible, Is.True);

                ((Behaviour)bridge).enabled = false;

                Assert.That(GetPublicProperty<bool>(bridge, "IsCoreLoopCoachRunning"), Is.False);
                Assert.That(GetPrivateField<bool>(bridge, "coreCoachEventsSubscribed"), Is.False);
                Assert.That(
                    CurrentCoreLoopCoachPhase(bridge),
                    Is.EqualTo("Interrupted"));
                Assert.That(GetPublicProperty<bool>(pointerAction, "IsInputBlocked"), Is.False);
                Assert.That(presenter.Visible, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(actionRoot);
                Object.DestroyImmediate(encounterRoot);
                Object.DestroyImmediate(energyRoot);
                Object.DestroyImmediate(coachRoot);
            }
        }

        private static object InvokePrivate(object target, string methodName, params object[] arguments)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, PrivateInstance);
            Assert.That(method, Is.Not.Null, $"Expected private method '{methodName}'.");
            return method.Invoke(target, arguments);
        }

        private static string CurrentCoreLoopCoachPhase(object bridge)
        {
            object phase = GetPublicProperty<object>(bridge, "CurrentCoreLoopCoachPhase");
            return phase?.ToString() ?? string.Empty;
        }

        private static T GetPublicProperty<T>(object target, string propertyName)
        {
            PropertyInfo property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(property, Is.Not.Null, $"Expected public property '{propertyName}'.");
            return (T)property.GetValue(target);
        }

        private static Type RequireType(string fullName)
        {
            Type type = Type.GetType($"{fullName}, Assembly-CSharp");
            Assert.That(type, Is.Not.Null, $"Missing runtime type '{fullName}'.");
            return type;
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, PrivateInstance);
            Assert.That(field, Is.Not.Null, $"Expected private field '{fieldName}'.");
            return (T)field.GetValue(target);
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, PrivateInstance);
            Assert.That(field, Is.Not.Null, $"Expected private field '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}
