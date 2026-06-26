using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    public enum ProxyCombatHudInputKind
    {
        None = 0,
        BasicAttackPressed = 10,
        SignalOrbPressed = 20,
        SignalOrbSequencePressed = 30,
        DodgePressed = 40,
        SignatureSkillPressed = 50,
        SwitchOrQtePressed = 60,
        PartnerSkillPressed = 70,
        ReadAcknowledged = 100
    }

    public enum ProxyCombatHudCompletionKind
    {
        None = 0,
        InputAccepted = 5,
        BasicAttackAccepted = 10,
        SignalOrbPinged = 20,
        ThreePingAccepted = 30,
        DodgeOrMatrixAccepted = 40,
        SignatureSkillCast = 50,
        CharacterSwitchOrQteAccepted = 60,
        PartnerSkillAccepted = 70,
        ScoreMeterVisibleOrChanged = 80,
        ScoreStateChanged = 90,
        DurationOrReadAck = 100,
        DurationOrStateObserved = 110,
        ReadAcknowledged = 120,
        DurationElapsed = 130,
        StateObserved = 140
    }

    public enum ProxyCombatHudInputPolicy
    {
        Default = 0,
        ObserveOnly = 10,
        GateRequestedInput = 20,
        AllowAll = 30
    }

    [Serializable]
    public struct ProxyCombatHudInputEvent
    {
        [SerializeField] private ProxyCombatHudInputKind kind;
        [SerializeField] private int primaryIndex;
        [SerializeField] private int secondaryIndex;
        [SerializeField] private int tertiaryIndex;
        [SerializeField] private int sequenceLength;

        public ProxyCombatHudInputEvent(
            ProxyCombatHudInputKind kind,
            int primaryIndex = -1,
            int secondaryIndex = -1,
            int tertiaryIndex = -1,
            int sequenceLength = 0)
        {
            this.kind = kind;
            this.primaryIndex = primaryIndex;
            this.secondaryIndex = secondaryIndex;
            this.tertiaryIndex = tertiaryIndex;
            this.sequenceLength = Mathf.Clamp(sequenceLength, 0, 3);
        }

        public ProxyCombatHudInputKind Kind => kind;
        public int PrimaryIndex => primaryIndex;
        public int SecondaryIndex => secondaryIndex;
        public int TertiaryIndex => tertiaryIndex;
        public int SequenceLength => sequenceLength;

        public static ProxyCombatHudInputEvent None => new ProxyCombatHudInputEvent(ProxyCombatHudInputKind.None);
        public static ProxyCombatHudInputEvent BasicAttack() => new ProxyCombatHudInputEvent(ProxyCombatHudInputKind.BasicAttackPressed);
        public static ProxyCombatHudInputEvent Dodge() => new ProxyCombatHudInputEvent(ProxyCombatHudInputKind.DodgePressed);
        public static ProxyCombatHudInputEvent SignatureSkill() => new ProxyCombatHudInputEvent(ProxyCombatHudInputKind.SignatureSkillPressed);
        public static ProxyCombatHudInputEvent PartnerSkill() => new ProxyCombatHudInputEvent(ProxyCombatHudInputKind.PartnerSkillPressed);
        public static ProxyCombatHudInputEvent SignalOrb(int index) => new ProxyCombatHudInputEvent(ProxyCombatHudInputKind.SignalOrbPressed, index);
        public static ProxyCombatHudInputEvent SwitchOrQte(int slot) => new ProxyCombatHudInputEvent(ProxyCombatHudInputKind.SwitchOrQtePressed, slot);
        public static ProxyCombatHudInputEvent ReadAck() => new ProxyCombatHudInputEvent(ProxyCombatHudInputKind.ReadAcknowledged);

        public static ProxyCombatHudInputEvent SignalOrbSequence(int first, int second, int third)
        {
            return new ProxyCombatHudInputEvent(ProxyCombatHudInputKind.SignalOrbSequencePressed, first, second, third, 3);
        }

        public bool Matches(ProxyCombatHudInputEvent other)
        {
            if (kind != other.kind)
            {
                return false;
            }

            switch (kind)
            {
                case ProxyCombatHudInputKind.SignalOrbPressed:
                case ProxyCombatHudInputKind.SwitchOrQtePressed:
                    return primaryIndex == other.primaryIndex;
                case ProxyCombatHudInputKind.SignalOrbSequencePressed:
                    return sequenceLength == other.sequenceLength
                        && primaryIndex == other.primaryIndex
                        && secondaryIndex == other.secondaryIndex
                        && tertiaryIndex == other.tertiaryIndex;
                default:
                    return true;
            }
        }
    }

    [Serializable]
    public struct ProxyCombatHudCompletionEvent
    {
        [SerializeField] private ProxyCombatHudCompletionKind kind;
        [SerializeField] private int index;

        public ProxyCombatHudCompletionEvent(ProxyCombatHudCompletionKind kind, int index = -1)
        {
            this.kind = kind;
            this.index = index;
        }

        public ProxyCombatHudCompletionKind Kind => kind;
        public int Index => index;
    }
}
