using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [Serializable]
    public struct PgrCombatHudProxyMapping
    {
        [SerializeField] private string mappingId;
        [SerializeField] private string contractId;
        [SerializeField] private string semanticLabel;
        [SerializeField] private string pgrMaskTarget;
        [SerializeField] private string pgrClickKey;
        [SerializeField] private string proxyHudObject;
        [SerializeField] private ProxyCombatHudInputEvent proxyInputEvent;
        [SerializeField] private ProxyCombatHudCompletionKind proxyCompletionKind;
        [SerializeField] private int completionIndex;
        [SerializeField] private string focusPolicy;
        [SerializeField] private string fallbackPolicy;
        [SerializeField] private string implementationPriority;
        [SerializeField] private int sourceRowCount;
        [SerializeField] private string sampleRecordIds;
        [SerializeField, TextArea(1, 3)] private string sampleTexts;
        [SerializeField, TextArea(1, 3)] private string sourceAnchors;
        [SerializeField, TextArea(1, 2)] private string implementationNote;

        public PgrCombatHudProxyMapping(
            string mappingId,
            string contractId,
            string semanticLabel,
            string pgrMaskTarget,
            string pgrClickKey,
            string proxyHudObject,
            ProxyCombatHudInputEvent proxyInputEvent,
            ProxyCombatHudCompletionKind proxyCompletionKind,
            int completionIndex,
            string focusPolicy,
            string fallbackPolicy,
            string implementationPriority,
            int sourceRowCount,
            string sampleRecordIds,
            string sampleTexts,
            string sourceAnchors,
            string implementationNote)
        {
            this.mappingId = mappingId;
            this.contractId = contractId;
            this.semanticLabel = semanticLabel;
            this.pgrMaskTarget = pgrMaskTarget;
            this.pgrClickKey = NormalizeClickKey(pgrClickKey);
            this.proxyHudObject = proxyHudObject;
            this.proxyInputEvent = proxyInputEvent;
            this.proxyCompletionKind = proxyCompletionKind;
            this.completionIndex = completionIndex;
            this.focusPolicy = focusPolicy;
            this.fallbackPolicy = fallbackPolicy;
            this.implementationPriority = implementationPriority;
            this.sourceRowCount = Mathf.Max(0, sourceRowCount);
            this.sampleRecordIds = sampleRecordIds;
            this.sampleTexts = sampleTexts;
            this.sourceAnchors = sourceAnchors;
            this.implementationNote = implementationNote;
        }

        public string MappingId => mappingId;
        public string ContractId => contractId;
        public string SemanticLabel => semanticLabel;
        public string PgrMaskTarget => pgrMaskTarget;
        public string PgrClickKey => NormalizeClickKey(pgrClickKey);
        public string ProxyHudObject => proxyHudObject;
        public ProxyCombatHudCompletionKind ProxyCompletionKind => proxyCompletionKind;
        public int CompletionIndex => completionIndex;
        public string FocusPolicy => focusPolicy;
        public string FallbackPolicy => fallbackPolicy;
        public string ImplementationPriority => implementationPriority;
        public int SourceRowCount => sourceRowCount;
        public string SampleRecordIds => sampleRecordIds;
        public string SampleTexts => sampleTexts;
        public string SourceAnchors => sourceAnchors;
        public string ImplementationNote => implementationNote;
        public bool HasInput => proxyInputEvent.Kind != ProxyCombatHudInputKind.None;
        public bool IsGroupTarget => proxyInputEvent.Kind == ProxyCombatHudInputKind.SignalOrbSequencePressed;
        public ProxyCombatHudInputEvent ProxyInputEvent => proxyInputEvent;

        public bool MatchesSource(string maskTarget, string clickKey)
        {
            return string.Equals(PgrMaskTarget, (maskTarget ?? string.Empty).Trim(), StringComparison.Ordinal)
                && string.Equals(PgrClickKey, NormalizeClickKey(clickKey), StringComparison.Ordinal);
        }

        public bool AcceptsInput(ProxyCombatHudInputEvent inputEvent)
        {
            return HasInput && proxyInputEvent.Matches(inputEvent);
        }

        public bool MatchesCompletion(ProxyCombatHudCompletionEvent completionEvent)
        {
            if (completionEvent.Kind == ProxyCombatHudCompletionKind.InputAccepted)
            {
                return HasInput;
            }

            if (proxyCompletionKind == ProxyCombatHudCompletionKind.DurationOrReadAck)
            {
                return completionEvent.Kind == ProxyCombatHudCompletionKind.ReadAcknowledged
                    || completionEvent.Kind == ProxyCombatHudCompletionKind.DurationElapsed;
            }

            if (proxyCompletionKind == ProxyCombatHudCompletionKind.DurationOrStateObserved)
            {
                return completionEvent.Kind == ProxyCombatHudCompletionKind.StateObserved
                    || completionEvent.Kind == ProxyCombatHudCompletionKind.DurationElapsed;
            }

            if (proxyCompletionKind != completionEvent.Kind)
            {
                return false;
            }

            switch (proxyCompletionKind)
            {
                case ProxyCombatHudCompletionKind.SignalOrbPinged:
                case ProxyCombatHudCompletionKind.CharacterSwitchOrQteAccepted:
                    return completionIndex < 0 || completionIndex == completionEvent.Index;
                default:
                    return true;
            }
        }

        public static string NormalizeClickKey(string clickKey)
        {
            if (string.IsNullOrWhiteSpace(clickKey))
            {
                return "(none)";
            }

            string trimmed = clickKey.Trim();
            return string.Equals(trimmed, "none", StringComparison.OrdinalIgnoreCase)
                ? "(none)"
                : trimmed;
        }
    }
}
