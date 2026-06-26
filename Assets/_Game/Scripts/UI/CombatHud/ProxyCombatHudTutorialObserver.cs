using System;
using DimensionBrawl.Player;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [DisallowMultipleComponent]
    public sealed class ProxyCombatHudTutorialObserver : MonoBehaviour
    {
        public event Action<ProxyCombatHudCompletionEvent> CompletionObserved;

        public void NotifyBasicAttackAccepted()
        {
            Raise(ProxyCombatHudCompletionKind.BasicAttackAccepted);
        }

        public void NotifySignalOrbPinged(int index)
        {
            Raise(ProxyCombatHudCompletionKind.SignalOrbPinged, index);
        }

        public void NotifyThreePingAccepted()
        {
            Raise(ProxyCombatHudCompletionKind.ThreePingAccepted);
        }

        public void NotifyDodgeOrMatrixAccepted()
        {
            Raise(ProxyCombatHudCompletionKind.DodgeOrMatrixAccepted);
        }

        public void NotifySignatureSkillCast()
        {
            Raise(ProxyCombatHudCompletionKind.SignatureSkillCast);
        }

        public void NotifyCharacterSwitchOrQteAccepted(int slot)
        {
            Raise(ProxyCombatHudCompletionKind.CharacterSwitchOrQteAccepted, slot);
        }

        public void NotifyPartnerSkillAccepted()
        {
            Raise(ProxyCombatHudCompletionKind.PartnerSkillAccepted);
        }

        public void NotifyReadAcknowledged()
        {
            Raise(ProxyCombatHudCompletionKind.ReadAcknowledged);
        }

        public void NotifyStateObserved()
        {
            Raise(ProxyCombatHudCompletionKind.StateObserved);
        }

        public void NotifyScoreMeterVisibleOrChanged()
        {
            Raise(ProxyCombatHudCompletionKind.ScoreMeterVisibleOrChanged);
        }

        private void Raise(ProxyCombatHudCompletionKind kind, int index = -1)
        {
            CompletionObserved?.Invoke(new ProxyCombatHudCompletionEvent(kind, index));
        }
    }

    [DisallowMultipleComponent]
    public sealed class ProxyCombatHudSummonQteObserverBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ProxyCombatHudTutorialObserver tutorialObserver;
        [SerializeField] private PlayerSummonSlot1Action summonSlot1Action;
        [SerializeField] private PlayerSupportSummonSlotAction summonSlot2Action;
        [SerializeField] private PlayerSupportSummonSlotAction summonSlot3Action;

        [Header("Local QTE Translation")]
        [SerializeField] private bool primarySummonReportsPartnerSkill = true;
        [SerializeField, Min(0)] private int summonSlot2QteIndex = 1;
        [SerializeField, Min(0)] private int summonSlot3QteIndex = 2;

        private bool subscribed;
        private ProxyCombatHudCompletionKind lastCompletionKind;
        private int lastCompletionIndex = -1;
        private int lastReportedTier;

        public ProxyCombatHudTutorialObserver TutorialObserver => tutorialObserver;
        public PlayerSummonSlot1Action SummonSlot1Action => summonSlot1Action;
        public PlayerSupportSummonSlotAction SummonSlot2Action => summonSlot2Action;
        public PlayerSupportSummonSlotAction SummonSlot3Action => summonSlot3Action;
        public bool PrimarySummonReportsPartnerSkill => primarySummonReportsPartnerSkill;
        public int SummonSlot2QteIndex => summonSlot2QteIndex;
        public int SummonSlot3QteIndex => summonSlot3QteIndex;
        public ProxyCombatHudCompletionKind LastCompletionKind => lastCompletionKind;
        public int LastCompletionIndex => lastCompletionIndex;
        public int LastReportedTier => lastReportedTier;

        private void Awake()
        {
            tutorialObserver ??= GetComponent<ProxyCombatHudTutorialObserver>();
            summonSlot1Action ??= GetComponent<PlayerSummonSlot1Action>();
            ResolveSupportSummonSlotsIfMissing();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Configure(
            ProxyCombatHudTutorialObserver newTutorialObserver,
            PlayerSummonSlot1Action newSummonSlot1Action,
            PlayerSupportSummonSlotAction newSummonSlot2Action,
            PlayerSupportSummonSlotAction newSummonSlot3Action,
            bool newPrimarySummonReportsPartnerSkill = true,
            int newSummonSlot2QteIndex = 1,
            int newSummonSlot3QteIndex = 2)
        {
            Unsubscribe();
            tutorialObserver = newTutorialObserver;
            summonSlot1Action = newSummonSlot1Action;
            summonSlot2Action = newSummonSlot2Action;
            summonSlot3Action = newSummonSlot3Action;
            primarySummonReportsPartnerSkill = newPrimarySummonReportsPartnerSkill;
            summonSlot2QteIndex = Mathf.Max(0, newSummonSlot2QteIndex);
            summonSlot3QteIndex = Mathf.Max(0, newSummonSlot3QteIndex);
            Subscribe();
        }

        public void NotifyPrimarySummonUsed(int tier)
        {
            if (primarySummonReportsPartnerSkill)
            {
                ReportPartnerSkillAccepted(tier);
            }
            else
            {
                ReportQteAccepted(0, tier);
            }
        }

        public void NotifySupportSummonUsed(PlayerSupportSummonSlotAction action, int tier)
        {
            if (action == null)
            {
                return;
            }

            if (action == summonSlot2Action)
            {
                ReportQteAccepted(summonSlot2QteIndex, tier);
                return;
            }

            if (action == summonSlot3Action)
            {
                ReportQteAccepted(summonSlot3QteIndex, tier);
            }
        }

        private void Subscribe()
        {
            if (subscribed)
            {
                return;
            }

            if (summonSlot1Action != null)
            {
                summonSlot1Action.SummonSlot1Used += NotifyPrimarySummonUsed;
            }

            if (summonSlot2Action != null)
            {
                summonSlot2Action.SummonUsed += NotifySupportSummonUsed;
            }

            if (summonSlot3Action != null)
            {
                summonSlot3Action.SummonUsed += NotifySupportSummonUsed;
            }

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            if (summonSlot1Action != null)
            {
                summonSlot1Action.SummonSlot1Used -= NotifyPrimarySummonUsed;
            }

            if (summonSlot2Action != null)
            {
                summonSlot2Action.SummonUsed -= NotifySupportSummonUsed;
            }

            if (summonSlot3Action != null)
            {
                summonSlot3Action.SummonUsed -= NotifySupportSummonUsed;
            }

            subscribed = false;
        }

        private void ReportPartnerSkillAccepted(int tier)
        {
            lastCompletionKind = ProxyCombatHudCompletionKind.PartnerSkillAccepted;
            lastCompletionIndex = -1;
            lastReportedTier = Mathf.Max(0, tier);
            tutorialObserver?.NotifyPartnerSkillAccepted();
        }

        private void ReportQteAccepted(int index, int tier)
        {
            lastCompletionKind = ProxyCombatHudCompletionKind.CharacterSwitchOrQteAccepted;
            lastCompletionIndex = Mathf.Max(0, index);
            lastReportedTier = Mathf.Max(0, tier);
            tutorialObserver?.NotifyCharacterSwitchOrQteAccepted(lastCompletionIndex);
        }

        private void ResolveSupportSummonSlotsIfMissing()
        {
            if (summonSlot2Action != null && summonSlot3Action != null)
            {
                return;
            }

            PlayerSupportSummonSlotAction[] actions = GetComponents<PlayerSupportSummonSlotAction>();
            for (int i = 0; i < actions.Length; i++)
            {
                PlayerSupportSummonSlotAction action = actions[i];
                if (action == null)
                {
                    continue;
                }

                if (summonSlot2Action == null && string.Equals(action.SlotActionName, "SummonSlot2", StringComparison.Ordinal))
                {
                    summonSlot2Action = action;
                    continue;
                }

                if (summonSlot3Action == null && string.Equals(action.SlotActionName, "SummonSlot3", StringComparison.Ordinal))
                {
                    summonSlot3Action = action;
                }
            }
        }
    }
}
