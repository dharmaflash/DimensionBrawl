using System;
using UnityEngine;
using UnityEngine.Events;

namespace DimensionBrawl.UI
{
    [Serializable]
    public sealed class CombatHudActionEvent : UnityEvent<CombatHudActionId>
    {
    }

    [DisallowMultipleComponent]
    public sealed class CombatHudInputBridge : MonoBehaviour
    {
        [SerializeField] private CombatHudPresenter presenter;
        [SerializeField] private ProxyCombatHudInputBridgeRouter tutorialInputRouter;
        [SerializeField] private CombatHudActionEvent actionRequested = new CombatHudActionEvent();

        public event Action<CombatHudActionId> ActionRequested;

        private void Awake()
        {
            if (tutorialInputRouter == null)
            {
                tutorialInputRouter = GetComponent<ProxyCombatHudInputBridgeRouter>();
            }
        }

        public void RequestBasicAttack()
        {
            RequestAction(CombatHudActionId.BasicAttack);
        }

        public void RequestDodge()
        {
            RequestAction(CombatHudActionId.Dodge);
        }

        public void RequestSkill1()
        {
            RequestAction(CombatHudActionId.Skill1);
        }

        public void RequestUltimate()
        {
            RequestAction(CombatHudActionId.Ultimate);
        }

        public void RequestSummonSlot1()
        {
            RequestAction(CombatHudActionId.SummonSlot1);
        }

        public void RequestSummonSlot2()
        {
            RequestAction(CombatHudActionId.SummonSlot2);
        }

        public void RequestSummonSlot3()
        {
            RequestAction(CombatHudActionId.SummonSlot3);
        }

        public void RequestPause()
        {
            RequestAction(CombatHudActionId.Pause);
        }

        public bool RequestAction(CombatHudActionId actionId)
        {
            if (tutorialInputRouter != null && !tutorialInputRouter.TryAcceptAction(actionId))
            {
                return false;
            }

            ActionRequested?.Invoke(actionId);
            actionRequested.Invoke(actionId);

            if (presenter != null)
            {
                presenter.SetActionFeedback(actionId);
            }

            return true;
        }
    }
}
