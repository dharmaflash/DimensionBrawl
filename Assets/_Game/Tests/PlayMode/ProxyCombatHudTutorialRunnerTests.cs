using System.Collections.Generic;
using System.Reflection;
using DimensionBrawl.UI;
using DimensionBrawl.Player;
using NUnit.Framework;
using UnityEngine;

namespace DimensionBrawl.Tests
{
    public sealed class ProxyCombatHudTutorialRunnerTests
    {
        [Test]
        public void DefaultP0MappingsResolvePgrTargetsThroughProxyHudObjects()
        {
            Assert.IsTrue(PgrCombatHudProxyMappingCatalog.TryResolveDefaultP0(
                "AttackButton",
                "15",
                out PgrCombatHudProxyMapping attackMapping));
            Assert.AreEqual("Hud.BasicAttackButton", attackMapping.ProxyHudObject);
            Assert.AreEqual(ProxyCombatHudInputKind.BasicAttackPressed, attackMapping.ProxyInputEvent.Kind);
            Assert.AreEqual(ProxyCombatHudCompletionKind.BasicAttackAccepted, attackMapping.ProxyCompletionKind);

            Assert.IsTrue(PgrCombatHudProxyMappingCatalog.TryResolveDefaultP0(
                "PanelBallBox/TopThreeBalls",
                "1|2|3",
                out PgrCombatHudProxyMapping threePingMapping));
            Assert.AreEqual("Hud.SignalOrbGroup.TopThree", threePingMapping.ProxyHudObject);
            Assert.IsTrue(threePingMapping.IsGroupTarget);
            Assert.AreEqual(3, threePingMapping.ProxyInputEvent.SequenceLength);
            Assert.AreEqual(ProxyCombatHudCompletionKind.ThreePingAccepted, threePingMapping.ProxyCompletionKind);

            Assert.IsTrue(PgrCombatHudProxyMappingCatalog.TryResolveDefaultP0(
                "HpTopBossTemplate/HpTopNormalTemplateList/Endure",
                "(none)",
                out PgrCombatHudProxyMapping bossPoiseMapping));
            Assert.AreEqual("Hud.BossPoiseBar", bossPoiseMapping.ProxyHudObject);
            Assert.IsFalse(bossPoiseMapping.HasInput);
            Assert.AreEqual(ProxyCombatHudCompletionKind.DurationOrReadAck, bossPoiseMapping.ProxyCompletionKind);
        }

        [Test]
        public void RunnerKeepsThreePingAsGroupedHudTargetAndCompletesOnObserver()
        {
            ProxyTutorialHarness harness = CreateHarness("ThreePingRunner");
            try
            {
                RectTransform orb0 = CreateRectTransform("Orb0", harness.Root.transform);
                RectTransform orb1 = CreateRectTransform("Orb1", harness.Root.transform);
                RectTransform orb2 = CreateRectTransform("Orb2", harness.Root.transform);
                harness.Resolver.RegisterTargetGroup("Hud.SignalOrbGroup.TopThree", orb0, orb1, orb2);

                Assert.IsTrue(harness.Runner.BeginMapping("signal_orb_three_ping", "Ping three adjacent orbs."));

                Assert.IsTrue(harness.Runner.IsRunning);
                Assert.AreEqual("signal_orb_three_ping", harness.Runner.ActiveMappingId);
                Assert.AreEqual(ProxyCombatHudInputPolicy.GateRequestedInput, harness.Runner.ActiveInputPolicy);
                Assert.IsTrue(harness.Presenter.Visible);
                Assert.AreEqual(3, harness.Presenter.LastTargetCount);
                Assert.AreEqual("Hud.SignalOrbGroup.TopThree", harness.Presenter.LastProxyHudObject);
                Assert.AreEqual("combat-signal-orb-ping", harness.Presenter.LastCueProfileId);
                Assert.AreEqual("3-PING", harness.Presenter.LastPromptLabel);
                Assert.Greater(harness.Presenter.LastAccentColor.a, 0.8f);
                Assert.IsTrue(harness.Presenter.HasCanvasOverlay);
                Assert.IsTrue(harness.Presenter.RuntimeMaskActive);
                Assert.IsTrue(harness.Presenter.RuntimeFocusFrameActive);
                Assert.IsTrue(harness.Presenter.RuntimeGuideBoxActive);
                Assert.IsFalse(harness.Presenter.RuntimeFallbackPulseActive);
                Assert.AreEqual("Ping three adjacent orbs.", harness.Presenter.RuntimeGuideBodyText);

                Assert.IsFalse(harness.Runner.TryAcceptInput(ProxyCombatHudInputEvent.Dodge()));
                Assert.AreEqual(ProxyCombatHudInputKind.DodgePressed, harness.Runner.LastRejectedInput.Kind);

                Assert.IsTrue(harness.Runner.TryAcceptInput(ProxyCombatHudInputEvent.SignalOrbSequence(0, 1, 2)));
                Assert.IsTrue(harness.Runner.IsRunning, "Accepted input should not finish until combat observer confirms the ping.");

                harness.Observer.NotifyThreePingAccepted();

                Assert.IsFalse(harness.Runner.IsRunning);
                Assert.IsFalse(harness.Presenter.Visible);
                Assert.AreEqual("signal_orb_three_ping", harness.Runner.LastCompletedMappingId);
                Assert.AreEqual(ProxyCombatHudCompletionKind.ThreePingAccepted, harness.Runner.LastCompletionReason);
            }
            finally
            {
                Object.DestroyImmediate(harness.Root);
            }
        }

        [Test]
        public void PresenterUsesCanvasOverlayInsteadOfOnGuiFallback()
        {
            Assert.IsNull(
                typeof(ProxyCombatHudOverlayPresenter).GetMethod(
                    "OnGUI",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));

            ProxyTutorialHarness harness = CreateHarness("CanvasOverlayRunner");
            try
            {
                RectTransform attackButton = CreateRectTransform("AttackButton", harness.Root.transform);
                harness.Resolver.RegisterTargetGroup("Hud.BasicAttackButton", attackButton);

                Assert.IsTrue(harness.Runner.BeginMapping("basic_attack_primary", "Confirm the attack cue."));

                Assert.IsTrue(harness.Presenter.Visible);
                Assert.IsTrue(harness.Presenter.HasCanvasOverlay);
                Assert.IsTrue(harness.Presenter.RuntimeMaskActive);
                Assert.IsTrue(harness.Presenter.RuntimeFocusFrameActive);
                Assert.IsFalse(harness.Presenter.RuntimeFallbackPulseActive);
                Assert.IsTrue(harness.Presenter.RuntimeGuideBoxActive);
                Assert.AreEqual("Confirm the attack cue.", harness.Presenter.RuntimeGuideBodyText);
            }
            finally
            {
                Object.DestroyImmediate(harness.Root);
            }
        }

        [Test]
        public void RunnerSupportsDurationAndReadAckForExplainOnlyTargets()
        {
            ProxyTutorialHarness harness = CreateHarness("ReadAckRunner");
            try
            {
                Assert.IsTrue(harness.Runner.BeginMapping(
                    "boss_poise_endure_bar",
                    "Watch the boss poise bar.",
                    durationSeconds: 0.5f));

                Assert.AreEqual(ProxyCombatHudInputPolicy.ObserveOnly, harness.Runner.ActiveInputPolicy);
                Assert.IsTrue(harness.Runner.TryAcceptInput(ProxyCombatHudInputEvent.BasicAttack()));
                harness.Runner.Tick(0.49f);
                Assert.IsTrue(harness.Runner.IsRunning);
                harness.Runner.Tick(0.02f);
                Assert.IsFalse(harness.Runner.IsRunning);
                Assert.AreEqual(ProxyCombatHudCompletionKind.DurationElapsed, harness.Runner.LastCompletionReason);

                Assert.IsTrue(harness.Runner.BeginMapping(
                    "boss_poise_endure_bar",
                    "Read the boss poise bar.",
                    durationSeconds: 10f));

                Assert.IsTrue(harness.Runner.TryAcceptInput(ProxyCombatHudInputEvent.ReadAck()));

                Assert.IsFalse(harness.Runner.IsRunning);
                Assert.AreEqual(ProxyCombatHudCompletionKind.ReadAcknowledged, harness.Runner.LastCompletionReason);
            }
            finally
            {
                Object.DestroyImmediate(harness.Root);
            }
        }

        [Test]
        public void PresenterResolvesSubcultureGuideCuesFromPgrMappings()
        {
            ProxyTutorialHarness harness = CreateHarness("VisualCueRunner");
            try
            {
                Assert.IsTrue(harness.Runner.BeginMapping("character_switch_slot_1", "Call in the support QTE."));

                Assert.IsTrue(harness.Presenter.Visible);
                Assert.IsTrue(harness.Presenter.LastTextOnlyFallback);
                Assert.AreEqual("combat-character-switch-qte", harness.Presenter.LastCueProfileId);
                Assert.AreEqual("QTE READY", harness.Presenter.LastPromptLabel);
                Assert.Greater(harness.Presenter.LastAccentColor.a, 0.8f);

                Assert.IsTrue(harness.Runner.BeginMapping(
                    "boss_poise_endure_bar",
                    "Watch the boss poise bar.",
                    durationSeconds: 0.5f));

                Assert.AreEqual("combat-boss-hp-poise-rage", harness.Presenter.LastCueProfileId);
                Assert.AreEqual("READ", harness.Presenter.LastPromptLabel);
                Assert.IsTrue(harness.Presenter.LastTextOnlyFallback);
            }
            finally
            {
                Object.DestroyImmediate(harness.Root);
            }
        }

        [Test]
        public void TargetSurfacePublishesCanvasProxyTargetsForScreenRectProviders()
        {
            ProxyTutorialHarness harness = CreateHarness("TargetSurfaceRunner");
            try
            {
                Canvas canvas = harness.Root.GetComponent<Canvas>() ?? harness.Root.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                ProxyScreenRectProvider provider = harness.Root.AddComponent<ProxyScreenRectProvider>();
                provider.SetRect("Hud.PartyPortraitSlots[1]", new Rect(100f, 220f, 96f, 64f));
                ProxyCombatHudTargetSurface surface = harness.Root.AddComponent<ProxyCombatHudTargetSurface>();
                surface.Configure(harness.Resolver, provider, canvas);
                surface.RebuildDefaultTargets();
                surface.SyncTargetRects();

                Assert.IsTrue(harness.Resolver.TryResolve(
                    "Hud.PartyPortraitSlots[1]",
                    out System.Collections.Generic.IReadOnlyList<RectTransform> targets));
                Assert.AreEqual(1, targets.Count);

                Assert.IsTrue(harness.Runner.BeginMapping("character_switch_slot_1", "Call support QTE."));

                Assert.IsTrue(harness.Presenter.Visible);
                Assert.IsFalse(harness.Presenter.LastTextOnlyFallback);
                Assert.AreEqual(1, harness.Presenter.LastTargetCount);
                Assert.AreEqual("QTE READY", harness.Presenter.LastPromptLabel);
            }
            finally
            {
                Object.DestroyImmediate(harness.Root);
            }
        }

        [Test]
        public void RunnerRequiresMatchingSwitchSlotCompletion()
        {
            ProxyTutorialHarness harness = CreateHarness("SwitchSlotRunner");
            try
            {
                Assert.IsTrue(harness.Runner.BeginMapping("character_switch_slot_1", "Switch to slot one."));

                Assert.IsFalse(harness.Runner.TryAcceptInput(ProxyCombatHudInputEvent.SwitchOrQte(2)));
                harness.Observer.NotifyCharacterSwitchOrQteAccepted(2);
                Assert.IsTrue(harness.Runner.IsRunning);

                harness.Observer.NotifyCharacterSwitchOrQteAccepted(1);

                Assert.IsFalse(harness.Runner.IsRunning);
                Assert.AreEqual("character_switch_slot_1", harness.Runner.LastCompletedMappingId);
                Assert.AreEqual(ProxyCombatHudCompletionKind.CharacterSwitchOrQteAccepted, harness.Runner.LastCompletionReason);
            }
            finally
            {
                Object.DestroyImmediate(harness.Root);
            }
        }

        [Test]
        public void InputBridgeRouterUsesDataBindingsBeforeDispatch()
        {
            ProxyTutorialHarness harness = CreateHarness("InputBridgeRunner");
            try
            {
                ProxyCombatHudInputBridgeRouter router = harness.Root.AddComponent<ProxyCombatHudInputBridgeRouter>();
                router.Configure(
                    harness.Runner,
                    new[]
                    {
                        new ProxyCombatHudInputBridgeRouter.ActionBinding(
                            CombatHudActionId.BasicAttack,
                            ProxyCombatHudInputEvent.BasicAttack()),
                        new ProxyCombatHudInputBridgeRouter.ActionBinding(
                            CombatHudActionId.Dodge,
                            ProxyCombatHudInputEvent.Dodge())
                    });

                Assert.IsTrue(harness.Runner.BeginMapping("basic_attack_primary", "Tap attack."));

                Assert.IsFalse(router.TryAcceptAction(CombatHudActionId.Dodge));
                Assert.IsTrue(harness.Runner.IsRunning);

                Assert.IsTrue(router.TryAcceptAction(CombatHudActionId.BasicAttack));
                Assert.IsTrue(harness.Runner.IsRunning);

                harness.Observer.NotifyBasicAttackAccepted();

                Assert.IsFalse(harness.Runner.IsRunning);
                Assert.AreEqual(ProxyCombatHudCompletionKind.BasicAttackAccepted, harness.Runner.LastCompletionReason);
            }
            finally
            {
                Object.DestroyImmediate(harness.Root);
            }
        }

        [Test]
        public void InputBridgeRouterDefaultsTranslateSummonButtonsToLocalQteGrammar()
        {
            ProxyTutorialHarness harness = CreateHarness("SummonInputBridgeRunner");
            try
            {
                ProxyCombatHudInputBridgeRouter router = harness.Root.AddComponent<ProxyCombatHudInputBridgeRouter>();
                router.Configure(harness.Runner, null);

                Assert.IsTrue(router.TryResolveInputEvent(
                    CombatHudActionId.SummonSlot1,
                    out ProxyCombatHudInputEvent primarySummonEvent));
                Assert.AreEqual(ProxyCombatHudInputKind.PartnerSkillPressed, primarySummonEvent.Kind);

                Assert.IsTrue(router.TryResolveInputEvent(
                    CombatHudActionId.SummonSlot2,
                    out ProxyCombatHudInputEvent slot2Event));
                Assert.AreEqual(ProxyCombatHudInputKind.SwitchOrQtePressed, slot2Event.Kind);
                Assert.AreEqual(1, slot2Event.PrimaryIndex);

                Assert.IsTrue(router.TryResolveInputEvent(
                    CombatHudActionId.SummonSlot3,
                    out ProxyCombatHudInputEvent slot3Event));
                Assert.AreEqual(ProxyCombatHudInputKind.SwitchOrQtePressed, slot3Event.Kind);
                Assert.AreEqual(2, slot3Event.PrimaryIndex);
            }
            finally
            {
                Object.DestroyImmediate(harness.Root);
            }
        }

        [Test]
        public void PlayerActionObserverBridgeCompletesCoreCombatHudSteps()
        {
            ProxyTutorialHarness harness = CreateHarness("PlayerActionObserverRunner");
            try
            {
                ProxyCombatHudPlayerActionObserverBridge bridge =
                    harness.Root.AddComponent<ProxyCombatHudPlayerActionObserverBridge>();
                bridge.Configure(harness.Observer, null, null, null);

                Assert.IsTrue(harness.Runner.BeginMapping("basic_attack_primary", "Fire once."));
                bridge.NotifyRangedBasicFireStarted();
                Assert.IsFalse(harness.Runner.IsRunning);
                Assert.AreEqual("basic_attack_primary", harness.Runner.LastCompletedMappingId);
                Assert.AreEqual(ProxyCombatHudCompletionKind.BasicAttackAccepted, harness.Runner.LastCompletionReason);

                Assert.IsTrue(harness.Runner.BeginMapping("dodge_matrix_primary", "Dodge once."));
                bridge.NotifyDodgeStarted();
                Assert.IsFalse(harness.Runner.IsRunning);
                Assert.AreEqual("dodge_matrix_primary", harness.Runner.LastCompletedMappingId);
                Assert.AreEqual(ProxyCombatHudCompletionKind.DodgeOrMatrixAccepted, harness.Runner.LastCompletionReason);

                Assert.IsTrue(harness.Runner.BeginMapping("signature_skill_primary", "Cast skill."));
                bridge.NotifySkill1Used(2);
                Assert.IsFalse(harness.Runner.IsRunning);
                Assert.AreEqual("signature_skill_primary", harness.Runner.LastCompletedMappingId);
                Assert.AreEqual(ProxyCombatHudCompletionKind.SignatureSkillCast, harness.Runner.LastCompletionReason);
            }
            finally
            {
                Object.DestroyImmediate(harness.Root);
            }
        }

        [Test]
        public void SummonQteObserverBridgeCompletesPortraitQteAndPartnerSkillSteps()
        {
            ProxyTutorialHarness harness = CreateHarness("SummonQteObserverRunner");
            try
            {
                PlayerSummonSlot1Action summonSlot1Action = harness.Root.AddComponent<PlayerSummonSlot1Action>();
                PlayerSupportSummonSlotAction summonSlot2Action = harness.Root.AddComponent<PlayerSupportSummonSlotAction>();
                PlayerSupportSummonSlotAction summonSlot3Action = harness.Root.AddComponent<PlayerSupportSummonSlotAction>();
                ProxyCombatHudSummonQteObserverBridge bridge =
                    harness.Root.AddComponent<ProxyCombatHudSummonQteObserverBridge>();
                bridge.Configure(
                    harness.Observer,
                    summonSlot1Action,
                    summonSlot2Action,
                    summonSlot3Action);

                Assert.IsTrue(harness.Runner.BeginMapping("character_switch_slot_1", "Call the first support QTE."));

                bridge.NotifySupportSummonUsed(summonSlot3Action, 1);
                Assert.IsTrue(harness.Runner.IsRunning);

                bridge.NotifySupportSummonUsed(summonSlot2Action, 2);

                Assert.IsFalse(harness.Runner.IsRunning);
                Assert.AreEqual("character_switch_slot_1", harness.Runner.LastCompletedMappingId);
                Assert.AreEqual(ProxyCombatHudCompletionKind.CharacterSwitchOrQteAccepted, harness.Runner.LastCompletionReason);
                Assert.AreEqual(1, bridge.LastCompletionIndex);
                Assert.AreEqual(2, bridge.LastReportedTier);

                Assert.IsTrue(harness.Runner.BeginMapping("partner_skill_button", "Call primary support."));

                bridge.NotifyPrimarySummonUsed(3);

                Assert.IsFalse(harness.Runner.IsRunning);
                Assert.AreEqual("partner_skill_button", harness.Runner.LastCompletedMappingId);
                Assert.AreEqual(ProxyCombatHudCompletionKind.PartnerSkillAccepted, harness.Runner.LastCompletionReason);
                Assert.AreEqual(ProxyCombatHudCompletionKind.PartnerSkillAccepted, bridge.LastCompletionKind);
                Assert.AreEqual(3, bridge.LastReportedTier);
            }
            finally
            {
                Object.DestroyImmediate(harness.Root);
            }
        }

        private static ProxyTutorialHarness CreateHarness(string name)
        {
            GameObject root = new GameObject(name);
            ProxyCombatHudTargetResolver resolver = root.AddComponent<ProxyCombatHudTargetResolver>();
            ProxyCombatHudOverlayPresenter presenter = root.AddComponent<ProxyCombatHudOverlayPresenter>();
            ProxyCombatHudTutorialObserver observer = root.AddComponent<ProxyCombatHudTutorialObserver>();
            ProxyCombatHudTutorialRunner runner = root.AddComponent<ProxyCombatHudTutorialRunner>();
            runner.Configure(null, resolver, presenter, observer);

            return new ProxyTutorialHarness(root, resolver, presenter, observer, runner);
        }

        private static RectTransform CreateRectTransform(string name, Transform parent)
        {
            GameObject targetObject = new GameObject(name);
            targetObject.transform.SetParent(parent, worldPositionStays: false);
            RectTransform rectTransform = targetObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(64f, 64f);
            return rectTransform;
        }

        private sealed class ProxyScreenRectProvider : MonoBehaviour, IProxyCombatHudScreenRectProvider
        {
            private readonly Dictionary<string, Rect> rects = new Dictionary<string, Rect>();

            public void SetRect(string proxyHudObject, Rect rect)
            {
                rects[proxyHudObject] = rect;
            }

            public bool TryGetProxyHudScreenRect(string proxyHudObject, out Rect screenRect)
            {
                return rects.TryGetValue(proxyHudObject, out screenRect);
            }
        }

        private sealed class ProxyTutorialHarness
        {
            public ProxyTutorialHarness(
                GameObject root,
                ProxyCombatHudTargetResolver resolver,
                ProxyCombatHudOverlayPresenter presenter,
                ProxyCombatHudTutorialObserver observer,
                ProxyCombatHudTutorialRunner runner)
            {
                Root = root;
                Resolver = resolver;
                Presenter = presenter;
                Observer = observer;
                Runner = runner;
            }

            public GameObject Root { get; }
            public ProxyCombatHudTargetResolver Resolver { get; }
            public ProxyCombatHudOverlayPresenter Presenter { get; }
            public ProxyCombatHudTutorialObserver Observer { get; }
            public ProxyCombatHudTutorialRunner Runner { get; }
        }
    }
}
