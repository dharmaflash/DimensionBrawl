using System;
using UnityEngine;

namespace DimensionBrawl.UI
{
    public enum BossBarrageLaneReviewTutorialCondition
    {
        None = 0,
        TimeElapsed = 1,
        DodgeStarted = 10,
        BasicDefenseFireUsed = 20,
        ForwardRiskEntered = 30,
        EnergyTierAvailable = 31,
        SummonSlot1Ready = 40,
        CloseThreatDefeated = 50,
        SummonBlockOpportunityOpened = 51,
        SummonSlot1PressureBlocked = 60,
        SummonFollowupWindowOpened = 70,
        Skill1Used = 71,
        Skill1FollowupHit = 72,
        PocketCleared = 90
    }

    [CreateAssetMenu(
        fileName = "DB_BossBarragePocketTutorialProfile",
        menuName = "DimensionBrawl/Combat/Boss Barrage Pocket Tutorial Profile")]
    public sealed class BossBarrageLaneReviewTutorialProfile : ScriptableObject
    {
        [Serializable]
        public sealed class Step
        {
            [SerializeField] private string stepId;
            [TextArea, SerializeField] private string objectiveText;
            [TextArea, SerializeField] private string promptText;
            [SerializeField] private CombatHudActionId focusAction = CombatHudActionId.None;
            [SerializeField] private bool dimUnfocusedActions = true;
            [SerializeField] private BossBarrageLaneReviewTutorialCondition completionCondition =
                BossBarrageLaneReviewTutorialCondition.TimeElapsed;
            [SerializeField, Min(0)] private int requiredTier = 1;
            [SerializeField, Min(0f)] private float requiredMana;
            [SerializeField, Min(0f)] private float minimumSeconds = 0.25f;

            public string StepId => stepId;
            public string ObjectiveText => objectiveText;
            public string PromptText => promptText;
            public CombatHudActionId FocusAction => focusAction;
            public bool DimUnfocusedActions => dimUnfocusedActions;
            public BossBarrageLaneReviewTutorialCondition CompletionCondition => completionCondition;
            public int RequiredTier => Mathf.Max(0, requiredTier);
            public float RequiredMana => Mathf.Max(0f, requiredMana);
            public float MinimumSeconds => Mathf.Max(0f, minimumSeconds);
        }

        [SerializeField] private bool tutorialEnabled = true;
        [TextArea, SerializeField] private string clearObjective = "CLEAR: summon cover created the Skill1 confirm.";
        [TextArea, SerializeField] private string failObjective = "FAIL: HP or route pressure broke before the answer completed.";
        [SerializeField] private Step[] steps = Array.Empty<Step>();

        public bool TutorialEnabled => tutorialEnabled;
        public string ClearObjective => clearObjective;
        public string FailObjective => failObjective;
        public int StepCount => steps != null ? steps.Length : 0;

        public Step GetStep(int index)
        {
            return steps != null && index >= 0 && index < steps.Length ? steps[index] : null;
        }
    }
}
