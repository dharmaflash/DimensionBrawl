using System;

namespace DimensionBrawl.LevelDesign
{
    internal enum TerminalStageRunAdapterRole
    {
        EntryBootstrap = 1,
        FactCollection = 2,
        ResultPresentation = 3
    }

    internal interface ITerminalStageRunAdapterLossOwner
    {
        StageRunAbortReason AdapterLossReason { get; }
    }

    internal static class OneRowStageRunAdapterContract
    {
        internal static bool TryValidateDefinition(
            PlayableStageDefinition definition,
            out StageRunRouteSnapshot snapshot,
            out string error)
        {
            snapshot = null;
            error = string.Empty;
            if (definition == null)
            {
                error = "A one-row stage-run bootstrap requires an authored playable-stage definition.";
                return false;
            }

            if (!StageRunRouteSnapshot.TryCreate(definition, out snapshot, out error))
            {
                return false;
            }

            if (snapshot.SegmentCount != 1
                || snapshot.GetSegmentRoles(0)
                    != (StageRunSegmentRole.Entry | StageRunSegmentRole.Terminal)
                || !snapshot.IsEntrySegment(0)
                || !snapshot.IsTerminalSegment(0))
            {
                error = "A one-row stage-run adapter requires exactly one Entry|Terminal segment.";
                snapshot = null;
                return false;
            }

            if (snapshot.TutorialFactRequirement != StageRunTutorialFactRequirement.None)
            {
                error = "A one-row stage-run adapter cannot fabricate a tutorial fact requirement.";
                snapshot = null;
                return false;
            }

            return true;
        }

        internal static bool TryValidateContext(
            StageRunContext context,
            int sceneHandle,
            out string error)
        {
            error = string.Empty;
            if (context == null
                || context.CurrentSceneHandle != sceneHandle
                || context.CurrentSegmentIndex != 0
                || context.RouteSnapshot == null
                || context.RouteSnapshot.SegmentCount != 1
                || context.CurrentSegmentRoles
                    != (StageRunSegmentRole.Entry | StageRunSegmentRole.Terminal)
                || context.TutorialFactRequirement != StageRunTutorialFactRequirement.None)
            {
                error = "No exact same-scene one-row Entry|Terminal run context is available.";
                return false;
            }

            if (context.PendingHandoffToken != null
                || context.SegmentEntryReceipt != null
                || context.HandoffTerminalReceipt != null)
            {
                error = "A one-row stage-run context cannot contain fabricated handoff evidence.";
                return false;
            }

            return true;
        }

        internal static bool IsFactBindingLifecycle(StageRunLifecycleState state)
        {
            return state == StageRunLifecycleState.StationActive
                || state == StageRunLifecycleState.TerminalFinalizing
                || state == StageRunLifecycleState.TerminalFinalizationOwnersSealed
                || state == StageRunLifecycleState.OutcomeFactsSealed
                || state == StageRunLifecycleState.CommitRequested
                || state == StageRunLifecycleState.CommitRecoveryPending;
        }
    }
}
