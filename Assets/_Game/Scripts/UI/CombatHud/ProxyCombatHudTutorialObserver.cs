using System;
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
}
