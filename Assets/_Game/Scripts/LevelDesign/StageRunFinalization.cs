using System;
using System.Globalization;
using System.Text;
using DimensionBrawl.Combat;
using static DimensionBrawl.LevelDesign.StageRunFinalizationDigest;

namespace DimensionBrawl.LevelDesign
{
    public enum StageTerminalResolvedCandidate
    {
        ClearCandidate = 1,
        FailCandidate = 2
    }

    public enum TerminalFinalizationLatchWinner
    {
        TerminalWon = 1
    }

    public enum StageTerminalFinalizationContext
    {
        NonCourseStationTerminal = 1,
        CourseChallengeTerminal = 2
    }

    public enum TerminalFinalizationOwnerKind
    {
        TutorialLesson = 1,
        TutorialCourse = 2,
        EncounterExecution = 3,
        Presentation = 4
    }

    public enum TerminalFinalizationOwnerDisposition
    {
        Succeeded = 1,
        NotAdmitted = 2,
        NotApplicable = 3
    }

    public sealed class StageRootResolutionTokenRecord
    {
        internal StageRootResolutionTokenRecord(
            StageRunIdentity identity,
            EncounterTerminalEpochEvidence evidence)
        {
            TokenId = evidence.TokenId;
            RunId = identity.RunId;
            PlayableStageId = identity.PlayableStageId;
            RouteRevision = identity.RouteRevision;
            RouteSnapshotDigest = identity.RouteSnapshotDigest;
            CoordinatorRunGeneration = evidence.Resolution.RunGeneration;
            RootAdmissionSequence = evidence.Resolution.RootAdmissionSequence;
            TerminalEpoch = evidence.Resolution.Epoch;
            TokenOpenedSequence = evidence.TokenOpenedSequence;
            CanonicalDigest = ComputeCanonicalDigest();
            EnvelopeChecksum = ComputeEnvelopeChecksum("root-resolution-token", CanonicalDigest);
        }

        public string TokenId { get; }
        public string RunId { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string RouteSnapshotDigest { get; }
        public long CoordinatorRunGeneration { get; }
        public long RootAdmissionSequence { get; }
        public long TerminalEpoch { get; }
        public long TokenOpenedSequence { get; }
        public string CanonicalDigest { get; }
        public string EnvelopeChecksum { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(768);
            StageCanonicalDigest.Append(builder, "token.id", TokenId);
            StageCanonicalDigest.Append(builder, "token.runId", RunId);
            StageCanonicalDigest.Append(builder, "token.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "token.routeRevision", RouteRevision);
            StageCanonicalDigest.Append(builder, "token.routeDigest", RouteSnapshotDigest);
            StageCanonicalDigest.Append(builder, "token.coordinatorRunGeneration", CoordinatorRunGeneration);
            StageCanonicalDigest.Append(builder, "token.rootAdmissionSequence", RootAdmissionSequence);
            StageCanonicalDigest.Append(builder, "token.terminalEpoch", TerminalEpoch);
            StageCanonicalDigest.Append(builder, "token.openedSequence", TokenOpenedSequence);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageTerminalSubjectFinalSnapshot
    {
        internal StageTerminalSubjectFinalSnapshot(
            StageRunIdentity identity,
            StageRootResolutionTokenRecord token,
            EncounterTerminalSubjectSnapshotEvidence evidence)
        {
            SnapshotId = $"{identity.RunId}:terminal-subject:{evidence.SubjectRole}:{evidence.SnapshotSequence}";
            RunId = identity.RunId;
            PlayableStageId = identity.PlayableStageId;
            RouteRevision = identity.RouteRevision;
            RouteSnapshotDigest = identity.RouteSnapshotDigest;
            RootAdmissionSequence = token.RootAdmissionSequence;
            TerminalEpoch = token.TerminalEpoch;
            TokenId = token.TokenId;
            TokenDigest = token.CanonicalDigest;
            SubjectRole = evidence.SubjectRole;
            SubjectBindingId = evidence.SubjectBindingId;
            BindingGeneration = evidence.BindingGeneration;
            CurrentHealth = evidence.CurrentHealth;
            MaxHealth = evidence.MaxHealth;
            SubjectState = evidence.SubjectState;
            TerminalCandidate = evidence.TerminalCandidate;
            AcceptedCandidateSequence = evidence.AcceptedCandidateSequence;
            SnapshotSequence = evidence.SnapshotSequence;
            SourceEvidenceDigest = evidence.CanonicalDigest;
            CanonicalDigest = ComputeCanonicalDigest();
            EnvelopeChecksum = ComputeEnvelopeChecksum("terminal-subject-final-snapshot", CanonicalDigest);
        }

        public string SnapshotId { get; }
        public string RunId { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string RouteSnapshotDigest { get; }
        public long RootAdmissionSequence { get; }
        public long TerminalEpoch { get; }
        public string TokenId { get; }
        public string TokenDigest { get; }
        public EncounterTerminalSubjectRole SubjectRole { get; }
        public string SubjectBindingId { get; }
        public long BindingGeneration { get; }
        public float CurrentHealth { get; }
        public float MaxHealth { get; }
        public EncounterTerminalSubjectState SubjectState { get; }
        public EncounterTerminalCandidateKind? TerminalCandidate { get; }
        public long AcceptedCandidateSequence { get; }
        public long SnapshotSequence { get; }
        public string SourceEvidenceDigest { get; }
        public string CanonicalDigest { get; }
        public string EnvelopeChecksum { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(1024);
            StageCanonicalDigest.Append(builder, "snapshot.id", SnapshotId);
            StageCanonicalDigest.Append(builder, "snapshot.runId", RunId);
            StageCanonicalDigest.Append(builder, "snapshot.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "snapshot.routeRevision", RouteRevision);
            StageCanonicalDigest.Append(builder, "snapshot.routeDigest", RouteSnapshotDigest);
            StageCanonicalDigest.Append(builder, "snapshot.rootAdmissionSequence", RootAdmissionSequence);
            StageCanonicalDigest.Append(builder, "snapshot.terminalEpoch", TerminalEpoch);
            StageCanonicalDigest.Append(builder, "snapshot.tokenId", TokenId);
            StageCanonicalDigest.Append(builder, "snapshot.tokenDigest", TokenDigest);
            StageCanonicalDigest.Append(builder, "snapshot.subjectRole", (int)SubjectRole);
            StageCanonicalDigest.Append(builder, "snapshot.subjectBindingId", SubjectBindingId);
            StageCanonicalDigest.Append(builder, "snapshot.bindingGeneration", BindingGeneration);
            AppendFloat(builder, "snapshot.currentHealth", CurrentHealth);
            AppendFloat(builder, "snapshot.maxHealth", MaxHealth);
            StageCanonicalDigest.Append(builder, "snapshot.subjectState", (int)SubjectState);
            StageCanonicalDigest.Append(builder, "snapshot.candidatePresent", TerminalCandidate.HasValue);
            StageCanonicalDigest.Append(
                builder,
                "snapshot.candidateKind",
                TerminalCandidate.HasValue ? (int)TerminalCandidate.Value : 0);
            StageCanonicalDigest.Append(
                builder,
                "snapshot.acceptedCandidateSequence",
                AcceptedCandidateSequence);
            StageCanonicalDigest.Append(builder, "snapshot.sequence", SnapshotSequence);
            StageCanonicalDigest.Append(builder, "snapshot.sourceEvidenceDigest", SourceEvidenceDigest);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageTerminalCandidateCoverageRow
    {
        internal StageTerminalCandidateCoverageRow(
            StageRootResolutionTokenRecord token,
            EncounterTerminalCandidateEvidence evidence)
        {
            IntraRootSequence = evidence.IntraRootSequence;
            CauseIdentity = evidence.CauseIdentity;
            SubjectRole = evidence.SubjectRole;
            CandidateKind = evidence.CandidateKind;
            TokenId = token.TokenId;
            TokenDigest = token.CanonicalDigest;
            ObservedCurrentHealth = evidence.ObservedCurrentHealth;
            ObservedMaxHealth = evidence.ObservedMaxHealth;
            ObservedState = evidence.ObservedState;
            ObservationSequence = evidence.ObservationSequence;
            Agreement = evidence.Agreement;
            SourceEvidenceDigest = evidence.CanonicalDigest;
            CanonicalDigest = ComputeCanonicalDigest();
        }

        public long IntraRootSequence { get; }
        public string CauseIdentity { get; }
        public EncounterTerminalSubjectRole SubjectRole { get; }
        public EncounterTerminalCandidateKind CandidateKind { get; }
        public string TokenId { get; }
        public string TokenDigest { get; }
        public float ObservedCurrentHealth { get; }
        public float ObservedMaxHealth { get; }
        public EncounterTerminalSubjectState ObservedState { get; }
        public long ObservationSequence { get; }
        public EncounterTerminalCandidateAgreementDisposition Agreement { get; }
        public string SourceEvidenceDigest { get; }
        public string CanonicalDigest { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(768);
            StageCanonicalDigest.Append(builder, "candidate.intraRootSequence", IntraRootSequence);
            StageCanonicalDigest.Append(builder, "candidate.causeIdentity", CauseIdentity);
            StageCanonicalDigest.Append(builder, "candidate.subjectRole", (int)SubjectRole);
            StageCanonicalDigest.Append(builder, "candidate.kind", (int)CandidateKind);
            StageCanonicalDigest.Append(builder, "candidate.tokenId", TokenId);
            StageCanonicalDigest.Append(builder, "candidate.tokenDigest", TokenDigest);
            AppendFloat(builder, "candidate.observedCurrentHealth", ObservedCurrentHealth);
            AppendFloat(builder, "candidate.observedMaxHealth", ObservedMaxHealth);
            StageCanonicalDigest.Append(builder, "candidate.observedState", (int)ObservedState);
            StageCanonicalDigest.Append(builder, "candidate.observationSequence", ObservationSequence);
            StageCanonicalDigest.Append(builder, "candidate.agreement", (int)Agreement);
            StageCanonicalDigest.Append(builder, "candidate.sourceEvidenceDigest", SourceEvidenceDigest);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class StageDiscardedPendingAdmissionCoverageRow
    {
        internal StageDiscardedPendingAdmissionCoverageRow(
            EncounterTerminalDiscardedAdmissionEvidence evidence)
        {
            RootAdmissionSequence = evidence.RootAdmissionSequence;
            CauseIdentity = evidence.CauseIdentity;
            NoTokenIssued = evidence.NoTokenIssued;
            Disposition = evidence.Disposition;
            SourceEvidenceDigest = evidence.CanonicalDigest;
            CanonicalDigest = ComputeCanonicalDigest();
        }

        public long RootAdmissionSequence { get; }
        public string CauseIdentity { get; }
        public bool NoTokenIssued { get; }
        public EncounterTerminalPendingAdmissionDisposition Disposition { get; }
        public string SourceEvidenceDigest { get; }
        public string CanonicalDigest { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(384);
            StageCanonicalDigest.Append(builder, "discarded.rootAdmissionSequence", RootAdmissionSequence);
            StageCanonicalDigest.Append(builder, "discarded.causeIdentity", CauseIdentity);
            StageCanonicalDigest.Append(builder, "discarded.noTokenIssued", NoTokenIssued);
            StageCanonicalDigest.Append(builder, "discarded.disposition", (int)Disposition);
            StageCanonicalDigest.Append(builder, "discarded.sourceEvidenceDigest", SourceEvidenceDigest);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class TerminalEpochClosureRecord
    {
        private readonly StageTerminalSubjectFinalSnapshot[] subjectSnapshots;
        private readonly StageTerminalCandidateCoverageRow[] candidateCoverage;
        private readonly StageDiscardedPendingAdmissionCoverageRow[] discardedAdmissions;

        private TerminalEpochClosureRecord(
            StageRunIdentity identity,
            StageRunTerminalPolicySnapshot policy,
            EncounterTerminalEpochEvidence evidence)
        {
            TerminalEpochClosureRecordId =
                $"{identity.RunId}:terminal-epoch-closure:{evidence.Resolution.RootAdmissionSequence}:{evidence.Resolution.Epoch}";
            RunId = identity.RunId;
            PlayableStageId = identity.PlayableStageId;
            RouteRevision = identity.RouteRevision;
            RouteSnapshotDigest = identity.RouteSnapshotDigest;
            CoordinatorRunGeneration = evidence.Resolution.RunGeneration;
            RootAdmissionSequence = evidence.Resolution.RootAdmissionSequence;
            TerminalEpoch = evidence.Resolution.Epoch;
            ActiveToken = new StageRootResolutionTokenRecord(identity, evidence);
            subjectSnapshots = new StageTerminalSubjectFinalSnapshot[evidence.SubjectSnapshotCount];
            for (int i = 0; i < subjectSnapshots.Length; i++)
            {
                subjectSnapshots[i] = new StageTerminalSubjectFinalSnapshot(
                    identity,
                    ActiveToken,
                    evidence.GetSubjectSnapshot(i));
            }

            candidateCoverage = new StageTerminalCandidateCoverageRow[evidence.CandidateCoverageCount];
            for (int i = 0; i < candidateCoverage.Length; i++)
            {
                candidateCoverage[i] = new StageTerminalCandidateCoverageRow(
                    ActiveToken,
                    evidence.GetCandidateCoverage(i));
            }

            ArbitrationPolicyId = policy.PolicyId;
            ArbitrationPolicyDigest = policy.PolicyDigest;
            ResolvedCandidate = evidence.Resolution.Outcome == EncounterTerminalOutcome.Clear
                ? StageTerminalResolvedCandidate.ClearCandidate
                : StageTerminalResolvedCandidate.FailCandidate;
            InvalidatedActiveTokenId = ActiveToken.TokenId;
            InvalidatedActiveTokenDigest = ActiveToken.CanonicalDigest;
            discardedAdmissions =
                new StageDiscardedPendingAdmissionCoverageRow[evidence.DiscardedAdmissionCount];
            for (int i = 0; i < discardedAdmissions.Length; i++)
            {
                discardedAdmissions[i] = new StageDiscardedPendingAdmissionCoverageRow(
                    evidence.GetDiscardedAdmission(i));
            }

            QueueDrainedAndSubjectsFinalized =
                evidence.QueueDrained && evidence.BothSubjectsFinalized;
            ActiveTokenInvalidated = evidence.ActiveTokenInvalidated;
            TerminalClosedSequence = evidence.TerminalClosedSequence;
            SourceEvidenceId = evidence.EvidenceId;
            SourceEvidenceDigest = evidence.CanonicalDigest;
            CanonicalDigest = ComputeCanonicalDigest();
            EnvelopeChecksum = ComputeEnvelopeChecksum("terminal-epoch-closure", CanonicalDigest);
        }

        public string TerminalEpochClosureRecordId { get; }
        public string RunId { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string RouteSnapshotDigest { get; }
        public long CoordinatorRunGeneration { get; }
        public long RootAdmissionSequence { get; }
        public long TerminalEpoch { get; }
        public StageRootResolutionTokenRecord ActiveToken { get; }
        public int SubjectSnapshotCount => subjectSnapshots.Length;
        public int CandidateCoverageCount => candidateCoverage.Length;
        public string ArbitrationPolicyId { get; }
        public string ArbitrationPolicyDigest { get; }
        public StageTerminalResolvedCandidate ResolvedCandidate { get; }
        public string InvalidatedActiveTokenId { get; }
        public string InvalidatedActiveTokenDigest { get; }
        public bool ActiveTokenInvalidated { get; }
        public int DiscardedAdmissionCount => discardedAdmissions.Length;
        public bool QueueDrainedAndSubjectsFinalized { get; }
        public long TerminalClosedSequence { get; }
        public string SourceEvidenceId { get; }
        public string SourceEvidenceDigest { get; }
        public string CanonicalDigest { get; }
        public string EnvelopeChecksum { get; }

        public StageTerminalSubjectFinalSnapshot GetSubjectSnapshot(int index)
        {
            if (index < 0 || index >= subjectSnapshots.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return subjectSnapshots[index];
        }

        public StageTerminalCandidateCoverageRow GetCandidateCoverage(int index)
        {
            if (index < 0 || index >= candidateCoverage.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return candidateCoverage[index];
        }

        public StageDiscardedPendingAdmissionCoverageRow GetDiscardedAdmission(int index)
        {
            if (index < 0 || index >= discardedAdmissions.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return discardedAdmissions[index];
        }

        internal bool Matches(EncounterTerminalEpochEvidence evidence)
        {
            return evidence != null
                && string.Equals(SourceEvidenceId, evidence.EvidenceId, StringComparison.Ordinal)
                && string.Equals(SourceEvidenceDigest, evidence.CanonicalDigest, StringComparison.Ordinal)
                && CoordinatorRunGeneration == evidence.Resolution.RunGeneration
                && RootAdmissionSequence == evidence.Resolution.RootAdmissionSequence
                && TerminalEpoch == evidence.Resolution.Epoch;
        }

        internal static bool TryCreate(
            StageRunIdentity identity,
            StageRunRouteSnapshot routeSnapshot,
            EncounterTerminalEpochEvidence evidence,
            out TerminalEpochClosureRecord record,
            out string error)
        {
            record = null;
            error = string.Empty;
            if (identity == null
                || routeSnapshot == null
                || evidence == null
                || !evidence.QueueDrained
                || !evidence.BothSubjectsFinalized
                || !evidence.ActiveTokenInvalidated
                || evidence.SubjectSnapshotCount != 2
                || evidence.CandidateCoverageCount <= 0
                || evidence.TokenOpenedSequence <= 0
                || evidence.TerminalClosedSequence <= evidence.TokenOpenedSequence
                || string.IsNullOrWhiteSpace(evidence.TokenId)
                || string.IsNullOrWhiteSpace(evidence.TokenDigest)
                || string.IsNullOrWhiteSpace(evidence.CanonicalDigest)
                || string.IsNullOrWhiteSpace(routeSnapshot.TerminalPolicy.PolicyDigest))
            {
                error = "Terminal epoch evidence is incomplete or lacks the frozen policy provenance.";
                return false;
            }

            EncounterTerminalResolution resolution = evidence.Resolution;
            if (resolution.RunGeneration <= 0
                || resolution.RootAdmissionSequence <= 0
                || resolution.Epoch <= 0
                || (resolution.Outcome != EncounterTerminalOutcome.Clear
                    && resolution.Outcome != EncounterTerminalOutcome.Fail))
            {
                error = "Terminal epoch evidence contains an invalid resolution identity.";
                return false;
            }

            EncounterTerminalSubjectSnapshotEvidence player = evidence.GetSubjectSnapshot(0);
            EncounterTerminalSubjectSnapshotEvidence boss = evidence.GetSubjectSnapshot(1);
            if (player.SubjectRole != EncounterTerminalSubjectRole.Player
                || boss.SubjectRole != EncounterTerminalSubjectRole.Boss
                || player.RootAdmissionSequence != resolution.RootAdmissionSequence
                || boss.RootAdmissionSequence != resolution.RootAdmissionSequence
                || player.Epoch != resolution.Epoch
                || boss.Epoch != resolution.Epoch
                || player.CurrentHealth != resolution.PlayerHealth
                || boss.CurrentHealth != resolution.BossHealth
                || (player.SubjectState == EncounterTerminalSubjectState.Down) != resolution.PlayerDown
                || (boss.SubjectState == EncounterTerminalSubjectState.Dead) != resolution.BossDead)
            {
                error = "Player/Boss final snapshots do not match the terminal resolution.";
                return false;
            }

            long previousCandidateSequence = 0;
            bool hasPlayerCandidate = false;
            bool hasBossCandidate = false;
            for (int i = 0; i < evidence.CandidateCoverageCount; i++)
            {
                EncounterTerminalCandidateEvidence candidate = evidence.GetCandidateCoverage(i);
                if (candidate.IntraRootSequence <= previousCandidateSequence
                    || !string.Equals(candidate.TokenId, evidence.TokenId, StringComparison.Ordinal)
                    || !string.Equals(candidate.TokenDigest, evidence.TokenDigest, StringComparison.Ordinal)
                    || candidate.Agreement
                        != EncounterTerminalCandidateAgreementDisposition.MatchesFinalSnapshot)
                {
                    error = "Terminal candidate coverage is unordered or does not match the active token.";
                    return false;
                }

                previousCandidateSequence = candidate.IntraRootSequence;
                hasPlayerCandidate |= candidate.SubjectRole == EncounterTerminalSubjectRole.Player;
                hasBossCandidate |= candidate.SubjectRole == EncounterTerminalSubjectRole.Boss;
            }

            if (hasPlayerCandidate != resolution.PlayerDown
                || hasBossCandidate != resolution.BossDead)
            {
                error = "Terminal candidate coverage does not match the two final subject snapshots.";
                return false;
            }

            long previousDiscardedSequence = resolution.RootAdmissionSequence;
            for (int i = 0; i < evidence.DiscardedAdmissionCount; i++)
            {
                EncounterTerminalDiscardedAdmissionEvidence discarded =
                    evidence.GetDiscardedAdmission(i);
                if (discarded.RootAdmissionSequence <= previousDiscardedSequence
                    || !discarded.NoTokenIssued
                    || discarded.Disposition
                        != EncounterTerminalPendingAdmissionDisposition.DiscardedAfterTerminalClosed)
                {
                    error = "Discarded higher root coverage is incomplete or not canonically ordered.";
                    return false;
                }

                previousDiscardedSequence = discarded.RootAdmissionSequence;
            }

            record = new TerminalEpochClosureRecord(
                identity,
                routeSnapshot.TerminalPolicy,
                evidence);
            return true;
        }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(4096);
            StageCanonicalDigest.Append(builder, "closure.id", TerminalEpochClosureRecordId);
            StageCanonicalDigest.Append(builder, "closure.runId", RunId);
            StageCanonicalDigest.Append(builder, "closure.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "closure.routeRevision", RouteRevision);
            StageCanonicalDigest.Append(builder, "closure.routeDigest", RouteSnapshotDigest);
            StageCanonicalDigest.Append(
                builder,
                "closure.coordinatorRunGeneration",
                CoordinatorRunGeneration);
            StageCanonicalDigest.Append(builder, "closure.rootAdmissionSequence", RootAdmissionSequence);
            StageCanonicalDigest.Append(builder, "closure.terminalEpoch", TerminalEpoch);
            StageCanonicalDigest.Append(builder, "closure.activeTokenId", ActiveToken.TokenId);
            StageCanonicalDigest.Append(builder, "closure.activeTokenDigest", ActiveToken.CanonicalDigest);
            StageCanonicalDigest.Append(builder, "closure.subjectCount", subjectSnapshots.Length);
            for (int i = 0; i < subjectSnapshots.Length; i++)
            {
                StageCanonicalDigest.Append(
                    builder,
                    $"closure.subject[{i}].id",
                    subjectSnapshots[i].SnapshotId);
                StageCanonicalDigest.Append(
                    builder,
                    $"closure.subject[{i}].digest",
                    subjectSnapshots[i].CanonicalDigest);
            }

            StageCanonicalDigest.Append(builder, "closure.candidateCount", candidateCoverage.Length);
            for (int i = 0; i < candidateCoverage.Length; i++)
            {
                StageCanonicalDigest.Append(
                    builder,
                    $"closure.candidate[{i}].digest",
                    candidateCoverage[i].CanonicalDigest);
            }

            StageCanonicalDigest.Append(builder, "closure.policyId", ArbitrationPolicyId);
            StageCanonicalDigest.Append(builder, "closure.policyDigest", ArbitrationPolicyDigest);
            StageCanonicalDigest.Append(builder, "closure.resolvedCandidate", (int)ResolvedCandidate);
            StageCanonicalDigest.Append(builder, "closure.invalidatedTokenId", InvalidatedActiveTokenId);
            StageCanonicalDigest.Append(
                builder,
                "closure.invalidatedTokenDigest",
                InvalidatedActiveTokenDigest);
            StageCanonicalDigest.Append(builder, "closure.activeTokenInvalidated", ActiveTokenInvalidated);
            StageCanonicalDigest.Append(builder, "closure.discardedCount", discardedAdmissions.Length);
            for (int i = 0; i < discardedAdmissions.Length; i++)
            {
                StageCanonicalDigest.Append(
                    builder,
                    $"closure.discarded[{i}].digest",
                    discardedAdmissions[i].CanonicalDigest);
            }

            StageCanonicalDigest.Append(
                builder,
                "closure.queueDrainedAndSubjectsFinalized",
                QueueDrainedAndSubjectsFinalized);
            StageCanonicalDigest.Append(builder, "closure.terminalClosedSequence", TerminalClosedSequence);
            StageCanonicalDigest.Append(builder, "closure.sourceEvidenceId", SourceEvidenceId);
            StageCanonicalDigest.Append(builder, "closure.sourceEvidenceDigest", SourceEvidenceDigest);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class TerminalFinalizationAuthority
    {
        internal TerminalFinalizationAuthority(
            StageRunIdentity identity,
            TerminalEpochClosureRecord terminalEpochClosure,
            long sealedSequence)
        {
            TerminalFinalizationAuthorityId =
                $"{identity.RunId}:terminal-finalization-authority:{sealedSequence}";
            RunId = identity.RunId;
            PlayableStageId = identity.PlayableStageId;
            RouteRevision = identity.RouteRevision;
            RouteSnapshotDigest = identity.RouteSnapshotDigest;
            RootAdmissionSequence = terminalEpochClosure.RootAdmissionSequence;
            TerminalEpoch = terminalEpochClosure.TerminalEpoch;
            TerminalEpochClosureRecordId = terminalEpochClosure.TerminalEpochClosureRecordId;
            TerminalEpochClosureDigest = terminalEpochClosure.CanonicalDigest;
            LatchWinner = TerminalFinalizationLatchWinner.TerminalWon;
            SealedSequence = sealedSequence;
            CanonicalDigest = ComputeCanonicalDigest();
            EnvelopeChecksum = ComputeEnvelopeChecksum("terminal-finalization-authority", CanonicalDigest);
        }

        public string TerminalFinalizationAuthorityId { get; }
        public string RunId { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string RouteSnapshotDigest { get; }
        public long RootAdmissionSequence { get; }
        public long TerminalEpoch { get; }
        public string TerminalEpochClosureRecordId { get; }
        public string TerminalEpochClosureDigest { get; }
        public TerminalFinalizationLatchWinner LatchWinner { get; }
        public long SealedSequence { get; }
        public string CanonicalDigest { get; }
        public string EnvelopeChecksum { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(1024);
            StageCanonicalDigest.Append(builder, "authority.id", TerminalFinalizationAuthorityId);
            StageCanonicalDigest.Append(builder, "authority.runId", RunId);
            StageCanonicalDigest.Append(builder, "authority.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "authority.routeRevision", RouteRevision);
            StageCanonicalDigest.Append(builder, "authority.routeDigest", RouteSnapshotDigest);
            StageCanonicalDigest.Append(builder, "authority.rootAdmissionSequence", RootAdmissionSequence);
            StageCanonicalDigest.Append(builder, "authority.terminalEpoch", TerminalEpoch);
            StageCanonicalDigest.Append(
                builder,
                "authority.terminalEpochClosureRecordId",
                TerminalEpochClosureRecordId);
            StageCanonicalDigest.Append(
                builder,
                "authority.terminalEpochClosureDigest",
                TerminalEpochClosureDigest);
            StageCanonicalDigest.Append(builder, "authority.latchWinner", (int)LatchWinner);
            StageCanonicalDigest.Append(builder, "authority.sealedSequence", SealedSequence);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class TerminalFinalizationOwnerCoverageRow
    {
        internal TerminalFinalizationOwnerCoverageRow(
            TerminalFinalizationOwnerKind ownerKind,
            TerminalFinalizationOwnerDisposition disposition)
        {
            OwnerKind = ownerKind;
            Disposition = disposition;
            ReceiptId = string.Empty;
            ReceiptDigest = string.Empty;
            HasTypedReceipt = false;
            CanonicalDigest = ComputeCanonicalDigest();
        }

        public TerminalFinalizationOwnerKind OwnerKind { get; }
        public TerminalFinalizationOwnerDisposition Disposition { get; }
        public bool HasTypedReceipt { get; }
        public string ReceiptId { get; }
        public string ReceiptDigest { get; }
        public string CanonicalDigest { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(256);
            StageCanonicalDigest.Append(builder, "owner.kind", (int)OwnerKind);
            StageCanonicalDigest.Append(builder, "owner.disposition", (int)Disposition);
            StageCanonicalDigest.Append(builder, "owner.hasTypedReceipt", HasTypedReceipt);
            StageCanonicalDigest.Append(builder, "owner.receiptId", ReceiptId);
            StageCanonicalDigest.Append(builder, "owner.receiptDigest", ReceiptDigest);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    public sealed class TerminalFinalizationOwnerCoverageRecord
    {
        private readonly TerminalFinalizationOwnerCoverageRow[] ownerRows;

        private TerminalFinalizationOwnerCoverageRecord(
            StageRunIdentity identity,
            TerminalFinalizationAuthority authority,
            StageTerminalFinalizationContext finalizationContext,
            long sealedSequence)
        {
            TerminalFinalizationOwnerCoverageRecordId =
                $"{identity.RunId}:terminal-finalization-owner-coverage:{sealedSequence}";
            RunId = identity.RunId;
            PlayableStageId = identity.PlayableStageId;
            RouteRevision = identity.RouteRevision;
            RouteSnapshotDigest = identity.RouteSnapshotDigest;
            TerminalFinalizationAuthorityId = authority.TerminalFinalizationAuthorityId;
            TerminalFinalizationAuthorityDigest = authority.CanonicalDigest;
            FinalizationContext = finalizationContext;
            ownerRows = new[]
            {
                new TerminalFinalizationOwnerCoverageRow(
                    TerminalFinalizationOwnerKind.TutorialLesson,
                    TerminalFinalizationOwnerDisposition.NotAdmitted),
                new TerminalFinalizationOwnerCoverageRow(
                    TerminalFinalizationOwnerKind.TutorialCourse,
                    TerminalFinalizationOwnerDisposition.NotAdmitted),
                new TerminalFinalizationOwnerCoverageRow(
                    TerminalFinalizationOwnerKind.EncounterExecution,
                    TerminalFinalizationOwnerDisposition.NotAdmitted),
                new TerminalFinalizationOwnerCoverageRow(
                    TerminalFinalizationOwnerKind.Presentation,
                    TerminalFinalizationOwnerDisposition.NotAdmitted)
            };
            PendingFinalizationOwnerCount = 0;
            ZeroPendingFinalizationOwners = true;
            SealedSequence = sealedSequence;
            CanonicalDigest = ComputeCanonicalDigest();
            EnvelopeChecksum = ComputeEnvelopeChecksum(
                "terminal-finalization-owner-coverage",
                CanonicalDigest);
        }

        public string TerminalFinalizationOwnerCoverageRecordId { get; }
        public string RunId { get; }
        public string PlayableStageId { get; }
        public int RouteRevision { get; }
        public string RouteSnapshotDigest { get; }
        public string TerminalFinalizationAuthorityId { get; }
        public string TerminalFinalizationAuthorityDigest { get; }
        public StageTerminalFinalizationContext FinalizationContext { get; }
        public int OwnerRowCount => ownerRows.Length;
        public int PendingFinalizationOwnerCount { get; }
        public bool ZeroPendingFinalizationOwners { get; }
        public long SealedSequence { get; }
        public string CanonicalDigest { get; }
        public string EnvelopeChecksum { get; }

        public TerminalFinalizationOwnerCoverageRow GetOwnerRow(int index)
        {
            if (index < 0 || index >= ownerRows.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return ownerRows[index];
        }

        internal static bool TryCreateCurrentSnapshot(
            StageRunIdentity identity,
            TerminalFinalizationAuthority authority,
            StageTerminalFinalizationContext finalizationContext,
            long sealedSequence,
            out TerminalFinalizationOwnerCoverageRecord record,
            out string error)
        {
            record = null;
            error = string.Empty;
            if (identity == null
                || authority == null
                || identity.SchemaVersion != 1
                || finalizationContext
                    != StageTerminalFinalizationContext.NonCourseStationTerminal
                || authority.LatchWinner != TerminalFinalizationLatchWinner.TerminalWon
                || sealedSequence <= authority.SealedSequence
                || !string.Equals(identity.RunId, authority.RunId, StringComparison.Ordinal)
                || !string.Equals(
                    identity.PlayableStageId,
                    authority.PlayableStageId,
                    StringComparison.Ordinal)
                || identity.RouteRevision != authority.RouteRevision
                || !string.Equals(
                    identity.RouteSnapshotDigest,
                    authority.RouteSnapshotDigest,
                    StringComparison.Ordinal))
            {
                error = "Current terminal-finalization owner coverage is not valid for this route schema or authority.";
                return false;
            }

            record = new TerminalFinalizationOwnerCoverageRecord(
                identity,
                authority,
                finalizationContext,
                sealedSequence);
            return true;
        }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(1536);
            StageCanonicalDigest.Append(
                builder,
                "coverage.id",
                TerminalFinalizationOwnerCoverageRecordId);
            StageCanonicalDigest.Append(builder, "coverage.runId", RunId);
            StageCanonicalDigest.Append(builder, "coverage.playableStageId", PlayableStageId);
            StageCanonicalDigest.Append(builder, "coverage.routeRevision", RouteRevision);
            StageCanonicalDigest.Append(builder, "coverage.routeDigest", RouteSnapshotDigest);
            StageCanonicalDigest.Append(
                builder,
                "coverage.authorityId",
                TerminalFinalizationAuthorityId);
            StageCanonicalDigest.Append(
                builder,
                "coverage.authorityDigest",
                TerminalFinalizationAuthorityDigest);
            StageCanonicalDigest.Append(builder, "coverage.context", (int)FinalizationContext);
            StageCanonicalDigest.Append(builder, "coverage.ownerCount", ownerRows.Length);
            for (int i = 0; i < ownerRows.Length; i++)
            {
                StageCanonicalDigest.Append(
                    builder,
                    $"coverage.owner[{i}]",
                    ownerRows[i].CanonicalDigest);
            }

            StageCanonicalDigest.Append(
                builder,
                "coverage.pendingOwnerCount",
                PendingFinalizationOwnerCount);
            StageCanonicalDigest.Append(
                builder,
                "coverage.zeroPendingOwners",
                ZeroPendingFinalizationOwners);
            StageCanonicalDigest.Append(builder, "coverage.sealedSequence", SealedSequence);
            return StageCanonicalDigest.Compute(builder.ToString());
        }
    }

    internal static class StageRunFinalizationDigest
    {
        public static string ComputeEnvelopeChecksum(string kind, string canonicalDigest)
        {
            StringBuilder builder = new(192);
            StageCanonicalDigest.Append(builder, "envelope.kind", kind);
            StageCanonicalDigest.Append(builder, "envelope.canonicalDigest", canonicalDigest);
            return StageCanonicalDigest.Compute(builder.ToString());
        }

        public static void AppendFloat(StringBuilder builder, string key, float value)
        {
            StageCanonicalDigest.Append(
                builder,
                key,
                value.ToString("R", CultureInfo.InvariantCulture));
        }
    }

}
