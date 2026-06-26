using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    [Serializable]
    public sealed class ProxyCombatHudTutorialStep
    {
        [SerializeField] private bool enabled = true;
        [SerializeField] private string stepId;
        [SerializeField] private string mappingId;
        [SerializeField] private string pgrMaskTarget;
        [SerializeField] private string pgrClickKey;
        [SerializeField, TextArea(1, 3)] private string guideText;
        [SerializeField, Min(0f)] private float durationSeconds;
        [SerializeField] private ProxyCombatHudInputPolicy inputPolicy;
        [SerializeField] private bool completeOnAcceptedInput;

        public ProxyCombatHudTutorialStep(
            string stepId,
            string mappingId,
            string pgrMaskTarget,
            string pgrClickKey,
            string guideText,
            float durationSeconds,
            ProxyCombatHudInputPolicy inputPolicy = ProxyCombatHudInputPolicy.Default,
            bool completeOnAcceptedInput = false)
        {
            enabled = true;
            this.stepId = stepId;
            this.mappingId = mappingId;
            this.pgrMaskTarget = pgrMaskTarget;
            this.pgrClickKey = PgrCombatHudProxyMapping.NormalizeClickKey(pgrClickKey);
            this.guideText = guideText;
            this.durationSeconds = Mathf.Max(0f, durationSeconds);
            this.inputPolicy = inputPolicy;
            this.completeOnAcceptedInput = completeOnAcceptedInput;
        }

        public bool Enabled => enabled;
        public string StepId => stepId;
        public string MappingId => mappingId;
        public string PgrMaskTarget => pgrMaskTarget;
        public string PgrClickKey => PgrCombatHudProxyMapping.NormalizeClickKey(pgrClickKey);
        public string GuideText => guideText;
        public float DurationSeconds => Mathf.Max(0f, durationSeconds);
        public ProxyCombatHudInputPolicy InputPolicy => inputPolicy;
        public bool CompleteOnAcceptedInput => completeOnAcceptedInput;

        public static ProxyCombatHudTutorialStep ForMappingId(
            string mappingId,
            string guideText,
            float durationSeconds = 0f,
            ProxyCombatHudInputPolicy inputPolicy = ProxyCombatHudInputPolicy.Default,
            bool completeOnAcceptedInput = false)
        {
            return new ProxyCombatHudTutorialStep(
                mappingId,
                mappingId,
                string.Empty,
                string.Empty,
                guideText,
                durationSeconds,
                inputPolicy,
                completeOnAcceptedInput);
        }
    }
}
