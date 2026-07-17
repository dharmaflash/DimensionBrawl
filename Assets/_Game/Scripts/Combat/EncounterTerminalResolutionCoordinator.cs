using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace DimensionBrawl.Combat
{
    public enum EncounterTerminalCoordinatorState
    {
        Unbound = 0,
        Idle = 1,
        Open = 2,
        Draining = 3,
        Finalizing = 4,
        EpochClosed = 5,
        TerminalClosed = 6,
        Faulted = 7,
        Cancelled = 8
    }

    public enum EncounterTerminalOutcome
    {
        None = 0,
        Clear = 1,
        Fail = 2
    }

    public enum EncounterTerminalReason
    {
        None = 0,
        BossTerminal = 1,
        PlayerTerminal = 2,
        SimultaneousTerminalClear = 3
    }

    public enum EncounterTerminalDiagnosticReason
    {
        None = 0,
        BindingConflict = 1,
        UnsupportedBoundMutation = 2,
        ClosedToken = 3,
        TokenMismatch = 4,
        InvalidSubject = 5,
        SubjectUnavailable = 6,
        ProducerException = 7,
        MutationException = 8,
        QueueLimitExceeded = 9,
        DirectMutationBypass = 10,
        InvalidTerminalEvidence = 11,
        ReentrantCallbackRootAdmission = 12,
        FinalizationException = 13
    }

    public enum CombatRootAdmissionDisposition
    {
        Executed = 0,
        Deferred = 1,
        Rejected = 2
    }

    public readonly struct CombatRootAdmissionResult
    {
        internal CombatRootAdmissionResult(
            CombatRootAdmissionDisposition disposition,
            long rootAdmissionSequence,
            EncounterTerminalCoordinatorState coordinatorState)
        {
            Disposition = disposition;
            RootAdmissionSequence = rootAdmissionSequence;
            CoordinatorState = coordinatorState;
        }

        public CombatRootAdmissionDisposition Disposition { get; }
        public long RootAdmissionSequence { get; }
        public EncounterTerminalCoordinatorState CoordinatorState { get; }
    }

    public readonly struct EncounterTerminalResolution
    {
        internal EncounterTerminalResolution(
            long runGeneration,
            long rootAdmissionSequence,
            long epoch,
            EncounterTerminalOutcome outcome,
            EncounterTerminalReason reason,
            bool playerDown,
            bool bossDead,
            float playerHealth,
            float bossHealth)
        {
            RunGeneration = runGeneration;
            RootAdmissionSequence = rootAdmissionSequence;
            Epoch = epoch;
            Outcome = outcome;
            Reason = reason;
            PlayerDown = playerDown;
            BossDead = bossDead;
            PlayerHealth = playerHealth;
            BossHealth = bossHealth;
        }

        public long RunGeneration { get; }
        public long RootAdmissionSequence { get; }
        public long Epoch { get; }
        public EncounterTerminalOutcome Outcome { get; }
        public EncounterTerminalReason Reason { get; }
        public bool PlayerDown { get; }
        public bool BossDead { get; }
        public float PlayerHealth { get; }
        public float BossHealth { get; }
    }

    public enum EncounterTerminalSubjectRole
    {
        Player = 1,
        Boss = 2
    }

    public enum EncounterTerminalSubjectState
    {
        Alive = 1,
        Down = 2,
        Dead = 3
    }

    public enum EncounterTerminalCandidateKind
    {
        PlayerTerminal = 1,
        BossTerminal = 2
    }

    public enum EncounterTerminalCandidateAgreementDisposition
    {
        MatchesFinalSnapshot = 1
    }

    public enum EncounterTerminalPendingAdmissionDisposition
    {
        DiscardedAfterTerminalClosed = 1
    }

    public sealed class EncounterTerminalSubjectSnapshotEvidence
    {
        internal EncounterTerminalSubjectSnapshotEvidence(
            long runGeneration,
            long rootAdmissionSequence,
            long epoch,
            string tokenId,
            string tokenDigest,
            EncounterTerminalSubjectRole subjectRole,
            float currentHealth,
            float maxHealth,
            EncounterTerminalSubjectState subjectState,
            EncounterTerminalCandidateKind? terminalCandidate,
            long acceptedCandidateSequence,
            long snapshotSequence)
        {
            SnapshotId = $"encounter:{runGeneration}:root:{rootAdmissionSequence}:epoch:{epoch}:subject:{subjectRole}";
            RunGeneration = runGeneration;
            RootAdmissionSequence = rootAdmissionSequence;
            Epoch = epoch;
            TokenId = tokenId ?? string.Empty;
            TokenDigest = tokenDigest ?? string.Empty;
            SubjectRole = subjectRole;
            SubjectBindingId = $"encounter:{runGeneration}:binding:{subjectRole}";
            BindingGeneration = runGeneration;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            SubjectState = subjectState;
            TerminalCandidate = terminalCandidate;
            AcceptedCandidateSequence = acceptedCandidateSequence;
            SnapshotSequence = snapshotSequence;
            CanonicalDigest = ComputeCanonicalDigest();
            EnvelopeChecksum = EncounterEvidenceDigest.ComputeEnvelope(
                "terminal-subject-snapshot",
                CanonicalDigest);
        }

        public string SnapshotId { get; }
        public long RunGeneration { get; }
        public long RootAdmissionSequence { get; }
        public long Epoch { get; }
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
        public string CanonicalDigest { get; }
        public string EnvelopeChecksum { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(768);
            EncounterEvidenceDigest.Append(builder, "snapshot.id", SnapshotId);
            EncounterEvidenceDigest.Append(builder, "snapshot.runGeneration", RunGeneration);
            EncounterEvidenceDigest.Append(builder, "snapshot.rootAdmissionSequence", RootAdmissionSequence);
            EncounterEvidenceDigest.Append(builder, "snapshot.epoch", Epoch);
            EncounterEvidenceDigest.Append(builder, "snapshot.tokenId", TokenId);
            EncounterEvidenceDigest.Append(builder, "snapshot.tokenDigest", TokenDigest);
            EncounterEvidenceDigest.Append(builder, "snapshot.subjectRole", (int)SubjectRole);
            EncounterEvidenceDigest.Append(builder, "snapshot.subjectBindingId", SubjectBindingId);
            EncounterEvidenceDigest.Append(builder, "snapshot.bindingGeneration", BindingGeneration);
            EncounterEvidenceDigest.AppendFloat(builder, "snapshot.currentHealth", CurrentHealth);
            EncounterEvidenceDigest.AppendFloat(builder, "snapshot.maxHealth", MaxHealth);
            EncounterEvidenceDigest.Append(builder, "snapshot.subjectState", (int)SubjectState);
            EncounterEvidenceDigest.Append(
                builder,
                "snapshot.candidatePresent",
                TerminalCandidate.HasValue);
            EncounterEvidenceDigest.Append(
                builder,
                "snapshot.candidateKind",
                TerminalCandidate.HasValue ? (int)TerminalCandidate.Value : 0);
            EncounterEvidenceDigest.Append(
                builder,
                "snapshot.acceptedCandidateSequence",
                AcceptedCandidateSequence);
            EncounterEvidenceDigest.Append(builder, "snapshot.sequence", SnapshotSequence);
            return EncounterEvidenceDigest.Compute(builder.ToString());
        }
    }

    public sealed class EncounterTerminalCandidateEvidence
    {
        internal EncounterTerminalCandidateEvidence(
            long intraRootSequence,
            string causeIdentity,
            EncounterTerminalSubjectRole subjectRole,
            EncounterTerminalCandidateKind candidateKind,
            string tokenId,
            string tokenDigest,
            float observedCurrentHealth,
            float observedMaxHealth,
            EncounterTerminalSubjectState observedState,
            long observationSequence)
        {
            IntraRootSequence = intraRootSequence;
            CauseIdentity = causeIdentity ?? string.Empty;
            SubjectRole = subjectRole;
            CandidateKind = candidateKind;
            TokenId = tokenId ?? string.Empty;
            TokenDigest = tokenDigest ?? string.Empty;
            ObservedCurrentHealth = observedCurrentHealth;
            ObservedMaxHealth = observedMaxHealth;
            ObservedState = observedState;
            ObservationSequence = observationSequence;
            Agreement = EncounterTerminalCandidateAgreementDisposition.MatchesFinalSnapshot;
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
        public string CanonicalDigest { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(512);
            EncounterEvidenceDigest.Append(builder, "candidate.intraRootSequence", IntraRootSequence);
            EncounterEvidenceDigest.Append(builder, "candidate.causeIdentity", CauseIdentity);
            EncounterEvidenceDigest.Append(builder, "candidate.subjectRole", (int)SubjectRole);
            EncounterEvidenceDigest.Append(builder, "candidate.kind", (int)CandidateKind);
            EncounterEvidenceDigest.Append(builder, "candidate.tokenId", TokenId);
            EncounterEvidenceDigest.Append(builder, "candidate.tokenDigest", TokenDigest);
            EncounterEvidenceDigest.AppendFloat(
                builder,
                "candidate.observedCurrentHealth",
                ObservedCurrentHealth);
            EncounterEvidenceDigest.AppendFloat(
                builder,
                "candidate.observedMaxHealth",
                ObservedMaxHealth);
            EncounterEvidenceDigest.Append(builder, "candidate.observedState", (int)ObservedState);
            EncounterEvidenceDigest.Append(builder, "candidate.observationSequence", ObservationSequence);
            EncounterEvidenceDigest.Append(builder, "candidate.agreement", (int)Agreement);
            return EncounterEvidenceDigest.Compute(builder.ToString());
        }
    }

    public sealed class EncounterTerminalDiscardedAdmissionEvidence
    {
        internal EncounterTerminalDiscardedAdmissionEvidence(
            long rootAdmissionSequence,
            string causeIdentity)
        {
            RootAdmissionSequence = rootAdmissionSequence;
            CauseIdentity = causeIdentity ?? string.Empty;
            NoTokenIssued = true;
            Disposition = EncounterTerminalPendingAdmissionDisposition.DiscardedAfterTerminalClosed;
            CanonicalDigest = ComputeCanonicalDigest();
        }

        public long RootAdmissionSequence { get; }
        public string CauseIdentity { get; }
        public bool NoTokenIssued { get; }
        public EncounterTerminalPendingAdmissionDisposition Disposition { get; }
        public string CanonicalDigest { get; }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(256);
            EncounterEvidenceDigest.Append(builder, "discarded.rootAdmissionSequence", RootAdmissionSequence);
            EncounterEvidenceDigest.Append(builder, "discarded.causeIdentity", CauseIdentity);
            EncounterEvidenceDigest.Append(builder, "discarded.noTokenIssued", NoTokenIssued);
            EncounterEvidenceDigest.Append(builder, "discarded.disposition", (int)Disposition);
            return EncounterEvidenceDigest.Compute(builder.ToString());
        }
    }

    public sealed class EncounterTerminalEpochEvidence
    {
        private readonly EncounterTerminalSubjectSnapshotEvidence[] subjectSnapshots;
        private readonly EncounterTerminalCandidateEvidence[] candidateCoverage;
        private readonly EncounterTerminalDiscardedAdmissionEvidence[] discardedAdmissions;

        internal EncounterTerminalEpochEvidence(
            EncounterTerminalResolution resolution,
            string tokenId,
            string tokenDigest,
            long tokenOpenedSequence,
            EncounterTerminalSubjectSnapshotEvidence[] subjectSnapshots,
            EncounterTerminalCandidateEvidence[] candidateCoverage,
            EncounterTerminalDiscardedAdmissionEvidence[] discardedAdmissions,
            long terminalClosedSequence)
        {
            EvidenceId =
                $"encounter:{resolution.RunGeneration}:root:{resolution.RootAdmissionSequence}:epoch:{resolution.Epoch}:terminal-closure";
            Resolution = resolution;
            TokenId = tokenId ?? string.Empty;
            TokenDigest = tokenDigest ?? string.Empty;
            TokenOpenedSequence = tokenOpenedSequence;
            this.subjectSnapshots = (EncounterTerminalSubjectSnapshotEvidence[])subjectSnapshots.Clone();
            this.candidateCoverage = (EncounterTerminalCandidateEvidence[])candidateCoverage.Clone();
            this.discardedAdmissions =
                (EncounterTerminalDiscardedAdmissionEvidence[])discardedAdmissions.Clone();
            QueueDrained = true;
            BothSubjectsFinalized = true;
            ActiveTokenInvalidated = true;
            TerminalClosedSequence = terminalClosedSequence;
            CanonicalDigest = ComputeCanonicalDigest();
            EnvelopeChecksum = EncounterEvidenceDigest.ComputeEnvelope(
                "terminal-epoch-evidence",
                CanonicalDigest);
        }

        public string EvidenceId { get; }
        public EncounterTerminalResolution Resolution { get; }
        public string TokenId { get; }
        public string TokenDigest { get; }
        public long TokenOpenedSequence { get; }
        public int SubjectSnapshotCount => subjectSnapshots.Length;
        public int CandidateCoverageCount => candidateCoverage.Length;
        public int DiscardedAdmissionCount => discardedAdmissions.Length;
        public bool QueueDrained { get; }
        public bool BothSubjectsFinalized { get; }
        public bool ActiveTokenInvalidated { get; }
        public long TerminalClosedSequence { get; }
        public string CanonicalDigest { get; }
        public string EnvelopeChecksum { get; }

        public EncounterTerminalSubjectSnapshotEvidence GetSubjectSnapshot(int index)
        {
            if (index < 0 || index >= subjectSnapshots.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return subjectSnapshots[index];
        }

        public EncounterTerminalCandidateEvidence GetCandidateCoverage(int index)
        {
            if (index < 0 || index >= candidateCoverage.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return candidateCoverage[index];
        }

        public EncounterTerminalDiscardedAdmissionEvidence GetDiscardedAdmission(int index)
        {
            if (index < 0 || index >= discardedAdmissions.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return discardedAdmissions[index];
        }

        private string ComputeCanonicalDigest()
        {
            StringBuilder builder = new(2048);
            EncounterEvidenceDigest.Append(builder, "epochEvidence.id", EvidenceId);
            EncounterEvidenceDigest.Append(builder, "epochEvidence.runGeneration", Resolution.RunGeneration);
            EncounterEvidenceDigest.Append(
                builder,
                "epochEvidence.rootAdmissionSequence",
                Resolution.RootAdmissionSequence);
            EncounterEvidenceDigest.Append(builder, "epochEvidence.epoch", Resolution.Epoch);
            EncounterEvidenceDigest.Append(builder, "epochEvidence.tokenId", TokenId);
            EncounterEvidenceDigest.Append(builder, "epochEvidence.tokenDigest", TokenDigest);
            EncounterEvidenceDigest.Append(builder, "epochEvidence.tokenOpenedSequence", TokenOpenedSequence);
            EncounterEvidenceDigest.Append(builder, "epochEvidence.queueDrained", QueueDrained);
            EncounterEvidenceDigest.Append(
                builder,
                "epochEvidence.bothSubjectsFinalized",
                BothSubjectsFinalized);
            EncounterEvidenceDigest.Append(
                builder,
                "epochEvidence.activeTokenInvalidated",
                ActiveTokenInvalidated);
            EncounterEvidenceDigest.Append(builder, "epochEvidence.subjectCount", subjectSnapshots.Length);
            for (int i = 0; i < subjectSnapshots.Length; i++)
            {
                EncounterEvidenceDigest.Append(
                    builder,
                    $"epochEvidence.subject[{i}]",
                    subjectSnapshots[i].CanonicalDigest);
            }

            EncounterEvidenceDigest.Append(builder, "epochEvidence.candidateCount", candidateCoverage.Length);
            for (int i = 0; i < candidateCoverage.Length; i++)
            {
                EncounterEvidenceDigest.Append(
                    builder,
                    $"epochEvidence.candidate[{i}]",
                    candidateCoverage[i].CanonicalDigest);
            }

            EncounterEvidenceDigest.Append(
                builder,
                "epochEvidence.discardedAdmissionCount",
                discardedAdmissions.Length);
            for (int i = 0; i < discardedAdmissions.Length; i++)
            {
                EncounterEvidenceDigest.Append(
                    builder,
                    $"epochEvidence.discardedAdmission[{i}]",
                    discardedAdmissions[i].CanonicalDigest);
            }

            EncounterEvidenceDigest.Append(builder, "epochEvidence.outcome", (int)Resolution.Outcome);
            EncounterEvidenceDigest.Append(builder, "epochEvidence.reason", (int)Resolution.Reason);
            EncounterEvidenceDigest.Append(builder, "epochEvidence.playerDown", Resolution.PlayerDown);
            EncounterEvidenceDigest.Append(builder, "epochEvidence.bossDead", Resolution.BossDead);
            EncounterEvidenceDigest.AppendFloat(
                builder,
                "epochEvidence.playerHealth",
                Resolution.PlayerHealth);
            EncounterEvidenceDigest.AppendFloat(
                builder,
                "epochEvidence.bossHealth",
                Resolution.BossHealth);
            EncounterEvidenceDigest.Append(
                builder,
                "epochEvidence.terminalClosedSequence",
                TerminalClosedSequence);
            return EncounterEvidenceDigest.Compute(builder.ToString());
        }
    }

    public readonly struct EncounterTerminalDiagnostic
    {
        internal EncounterTerminalDiagnostic(
            EncounterTerminalDiagnosticReason reason,
            long runGeneration,
            long rootAdmissionSequence,
            long epoch,
            string message)
        {
            Reason = reason;
            RunGeneration = runGeneration;
            RootAdmissionSequence = rootAdmissionSequence;
            Epoch = epoch;
            Message = message ?? string.Empty;
        }

        public EncounterTerminalDiagnosticReason Reason { get; }
        public long RunGeneration { get; }
        public long RootAdmissionSequence { get; }
        public long Epoch { get; }
        public string Message { get; }
    }

    internal enum BoundHealthMutationKind
    {
        ConfigureMaxHealth = 0,
        ResetHealthToFull = 1
    }

    internal interface ICombatHealthMutationAuthority
    {
        bool TryApplyDamage(CombatHealth target, DamageInfo damageInfo);
        bool IsAuthorizedDamageMutation(CombatHealth target);
        void ReportDirectMutationBypass(CombatHealth target);
        bool TryAuthorizeBoundMutation(CombatHealth target, BoundHealthMutationKind mutationKind);
    }

    internal readonly struct RootResolutionToken : IEquatable<RootResolutionToken>
    {
        public RootResolutionToken(long runGeneration, long rootAdmissionSequence, long epoch)
        {
            RunGeneration = runGeneration;
            RootAdmissionSequence = rootAdmissionSequence;
            Epoch = epoch;
        }

        public long RunGeneration { get; }
        public long RootAdmissionSequence { get; }
        public long Epoch { get; }
        public bool IsValid => RunGeneration > 0 && RootAdmissionSequence > 0 && Epoch > 0;

        public bool Equals(RootResolutionToken other)
        {
            return RunGeneration == other.RunGeneration
                && RootAdmissionSequence == other.RootAdmissionSequence
                && Epoch == other.Epoch;
        }

        public override bool Equals(object obj)
        {
            return obj is RootResolutionToken other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = RunGeneration.GetHashCode();
                hashCode = (hashCode * 397) ^ RootAdmissionSequence.GetHashCode();
                hashCode = (hashCode * 397) ^ Epoch.GetHashCode();
                return hashCode;
            }
        }
    }

    public sealed class CanonicalCombatRootContext
    {
        private readonly EncounterTerminalResolutionCoordinator coordinator;
        private readonly RootResolutionToken token;

        internal CanonicalCombatRootContext(
            EncounterTerminalResolutionCoordinator coordinator,
            RootResolutionToken token)
        {
            this.coordinator = coordinator;
            this.token = token;
        }

        public long RunGeneration => token.RunGeneration;
        public long RootAdmissionSequence => token.RootAdmissionSequence;
        public long Epoch => token.Epoch;

        public bool TryApplyDamage(CombatHealth target, DamageInfo damageInfo)
        {
            return coordinator != null
                && coordinator.TryQueueDamage(this, target, damageInfo);
        }

        internal RootResolutionToken Token => token;

        internal bool IsOwnedBy(EncounterTerminalResolutionCoordinator owner)
        {
            return ReferenceEquals(coordinator, owner);
        }
    }

    public static class CanonicalCombatRootAdmission
    {
        public static CombatRootAdmissionResult Run(
            EncounterTerminalResolutionCoordinator coordinator,
            Action<CanonicalCombatRootContext> producer)
        {
            return Run(coordinator, "combat.external-root", producer);
        }

        public static CombatRootAdmissionResult Run(
            EncounterTerminalResolutionCoordinator coordinator,
            string causeIdentity,
            Action<CanonicalCombatRootContext> producer)
        {
            if (coordinator == null)
            {
                throw new ArgumentNullException(nameof(coordinator));
            }

            return coordinator.AdmitRoot(causeIdentity, producer);
        }
    }

    public sealed class EncounterTerminalResolutionCoordinator :
        ICombatHealthMutationAuthority,
        IDisposable
    {
        private const int MaxMutationsPerRoot = 4096;

        private sealed class PendingRoot
        {
            public PendingRoot(
                long sequence,
                string causeIdentity,
                Action<CanonicalCombatRootContext> producer)
            {
                Sequence = sequence;
                CauseIdentity = causeIdentity;
                Producer = producer;
            }

            public long Sequence { get; }
            public string CauseIdentity { get; }
            public Action<CanonicalCombatRootContext> Producer { get; }
            public bool Executed { get; set; }
        }

        private sealed class TerminalCandidateDraft
        {
            public TerminalCandidateDraft(
                long intraRootSequence,
                string causeIdentity,
                EncounterTerminalSubjectRole subjectRole,
                EncounterTerminalCandidateKind candidateKind,
                float observedCurrentHealth,
                float observedMaxHealth,
                EncounterTerminalSubjectState observedState,
                long observationSequence)
            {
                IntraRootSequence = intraRootSequence;
                CauseIdentity = causeIdentity;
                SubjectRole = subjectRole;
                CandidateKind = candidateKind;
                ObservedCurrentHealth = observedCurrentHealth;
                ObservedMaxHealth = observedMaxHealth;
                ObservedState = observedState;
                ObservationSequence = observationSequence;
            }

            public long IntraRootSequence { get; }
            public string CauseIdentity { get; }
            public EncounterTerminalSubjectRole SubjectRole { get; }
            public EncounterTerminalCandidateKind CandidateKind { get; }
            public float ObservedCurrentHealth { get; }
            public float ObservedMaxHealth { get; }
            public EncounterTerminalSubjectState ObservedState { get; }
            public long ObservationSequence { get; }
        }

        private sealed class DamageMutation
        {
            public DamageMutation(
                RootResolutionToken token,
                long intraRootSequence,
                CombatHealth target,
                DamageInfo damageInfo)
            {
                Token = token;
                IntraRootSequence = intraRootSequence;
                Target = target;
                DamageInfo = damageInfo;
            }

            public RootResolutionToken Token { get; }
            public long IntraRootSequence { get; }
            public CombatHealth Target { get; }
            public DamageInfo DamageInfo { get; }
            public bool Executed { get; set; }
            public bool Applied { get; set; }
        }

        private readonly Queue<PendingRoot> pendingRoots = new();
        private readonly Queue<DamageMutation> activeMutations = new();
        private readonly List<TerminalCandidateDraft> terminalCandidateDrafts = new(2);
        private readonly long runGeneration;
        private CombatHealth playerHealth;
        private CombatHealth bossHealth;
        private RootResolutionToken activeToken;
        private CombatHealth activeMutationTarget;
        private long nextRootAdmissionSequence;
        private long nextEpoch;
        private long nextIntraRootSequence;
        private long nextEvidenceSequence;
        private long activeTokenOpenedSequence;
        private string activeRootCauseIdentity = string.Empty;
        private bool processingRoots;
        private bool acceptingEnqueues;
        private bool executingMutation;
        private bool playerTerminalCandidate;
        private bool bossTerminalCandidate;
        private bool diagnosticPublished;
#if UNITY_INCLUDE_TESTS
        private Action<EncounterTerminalResolutionCoordinator> finalizationBoundaryForTests;
        private Action<EncounterTerminalResolutionCoordinator> finalSnapshotBoundaryForTests;
#endif

        private EncounterTerminalResolutionCoordinator(
            long runGeneration,
            CombatHealth playerHealth,
            CombatHealth bossHealth)
        {
            this.runGeneration = runGeneration;
            this.playerHealth = playerHealth;
            this.bossHealth = bossHealth;
            State = EncounterTerminalCoordinatorState.Unbound;
        }

        public EncounterTerminalCoordinatorState State { get; private set; }
        public long RunGeneration => runGeneration;
        public long ActiveRootAdmissionSequence => activeToken.RootAdmissionSequence;
        public long ActiveEpoch => activeToken.Epoch;
        public long LastClosedRootAdmissionSequence { get; private set; }
        public long LastClosedEpoch { get; private set; }
        public bool HasTerminalResolution { get; private set; }
        public EncounterTerminalResolution TerminalResolution { get; private set; }
        public bool HasTerminalEpochEvidence { get; private set; }
        public EncounterTerminalEpochEvidence TerminalEpochEvidence { get; private set; }
        public bool HasDiagnostic { get; private set; }
        public EncounterTerminalDiagnostic Diagnostic { get; private set; }

        public event Action<EncounterTerminalResolution> Resolved;
        public event Action<EncounterTerminalDiagnostic> DiagnosticAborted;

        internal void ReportSubjectRebindAttempt()
        {
            Fault(
                EncounterTerminalDiagnosticReason.SubjectUnavailable,
                "A live terminal subject binding cannot be replaced before the current coordinator closes.");
        }

        internal static bool TryCreate(
            long runGeneration,
            CombatHealth playerHealth,
            CombatHealth bossHealth,
            out EncounterTerminalResolutionCoordinator coordinator,
            out EncounterTerminalDiagnostic diagnostic)
        {
            coordinator = null;
            diagnostic = default;

            if (runGeneration <= 0
                || playerHealth == null
                || bossHealth == null
                || playerHealth == bossHealth
                || !playerHealth.isActiveAndEnabled
                || !bossHealth.isActiveAndEnabled
                || !playerHealth.IsAlive
                || !bossHealth.IsAlive)
            {
                diagnostic = new EncounterTerminalDiagnostic(
                    EncounterTerminalDiagnosticReason.BindingConflict,
                    runGeneration,
                    0,
                    0,
                    $"Terminal subjects must be distinct, active, and alive before binding. "
                    + $"Run={runGeneration}, PlayerNull={playerHealth == null}, BossNull={bossHealth == null}, "
                    + $"SameSubject={playerHealth == bossHealth}, "
                    + $"PlayerActive={playerHealth != null && playerHealth.isActiveAndEnabled}, "
                    + $"BossActive={bossHealth != null && bossHealth.isActiveAndEnabled}, "
                    + $"PlayerAlive={playerHealth != null && playerHealth.IsAlive}, "
                    + $"BossAlive={bossHealth != null && bossHealth.IsAlive}.");
                return false;
            }

            EncounterTerminalResolutionCoordinator candidate = new(
                runGeneration,
                playerHealth,
                bossHealth);
            if (!playerHealth.TryBindMutationAuthority(candidate))
            {
                diagnostic = new EncounterTerminalDiagnostic(
                    EncounterTerminalDiagnosticReason.BindingConflict,
                    runGeneration,
                    0,
                    0,
                    "The player terminal subject is already bound to another authority.");
                return false;
            }

            if (!bossHealth.TryBindMutationAuthority(candidate))
            {
                playerHealth.UnbindMutationAuthority(candidate);
                diagnostic = new EncounterTerminalDiagnostic(
                    EncounterTerminalDiagnosticReason.BindingConflict,
                    runGeneration,
                    0,
                    0,
                    "The boss terminal subject is already bound to another authority.");
                return false;
            }

            candidate.State = EncounterTerminalCoordinatorState.Idle;
            coordinator = candidate;
            return true;
        }

        public CombatRootAdmissionResult AdmitRoot(Action<CanonicalCombatRootContext> producer)
        {
            return AdmitRoot("combat.external-root", producer);
        }

        public CombatRootAdmissionResult AdmitRoot(
            string causeIdentity,
            Action<CanonicalCombatRootContext> producer)
        {
            if (producer == null)
            {
                throw new ArgumentNullException(nameof(producer));
            }

            if (executingMutation)
            {
                Fault(
                    EncounterTerminalDiagnosticReason.ReentrantCallbackRootAdmission,
                    "A mutation callback cannot mint an independent combat root.");
                return new CombatRootAdmissionResult(
                    CombatRootAdmissionDisposition.Rejected,
                    0,
                    State);
            }

            if (!CanAcceptRootAdmission())
            {
                return new CombatRootAdmissionResult(
                    CombatRootAdmissionDisposition.Rejected,
                    0,
                    State);
            }

            long sequence = ++nextRootAdmissionSequence;
            PendingRoot root = new(
                sequence,
                NormalizeCauseIdentity(causeIdentity),
                producer);
            pendingRoots.Enqueue(root);

            bool deferred = processingRoots || State != EncounterTerminalCoordinatorState.Idle;
            if (!deferred)
            {
                ProcessPendingRoots();
            }

            return new CombatRootAdmissionResult(
                deferred
                    ? CombatRootAdmissionDisposition.Deferred
                    : CombatRootAdmissionDisposition.Executed,
                sequence,
                State);
        }

        public void Cancel()
        {
            if (State == EncounterTerminalCoordinatorState.Cancelled)
            {
                return;
            }

            if (State != EncounterTerminalCoordinatorState.TerminalClosed
                && State != EncounterTerminalCoordinatorState.Faulted)
            {
                State = EncounterTerminalCoordinatorState.Cancelled;
            }

            InvalidateAuthority();
        }

        public void Dispose()
        {
            Cancel();
            if (playerHealth != null)
            {
                playerHealth.UnbindMutationAuthority(this);
            }

            if (bossHealth != null)
            {
                bossHealth.UnbindMutationAuthority(this);
            }

            playerHealth = null;
            bossHealth = null;
        }

        bool ICombatHealthMutationAuthority.TryApplyDamage(
            CombatHealth target,
            DamageInfo damageInfo)
        {
            if (!OwnsSubject(target))
            {
                Fault(
                    EncounterTerminalDiagnosticReason.InvalidSubject,
                    "A damage request targeted a health subject outside the bound player/boss pair.");
                return false;
            }

            if (State == EncounterTerminalCoordinatorState.Open
                || State == EncounterTerminalCoordinatorState.Draining)
            {
                return TryQueueDamage(activeToken, target, damageInfo, out _);
            }

            if (State != EncounterTerminalCoordinatorState.Idle)
            {
                return false;
            }

            DamageMutation mutation = null;
            AdmitRoot(ResolveDamageCauseIdentity(damageInfo), context =>
            {
                TryQueueDamage(context, target, damageInfo, out mutation);
            });
            return mutation != null && mutation.Executed && mutation.Applied;
        }

        bool ICombatHealthMutationAuthority.IsAuthorizedDamageMutation(CombatHealth target)
        {
            return State == EncounterTerminalCoordinatorState.Draining
                && executingMutation
                && activeMutationTarget == target
                && OwnsSubject(target)
                && activeToken.IsValid;
        }

        void ICombatHealthMutationAuthority.ReportDirectMutationBypass(CombatHealth target)
        {
            Fault(
                EncounterTerminalDiagnosticReason.DirectMutationBypass,
                target == null
                    ? "A bound damage mutation bypassed its active authority."
                    : $"A bound damage mutation bypassed authority on '{target.name}'.");
        }

        bool ICombatHealthMutationAuthority.TryAuthorizeBoundMutation(
            CombatHealth target,
            BoundHealthMutationKind mutationKind)
        {
            if (!OwnsSubject(target))
            {
                Fault(
                    EncounterTerminalDiagnosticReason.InvalidSubject,
                    "A bound-health mutation targeted an unknown terminal subject.");
                return false;
            }

            if (State == EncounterTerminalCoordinatorState.TerminalClosed
                || State == EncounterTerminalCoordinatorState.Faulted
                || State == EncounterTerminalCoordinatorState.Cancelled)
            {
                return false;
            }

            Fault(
                EncounterTerminalDiagnosticReason.UnsupportedBoundMutation,
                $"{mutationKind} is not supported after terminal-subject binding.");
            return false;
        }

        internal bool TryQueueDamage(
            CanonicalCombatRootContext context,
            CombatHealth target,
            DamageInfo damageInfo)
        {
            return TryQueueDamage(context, target, damageInfo, out _);
        }

#if UNITY_INCLUDE_TESTS
        public bool TryQueueForeignContextForTests(
            CanonicalCombatRootContext context,
            CombatHealth target,
            DamageInfo damageInfo)
        {
            return TryQueueDamage(context, target, damageInfo, out _);
        }

        public void SetFinalizationBoundaryForTests(
            Action<EncounterTerminalResolutionCoordinator> callback)
        {
            finalizationBoundaryForTests = callback;
        }

        public void SetFinalSnapshotBoundaryForTests(
            Action<EncounterTerminalResolutionCoordinator> callback)
        {
            finalSnapshotBoundaryForTests = callback;
        }
#endif

        private bool TryQueueDamage(
            CanonicalCombatRootContext context,
            CombatHealth target,
            DamageInfo damageInfo,
            out DamageMutation mutation)
        {
            mutation = null;
            if (context == null)
            {
                Fault(
                    EncounterTerminalDiagnosticReason.TokenMismatch,
                    "A canonical damage mutation was submitted without a root context.");
                return false;
            }

            if (!context.IsOwnedBy(this))
            {
                return false;
            }

            return TryQueueDamage(context.Token, target, damageInfo, out mutation);
        }

        private bool TryQueueDamage(
            RootResolutionToken token,
            CombatHealth target,
            DamageInfo damageInfo,
            out DamageMutation mutation)
        {
            mutation = null;
            if (State == EncounterTerminalCoordinatorState.TerminalClosed
                || State == EncounterTerminalCoordinatorState.Faulted
                || State == EncounterTerminalCoordinatorState.Cancelled)
            {
                return false;
            }

            if (token.RunGeneration != runGeneration)
            {
                return false;
            }

            if (!OwnsSubject(target))
            {
                Fault(
                    EncounterTerminalDiagnosticReason.InvalidSubject,
                    "Only the bound player or boss can enter the terminal mutation queue.");
                return false;
            }

            if (!activeToken.IsValid || !token.Equals(activeToken))
            {
                Fault(
                    token.RunGeneration == runGeneration
                        && token.RootAdmissionSequence <= LastClosedRootAdmissionSequence
                        ? EncounterTerminalDiagnosticReason.ClosedToken
                        : EncounterTerminalDiagnosticReason.TokenMismatch,
                    "The root token does not match the active run, root sequence, and epoch.");
                return false;
            }

            if (!acceptingEnqueues
                || (State != EncounterTerminalCoordinatorState.Open
                    && State != EncounterTerminalCoordinatorState.Draining))
            {
                Fault(
                    EncounterTerminalDiagnosticReason.TokenMismatch,
                    "The active root queue is already sealed for finalization.");
                return false;
            }

            if (nextIntraRootSequence >= MaxMutationsPerRoot)
            {
                Fault(
                    EncounterTerminalDiagnosticReason.QueueLimitExceeded,
                    "The active root exceeded the bounded synchronous mutation limit.");
                return false;
            }

            mutation = new DamageMutation(
                token,
                ++nextIntraRootSequence,
                target,
                damageInfo);
            activeMutations.Enqueue(mutation);
            return true;
        }

        private bool CanAcceptRootAdmission()
        {
            return State == EncounterTerminalCoordinatorState.Idle
                || State == EncounterTerminalCoordinatorState.Open
                || State == EncounterTerminalCoordinatorState.Draining;
        }

        private void ProcessPendingRoots()
        {
            if (processingRoots)
            {
                return;
            }

            processingRoots = true;
            try
            {
                while (State == EncounterTerminalCoordinatorState.Idle
                    && pendingRoots.Count > 0)
                {
                    ExecuteRoot(pendingRoots.Dequeue());
                }
            }
            finally
            {
                processingRoots = false;
            }
        }

        private void ExecuteRoot(PendingRoot root)
        {
            root.Executed = true;
            activeToken = new RootResolutionToken(
                runGeneration,
                root.Sequence,
                ++nextEpoch);
            activeTokenOpenedSequence = ++nextEvidenceSequence;
            activeRootCauseIdentity = root.CauseIdentity;
            nextIntraRootSequence = 0;
            terminalCandidateDrafts.Clear();
            playerTerminalCandidate = false;
            bossTerminalCandidate = false;
            acceptingEnqueues = true;
            State = EncounterTerminalCoordinatorState.Open;

            CanonicalCombatRootContext context = new(this, activeToken);
            try
            {
                root.Producer.Invoke(context);
            }
            catch (Exception exception)
            {
                Fault(
                    EncounterTerminalDiagnosticReason.ProducerException,
                    exception.Message);
            }

            if (State == EncounterTerminalCoordinatorState.Faulted
                || State == EncounterTerminalCoordinatorState.Cancelled)
            {
                return;
            }

            State = EncounterTerminalCoordinatorState.Draining;
            while (activeMutations.Count > 0)
            {
                DamageMutation mutation = activeMutations.Dequeue();
                if (!mutation.Token.Equals(activeToken))
                {
                    Fault(
                        EncounterTerminalDiagnosticReason.TokenMismatch,
                        "Queued work carried a token from another root or epoch.");
                    return;
                }

                ExecuteDamageMutation(mutation);
                if (State == EncounterTerminalCoordinatorState.Faulted
                    || State == EncounterTerminalCoordinatorState.Cancelled)
                {
                    return;
                }
            }

            acceptingEnqueues = false;
            State = EncounterTerminalCoordinatorState.Finalizing;
#if UNITY_INCLUDE_TESTS
            Action<EncounterTerminalResolutionCoordinator> finalizationBoundary =
                finalizationBoundaryForTests;
            finalizationBoundaryForTests = null;
            if (finalizationBoundary != null)
            {
                try
                {
                    finalizationBoundary.Invoke(this);
                }
                catch (Exception exception)
                {
                    Fault(
                        EncounterTerminalDiagnosticReason.ProducerException,
                        exception.Message);
                }
            }

            if (State == EncounterTerminalCoordinatorState.Faulted
                || State == EncounterTerminalCoordinatorState.Cancelled)
            {
                return;
            }
#endif
            FinalizeActiveEpoch();
        }

        private void ExecuteDamageMutation(DamageMutation mutation)
        {
            bool wasAlive = mutation.Target != null && mutation.Target.IsAlive;
            executingMutation = true;
            activeMutationTarget = mutation.Target;
            try
            {
                mutation.Applied = mutation.Target != null
                    && mutation.Target.TryApplyDamageAuthorized(mutation.DamageInfo, this);
                mutation.Executed = true;
            }
            catch (Exception exception)
            {
                Fault(
                    EncounterTerminalDiagnosticReason.MutationException,
                    exception.Message);
            }
            finally
            {
                activeMutationTarget = null;
                executingMutation = false;
            }

            if (State == EncounterTerminalCoordinatorState.Faulted
                || State == EncounterTerminalCoordinatorState.Cancelled
                || !mutation.Applied
                || !wasAlive
                || mutation.Target == null
                || mutation.Target.IsAlive)
            {
                return;
            }

            if (mutation.Target == playerHealth)
            {
                playerTerminalCandidate = true;
                terminalCandidateDrafts.Add(new TerminalCandidateDraft(
                    mutation.IntraRootSequence,
                    ResolveMutationCauseIdentity(activeRootCauseIdentity, mutation.DamageInfo),
                    EncounterTerminalSubjectRole.Player,
                    EncounterTerminalCandidateKind.PlayerTerminal,
                    mutation.Target.CurrentHealth,
                    mutation.Target.MaxHealth,
                    EncounterTerminalSubjectState.Down,
                    ++nextEvidenceSequence));
            }
            else if (mutation.Target == bossHealth)
            {
                bossTerminalCandidate = true;
                terminalCandidateDrafts.Add(new TerminalCandidateDraft(
                    mutation.IntraRootSequence,
                    ResolveMutationCauseIdentity(activeRootCauseIdentity, mutation.DamageInfo),
                    EncounterTerminalSubjectRole.Boss,
                    EncounterTerminalCandidateKind.BossTerminal,
                    mutation.Target.CurrentHealth,
                    mutation.Target.MaxHealth,
                    EncounterTerminalSubjectState.Dead,
                    ++nextEvidenceSequence));
            }
        }

        private void FinalizeActiveEpoch()
        {
            try
            {
                FinalizeActiveEpochCore();
            }
            catch (Exception exception)
            {
                if (State == EncounterTerminalCoordinatorState.TerminalClosed)
                {
                    throw;
                }

                HasTerminalResolution = false;
                TerminalResolution = default;
                HasTerminalEpochEvidence = false;
                TerminalEpochEvidence = default;
                Fault(
                    EncounterTerminalDiagnosticReason.FinalizationException,
                    $"Terminal final snapshot/evidence construction failed: "
                    + $"{exception.GetType().Name}: {exception.Message}");
            }
        }

        private void FinalizeActiveEpochCore()
        {
            if (playerHealth == null
                || bossHealth == null
                || !playerHealth.isActiveAndEnabled
                || !bossHealth.isActiveAndEnabled
                || !playerHealth.HasMutationAuthority(this)
                || !bossHealth.HasMutationAuthority(this))
            {
                Fault(
                    EncounterTerminalDiagnosticReason.SubjectUnavailable,
                    "Both bound terminal subjects must finalize synchronously in the active epoch.");
                return;
            }

#if UNITY_INCLUDE_TESTS
            Action<EncounterTerminalResolutionCoordinator> finalSnapshotBoundary =
                finalSnapshotBoundaryForTests;
            finalSnapshotBoundaryForTests = null;
            finalSnapshotBoundary?.Invoke(this);
#endif

            float finalPlayerHealth = playerHealth.CurrentHealth;
            bool finalPlayerDown = !playerHealth.IsAlive;
            float finalBossHealth = bossHealth.CurrentHealth;
            bool finalBossDead = !bossHealth.IsAlive;

            if (finalPlayerDown != playerTerminalCandidate
                || finalBossDead != bossTerminalCandidate)
            {
                Fault(
                    EncounterTerminalDiagnosticReason.InvalidTerminalEvidence,
                    "Terminal candidates did not match the two finalized subject snapshots.");
                return;
            }

            if (!finalPlayerDown && !finalBossDead)
            {
                LastClosedRootAdmissionSequence = activeToken.RootAdmissionSequence;
                LastClosedEpoch = activeToken.Epoch;
                State = EncounterTerminalCoordinatorState.EpochClosed;
                activeToken = default;
                activeTokenOpenedSequence = 0;
                activeRootCauseIdentity = string.Empty;
                terminalCandidateDrafts.Clear();
                State = EncounterTerminalCoordinatorState.Idle;
                return;
            }

            EncounterTerminalOutcome outcome;
            EncounterTerminalReason reason;
            if (finalBossDead)
            {
                outcome = EncounterTerminalOutcome.Clear;
                reason = finalPlayerDown
                    ? EncounterTerminalReason.SimultaneousTerminalClear
                    : EncounterTerminalReason.BossTerminal;
            }
            else
            {
                outcome = EncounterTerminalOutcome.Fail;
                reason = EncounterTerminalReason.PlayerTerminal;
            }

            RootResolutionToken closedToken = activeToken;
            long closedTokenOpenedSequence = activeTokenOpenedSequence;
            string tokenId = CreateTokenId(closedToken);
            string tokenDigest = ComputeTokenDigest(closedToken, closedTokenOpenedSequence);
            var terminalResolution = new EncounterTerminalResolution(
                runGeneration,
                closedToken.RootAdmissionSequence,
                closedToken.Epoch,
                outcome,
                reason,
                finalPlayerDown,
                finalBossDead,
                finalPlayerHealth,
                finalBossHealth);

            EncounterTerminalCandidateEvidence[] candidateEvidence =
                new EncounterTerminalCandidateEvidence[terminalCandidateDrafts.Count];
            long playerCandidateSequence = 0;
            long bossCandidateSequence = 0;
            for (int i = 0; i < terminalCandidateDrafts.Count; i++)
            {
                TerminalCandidateDraft draft = terminalCandidateDrafts[i];
                candidateEvidence[i] = new EncounterTerminalCandidateEvidence(
                    draft.IntraRootSequence,
                    draft.CauseIdentity,
                    draft.SubjectRole,
                    draft.CandidateKind,
                    tokenId,
                    tokenDigest,
                    draft.ObservedCurrentHealth,
                    draft.ObservedMaxHealth,
                    draft.ObservedState,
                    draft.ObservationSequence);
                if (draft.SubjectRole == EncounterTerminalSubjectRole.Player)
                {
                    playerCandidateSequence = draft.IntraRootSequence;
                }
                else
                {
                    bossCandidateSequence = draft.IntraRootSequence;
                }
            }

            var playerSnapshot = new EncounterTerminalSubjectSnapshotEvidence(
                runGeneration,
                closedToken.RootAdmissionSequence,
                closedToken.Epoch,
                tokenId,
                tokenDigest,
                EncounterTerminalSubjectRole.Player,
                finalPlayerHealth,
                playerHealth.MaxHealth,
                finalPlayerDown
                    ? EncounterTerminalSubjectState.Down
                    : EncounterTerminalSubjectState.Alive,
                finalPlayerDown
                    ? EncounterTerminalCandidateKind.PlayerTerminal
                    : null,
                playerCandidateSequence,
                ++nextEvidenceSequence);
            var bossSnapshot = new EncounterTerminalSubjectSnapshotEvidence(
                runGeneration,
                closedToken.RootAdmissionSequence,
                closedToken.Epoch,
                tokenId,
                tokenDigest,
                EncounterTerminalSubjectRole.Boss,
                finalBossHealth,
                bossHealth.MaxHealth,
                finalBossDead
                    ? EncounterTerminalSubjectState.Dead
                    : EncounterTerminalSubjectState.Alive,
                finalBossDead
                    ? EncounterTerminalCandidateKind.BossTerminal
                    : null,
                bossCandidateSequence,
                ++nextEvidenceSequence);

            var discardedAdmissions =
                new EncounterTerminalDiscardedAdmissionEvidence[pendingRoots.Count];
            int discardedIndex = 0;
            foreach (PendingRoot pendingRoot in pendingRoots)
            {
                discardedAdmissions[discardedIndex++] =
                    new EncounterTerminalDiscardedAdmissionEvidence(
                        pendingRoot.Sequence,
                        pendingRoot.CauseIdentity);
            }

            var terminalEpochEvidence = new EncounterTerminalEpochEvidence(
                terminalResolution,
                tokenId,
                tokenDigest,
                closedTokenOpenedSequence,
                new[] { playerSnapshot, bossSnapshot },
                candidateEvidence,
                discardedAdmissions,
                ++nextEvidenceSequence);

            LastClosedRootAdmissionSequence = closedToken.RootAdmissionSequence;
            LastClosedEpoch = closedToken.Epoch;
            TerminalResolution = terminalResolution;
            HasTerminalResolution = true;
            TerminalEpochEvidence = terminalEpochEvidence;
            HasTerminalEpochEvidence = true;
            activeToken = default;
            pendingRoots.Clear();
            activeMutations.Clear();
            acceptingEnqueues = false;
            activeTokenOpenedSequence = 0;
            activeRootCauseIdentity = string.Empty;
            terminalCandidateDrafts.Clear();
            State = EncounterTerminalCoordinatorState.TerminalClosed;
            Resolved?.Invoke(TerminalResolution);
        }

        private static string NormalizeCauseIdentity(string causeIdentity)
        {
            return string.IsNullOrWhiteSpace(causeIdentity)
                ? "combat.unknown-root"
                : causeIdentity.Trim();
        }

        private static string ResolveDamageCauseIdentity(DamageInfo damageInfo)
        {
            return $"combat.damage.{damageInfo.SourceTeam}.{damageInfo.ResponsePolicy}.{damageInfo.ControlLockPolicy}";
        }

        private static string ResolveMutationCauseIdentity(
            string rootCauseIdentity,
            DamageInfo damageInfo)
        {
            return NormalizeCauseIdentity(rootCauseIdentity)
                + "/"
                + ResolveDamageCauseIdentity(damageInfo);
        }

        private static string CreateTokenId(RootResolutionToken token)
        {
            return $"encounter:{token.RunGeneration}:root:{token.RootAdmissionSequence}:epoch:{token.Epoch}:token";
        }

        private static string ComputeTokenDigest(
            RootResolutionToken token,
            long openedSequence)
        {
            StringBuilder builder = new(256);
            EncounterEvidenceDigest.Append(builder, "token.id", CreateTokenId(token));
            EncounterEvidenceDigest.Append(builder, "token.runGeneration", token.RunGeneration);
            EncounterEvidenceDigest.Append(
                builder,
                "token.rootAdmissionSequence",
                token.RootAdmissionSequence);
            EncounterEvidenceDigest.Append(builder, "token.epoch", token.Epoch);
            EncounterEvidenceDigest.Append(builder, "token.openedSequence", openedSequence);
            return EncounterEvidenceDigest.Compute(builder.ToString());
        }

        private bool OwnsSubject(CombatHealth target)
        {
            return target != null && (target == playerHealth || target == bossHealth);
        }

        private void Fault(EncounterTerminalDiagnosticReason reason, string message)
        {
            if (State == EncounterTerminalCoordinatorState.TerminalClosed
                || State == EncounterTerminalCoordinatorState.Faulted
                || State == EncounterTerminalCoordinatorState.Cancelled)
            {
                return;
            }

            long rootSequence = activeToken.RootAdmissionSequence;
            long epoch = activeToken.Epoch;
            State = EncounterTerminalCoordinatorState.Faulted;
            InvalidateAuthority();
            Diagnostic = new EncounterTerminalDiagnostic(
                reason,
                runGeneration,
                rootSequence,
                epoch,
                message);
            HasDiagnostic = true;

            if (diagnosticPublished)
            {
                return;
            }

            diagnosticPublished = true;
            DiagnosticAborted?.Invoke(Diagnostic);
        }

        private void InvalidateAuthority()
        {
            acceptingEnqueues = false;
            executingMutation = false;
            activeMutationTarget = null;
            activeToken = default;
            activeMutations.Clear();
            pendingRoots.Clear();
        }
    }

    internal static class EncounterEvidenceDigest
    {
        public static void Append(StringBuilder builder, string key, string value)
        {
            string safeValue = value ?? string.Empty;
            builder.Append(key);
            builder.Append('=');
            builder.Append(safeValue.Length.ToString(CultureInfo.InvariantCulture));
            builder.Append(':');
            builder.Append(safeValue);
            builder.Append('\n');
        }

        public static void Append(StringBuilder builder, string key, int value)
        {
            Append(builder, key, value.ToString(CultureInfo.InvariantCulture));
        }

        public static void Append(StringBuilder builder, string key, long value)
        {
            Append(builder, key, value.ToString(CultureInfo.InvariantCulture));
        }

        public static void Append(StringBuilder builder, string key, bool value)
        {
            Append(builder, key, value ? "1" : "0");
        }

        public static void AppendFloat(StringBuilder builder, string key, float value)
        {
            Append(builder, key, value.ToString("R", CultureInfo.InvariantCulture));
        }

        public static string ComputeEnvelope(string kind, string canonicalDigest)
        {
            StringBuilder builder = new(192);
            Append(builder, "envelope.kind", kind);
            Append(builder, "envelope.canonicalDigest", canonicalDigest);
            return Compute(builder.ToString());
        }

        public static string Compute(string canonicalPayload)
        {
            byte[] payload = Encoding.UTF8.GetBytes(canonicalPayload ?? string.Empty);
            byte[] hash;
            using (SHA256 sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(payload);
            }

            char[] characters = new char[hash.Length * 2];
            const string alphabet = "0123456789abcdef";
            for (int i = 0; i < hash.Length; i++)
            {
                characters[i * 2] = alphabet[hash[i] >> 4];
                characters[(i * 2) + 1] = alphabet[hash[i] & 0x0f];
            }

            return new string(characters);
        }
    }
}
