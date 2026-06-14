namespace DimensionBrawl.LevelDesign
{
    public enum LinearStageTemplateKind
    {
        TutorialRun,
        StandardStoryRun,
        BacklineLesson,
        ElitePressureRun,
        BossHandoffDrill
    }

    public enum LinearStageSegmentKind
    {
        EntryRead,
        BasicPressure,
        BreakGate,
        BacklinePressure,
        PressureRescue,
        Relief,
        BossBreakHandoff,
        FinalStand
    }

    public enum EncounterPocketKind
    {
        Teach,
        Reinforce,
        MixedPressure,
        Spike,
        Relief,
        Handoff
    }

    public enum LinearStageObjectiveKind
    {
        None,
        ReadThreat,
        PunishRecovery,
        BreakGuard,
        PrioritizeBackline,
        SurvivePressure,
        RecoverPosition,
        ReadPhaseHandoff,
        FinalClear
    }

    public enum StageSummonNeed
    {
        None,
        Break,
        Arrow,
        Tank,
        Heal,
        Any
    }
}
