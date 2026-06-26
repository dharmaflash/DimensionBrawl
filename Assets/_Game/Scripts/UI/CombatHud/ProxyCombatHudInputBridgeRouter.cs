using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class ProxyCombatHudInputBridgeRouter : MonoBehaviour
    {
        [Serializable]
        public struct ActionBinding
        {
            [SerializeField] private CombatHudActionId actionId;
            [SerializeField] private ProxyCombatHudInputEvent proxyInputEvent;

            public ActionBinding(CombatHudActionId actionId, ProxyCombatHudInputEvent proxyInputEvent)
            {
                this.actionId = actionId;
                this.proxyInputEvent = proxyInputEvent;
            }

            public CombatHudActionId ActionId => actionId;
            public ProxyCombatHudInputEvent ProxyInputEvent => proxyInputEvent;
        }

        [SerializeField] private ProxyCombatHudTutorialRunner tutorialRunner;
        [SerializeField] private bool useDefaultBindingsWhenEmpty = true;
        [SerializeField] private ActionBinding[] actionBindings = Array.Empty<ActionBinding>();

        private static readonly ActionBinding[] DefaultBindings =
        {
            new ActionBinding(CombatHudActionId.BasicAttack, ProxyCombatHudInputEvent.BasicAttack()),
            new ActionBinding(CombatHudActionId.Dodge, ProxyCombatHudInputEvent.Dodge()),
            new ActionBinding(CombatHudActionId.Skill1, ProxyCombatHudInputEvent.SignatureSkill()),
            new ActionBinding(CombatHudActionId.Ultimate, ProxyCombatHudInputEvent.SignatureSkill())
        };

        public ProxyCombatHudTutorialRunner TutorialRunner => tutorialRunner;
        public ActionBinding[] ActionBindings => actionBindings ?? Array.Empty<ActionBinding>();

        private void Awake()
        {
            if (tutorialRunner == null)
            {
                tutorialRunner = GetComponent<ProxyCombatHudTutorialRunner>();
            }
        }

        public void Configure(ProxyCombatHudTutorialRunner newTutorialRunner, ActionBinding[] newActionBindings)
        {
            tutorialRunner = newTutorialRunner;
            actionBindings = newActionBindings ?? Array.Empty<ActionBinding>();
        }

        public bool TryAcceptAction(CombatHudActionId actionId)
        {
            if (!TryResolveInputEvent(actionId, out ProxyCombatHudInputEvent inputEvent))
            {
                return true;
            }

            return tutorialRunner == null || tutorialRunner.TryAcceptInput(inputEvent);
        }

        public bool TryResolveInputEvent(CombatHudActionId actionId, out ProxyCombatHudInputEvent inputEvent)
        {
            ActionBinding[] configuredBindings = ActionBindings;
            if (TryResolveInputEvent(configuredBindings, actionId, out inputEvent))
            {
                return true;
            }

            if (useDefaultBindingsWhenEmpty && configuredBindings.Length == 0)
            {
                return TryResolveInputEvent(DefaultBindings, actionId, out inputEvent);
            }

            inputEvent = ProxyCombatHudInputEvent.None;
            return false;
        }

        private static bool TryResolveInputEvent(
            ActionBinding[] bindings,
            CombatHudActionId actionId,
            out ProxyCombatHudInputEvent inputEvent)
        {
            for (int i = 0; i < bindings.Length; i++)
            {
                if (bindings[i].ActionId == actionId)
                {
                    inputEvent = bindings[i].ProxyInputEvent;
                    return true;
                }
            }

            inputEvent = ProxyCombatHudInputEvent.None;
            return false;
        }
    }
}
