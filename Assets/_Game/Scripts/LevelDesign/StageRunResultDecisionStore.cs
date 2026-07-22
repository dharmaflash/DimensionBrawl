using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace DimensionBrawl.LevelDesign
{
    internal enum StageRunResultCommitStoreFailureKind
    {
        None = 0,
        RecoveryPending = 1,
        Conflict = 2,
        Corrupt = 3
    }

    internal static class StageRunResultCommitStore
    {
        [Serializable]
        private sealed class StoredResultCommitDecision
        {
            public int schemaVersion;
            public string runId;
            public string playableStageId;
            public int routeRevision;
            public string routeDigest;
            public string resultSummaryDigest;
            public string terminalFinalizationOwnerCoverageRecordId;
            public string terminalFinalizationOwnerCoverageDigest;
            public int preparationKind;
            public string preparationDigest;
            public string commitReceiptId;
            public long summaryCommittedAtSequence;
            public string resultCommitReceiptDigest;
            public string receiptEnvelopeChecksum;
            public string decisionEnvelopeChecksum;
        }

        private sealed class CachedCommit
        {
            public CachedCommit(
                StageRunResultSummary summary,
                TerminalFinalizationOwnerCoverageRecord coverage,
                StageRunResultCommitReceipt receipt)
            {
                Summary = summary;
                Coverage = coverage;
                Receipt = receipt;
            }

            public StageRunResultSummary Summary { get; }
            public TerminalFinalizationOwnerCoverageRecord Coverage { get; }
            public StageRunResultCommitReceipt Receipt { get; }
        }

        private const int SchemaVersion = 2;
        private const long FirstCommitSequence = 1;
        private const string DecisionFileExtension = ".result-decision.json";
        private static readonly UTF8Encoding Utf8NoBom = new(false);
        private static readonly object Gate = new();
        private static readonly Dictionary<string, CachedCommit> Cache =
            new(StringComparer.Ordinal);
        private static string storageRootOverride;
#if UNITY_INCLUDE_TESTS
        private static int injectedTransientWriteFailureCount;
        private static int injectedTransientReadFailureCount;
#endif

        public static void ConfigureProductionStorage()
        {
            lock (Gate)
            {
                storageRootOverride = null;
                Cache.Clear();
#if UNITY_INCLUDE_TESTS
                injectedTransientWriteFailureCount = 0;
                injectedTransientReadFailureCount = 0;
#endif
            }
        }

        public static void ClearMemoryCache()
        {
            lock (Gate)
            {
                Cache.Clear();
            }
        }

        public static bool TryCommit(
            StageRunResultSummary candidate,
            TerminalFinalizationOwnerCoverageRecord coverage,
            StageRunResultCommitPreparation preparation,
            out StageRunResultSummary storedSummary,
            out TerminalFinalizationOwnerCoverageRecord storedCoverage,
            out StageRunResultCommitReceipt receipt,
            out StageRunResultCommitStoreFailureKind failureKind,
            out string error)
        {
            storedSummary = null;
            storedCoverage = null;
            receipt = null;
            failureKind = StageRunResultCommitStoreFailureKind.None;
            error = string.Empty;
            if (!ValidateCandidate(candidate, coverage, preparation, out error))
            {
                failureKind = StageRunResultCommitStoreFailureKind.Conflict;
                return false;
            }

            lock (Gate)
            {
                string runId = candidate.Identity.RunId;
                if (Cache.TryGetValue(runId, out CachedCommit cached))
                {
                    if (!MatchesCandidate(
                        cached.Receipt,
                        candidate,
                        coverage,
                        preparation))
                    {
                        failureKind = StageRunResultCommitStoreFailureKind.Conflict;
                        error = "The run ID already owns a different committed result comparison value.";
                        return false;
                    }

                    storedSummary = cached.Summary;
                    storedCoverage = cached.Coverage;
                    receipt = cached.Receipt;
                    return true;
                }

                string decisionPath = GetDecisionPath(runId);
                if (File.Exists(decisionPath))
                {
                    return TryUseStoredDecision(
                        decisionPath,
                        candidate,
                        coverage,
                        preparation,
                        out storedSummary,
                        out storedCoverage,
                        out receipt,
                        out failureKind,
                        out error);
                }

                var candidateReceipt = new StageRunResultCommitReceipt(
                    candidate,
                    coverage,
                    preparation,
                    FirstCommitSequence);
                StoredResultCommitDecision decision = CreateDecision(candidateReceipt);
                if (!TryWriteDecisionAtomically(
                    decisionPath,
                    decision,
                    out bool destinationMayExist,
                    out error))
                {
                    if (destinationMayExist && File.Exists(decisionPath))
                    {
                        return TryUseStoredDecision(
                            decisionPath,
                            candidate,
                            coverage,
                            preparation,
                            out storedSummary,
                            out storedCoverage,
                            out receipt,
                            out failureKind,
                            out error);
                    }

                    failureKind = StageRunResultCommitStoreFailureKind.RecoveryPending;
                    return false;
                }

                return TryUseStoredDecision(
                    decisionPath,
                    candidate,
                    coverage,
                    preparation,
                    out storedSummary,
                    out storedCoverage,
                    out receipt,
                    out failureKind,
                    out error);
            }
        }

        public static bool TryReadReceipt(
            string runId,
            out StageRunResultCommitReceipt receipt,
            out StageRunResultCommitStoreFailureKind failureKind,
            out string error)
        {
            receipt = null;
            failureKind = StageRunResultCommitStoreFailureKind.None;
            error = string.Empty;
            if (!IsSafeRunId(runId))
            {
                failureKind = StageRunResultCommitStoreFailureKind.Conflict;
                error = "Result commit lookup run ID is invalid.";
                return false;
            }

            lock (Gate)
            {
                if (Cache.TryGetValue(runId, out CachedCommit cached))
                {
                    if (cached.Receipt != null
                        && string.Equals(cached.Receipt.RunId, runId, StringComparison.Ordinal))
                    {
                        receipt = cached.Receipt;
                        return true;
                    }

                    failureKind = StageRunResultCommitStoreFailureKind.Conflict;
                    error = "Cached result commit decision does not match the requested run ID.";
                    return false;
                }

                string path = GetDecisionPath(runId);
                if (!File.Exists(path))
                {
                    error = "No durable result commit decision exists for this run ID.";
                    return false;
                }

                if (!TryReadDecision(path, out _, out receipt, out failureKind, out error))
                {
                    return false;
                }

                if (!string.Equals(receipt.RunId, runId, StringComparison.Ordinal))
                {
                    receipt = null;
                    failureKind = StageRunResultCommitStoreFailureKind.Conflict;
                    error = "Durable result commit decision does not match the requested run ID.";
                    return false;
                }

                return true;
            }
        }

        private static bool TryUseStoredDecision(
            string path,
            StageRunResultSummary candidate,
            TerminalFinalizationOwnerCoverageRecord coverage,
            StageRunResultCommitPreparation preparation,
            out StageRunResultSummary storedSummary,
            out TerminalFinalizationOwnerCoverageRecord storedCoverage,
            out StageRunResultCommitReceipt receipt,
            out StageRunResultCommitStoreFailureKind failureKind,
            out string error)
        {
            storedSummary = null;
            storedCoverage = null;
            receipt = null;
            if (!TryReadDecision(path, out _, out receipt, out failureKind, out error))
            {
                return false;
            }

            if (!MatchesCandidate(receipt, candidate, coverage, preparation))
            {
                receipt = null;
                failureKind = StageRunResultCommitStoreFailureKind.Conflict;
                error = "The durable run slot contains a different committed comparison value.";
                return false;
            }

            var cached = new CachedCommit(candidate, coverage, receipt);
            Cache[candidate.Identity.RunId] = cached;
            storedSummary = cached.Summary;
            storedCoverage = cached.Coverage;
            return true;
        }

        private static bool TryReadDecision(
            string path,
            out StoredResultCommitDecision decision,
            out StageRunResultCommitReceipt receipt,
            out StageRunResultCommitStoreFailureKind failureKind,
            out string error)
        {
            decision = null;
            receipt = null;
            failureKind = StageRunResultCommitStoreFailureKind.None;
            error = string.Empty;
#if UNITY_INCLUDE_TESTS
            if (injectedTransientReadFailureCount > 0)
            {
                injectedTransientReadFailureCount--;
                failureKind = StageRunResultCommitStoreFailureKind.RecoveryPending;
                error = "Injected transient result decision read failure.";
                return false;
            }
#endif
            try
            {
                string json = File.ReadAllText(path, Utf8NoBom);
                decision = JsonUtility.FromJson<StoredResultCommitDecision>(json);
            }
            catch (IOException exception)
            {
                failureKind = StageRunResultCommitStoreFailureKind.RecoveryPending;
                error = $"Result commit decision read is temporarily unavailable: {exception.Message}";
                return false;
            }
            catch (UnauthorizedAccessException exception)
            {
                failureKind = StageRunResultCommitStoreFailureKind.RecoveryPending;
                error = $"Result commit decision read is not currently authorized: {exception.Message}";
                return false;
            }
            catch (Exception exception)
            {
                failureKind = StageRunResultCommitStoreFailureKind.Corrupt;
                error = $"Result commit decision JSON is invalid: {exception.Message}";
                return false;
            }

            if (!TryValidateDecision(decision, out receipt, out error))
            {
                failureKind = StageRunResultCommitStoreFailureKind.Corrupt;
                return false;
            }

            return true;
        }

        private static bool TryValidateDecision(
            StoredResultCommitDecision decision,
            out StageRunResultCommitReceipt receipt,
            out string error)
        {
            receipt = null;
            error = string.Empty;
            if (decision == null
                || decision.schemaVersion != SchemaVersion
                || !IsSafeRunId(decision.runId)
                || string.IsNullOrWhiteSpace(decision.playableStageId)
                || decision.routeRevision <= 0
                || string.IsNullOrWhiteSpace(decision.routeDigest)
                || string.IsNullOrWhiteSpace(decision.resultSummaryDigest)
                || string.IsNullOrWhiteSpace(
                    decision.terminalFinalizationOwnerCoverageRecordId)
                || string.IsNullOrWhiteSpace(
                    decision.terminalFinalizationOwnerCoverageDigest)
                || decision.preparationKind != (int)StageRunResultCommitPreparationKind.NotRequired
                || !string.Equals(
                    decision.preparationDigest,
                    StageRunResultCommitPreparation.NotRequired.CanonicalDigest,
                    StringComparison.Ordinal)
                || decision.summaryCommittedAtSequence <= 0)
            {
                error = "Result commit decision has an invalid schema or required field.";
                return false;
            }

            string expectedDecisionChecksum = ComputeDecisionEnvelopeChecksum(decision);
            if (!string.Equals(
                decision.decisionEnvelopeChecksum,
                expectedDecisionChecksum,
                StringComparison.Ordinal))
            {
                error = "Result commit decision envelope checksum mismatch.";
                return false;
            }

            receipt = new StageRunResultCommitReceipt(
                decision.schemaVersion,
                decision.commitReceiptId,
                decision.runId,
                decision.playableStageId,
                decision.routeRevision,
                decision.routeDigest,
                decision.resultSummaryDigest,
                decision.terminalFinalizationOwnerCoverageRecordId,
                decision.terminalFinalizationOwnerCoverageDigest,
                StageRunResultCommitPreparation.NotRequired,
                decision.summaryCommittedAtSequence,
                decision.resultCommitReceiptDigest,
                decision.receiptEnvelopeChecksum);
            if (!receipt.HasValidIntegrity())
            {
                receipt = null;
                error = "Result commit receipt integrity check failed.";
                return false;
            }

            return true;
        }

        private static StoredResultCommitDecision CreateDecision(
            StageRunResultCommitReceipt receipt)
        {
            var decision = new StoredResultCommitDecision
            {
                schemaVersion = receipt.SchemaVersion,
                runId = receipt.RunId,
                playableStageId = receipt.PlayableStageId,
                routeRevision = receipt.RouteRevision,
                routeDigest = receipt.RouteDigest,
                resultSummaryDigest = receipt.ResultSummaryDigest,
                terminalFinalizationOwnerCoverageRecordId =
                    receipt.TerminalFinalizationOwnerCoverageRecordId,
                terminalFinalizationOwnerCoverageDigest =
                    receipt.TerminalFinalizationOwnerCoverageDigest,
                preparationKind = (int)receipt.Preparation.Kind,
                preparationDigest = receipt.Preparation.CanonicalDigest,
                commitReceiptId = receipt.CommitReceiptId,
                summaryCommittedAtSequence = receipt.SummaryCommittedAtSequence,
                resultCommitReceiptDigest = receipt.CanonicalDigest,
                receiptEnvelopeChecksum = receipt.EnvelopeChecksum
            };
            decision.decisionEnvelopeChecksum = ComputeDecisionEnvelopeChecksum(decision);
            return decision;
        }

        private static string ComputeDecisionEnvelopeChecksum(
            StoredResultCommitDecision decision)
        {
            StringBuilder builder = new(1024);
            StageCanonicalDigest.Append(builder, "decision.schemaVersion", decision?.schemaVersion ?? 0);
            StageCanonicalDigest.Append(builder, "decision.runId", decision?.runId);
            StageCanonicalDigest.Append(builder, "decision.playableStageId", decision?.playableStageId);
            StageCanonicalDigest.Append(builder, "decision.routeRevision", decision?.routeRevision ?? 0);
            StageCanonicalDigest.Append(builder, "decision.routeDigest", decision?.routeDigest);
            StageCanonicalDigest.Append(builder, "decision.resultSummaryDigest", decision?.resultSummaryDigest);
            StageCanonicalDigest.Append(
                builder,
                "decision.terminalFinalizationOwnerCoverageRecordId",
                decision?.terminalFinalizationOwnerCoverageRecordId);
            StageCanonicalDigest.Append(
                builder,
                "decision.terminalFinalizationOwnerCoverageDigest",
                decision?.terminalFinalizationOwnerCoverageDigest);
            StageCanonicalDigest.Append(builder, "decision.preparationKind", decision?.preparationKind ?? -1);
            StageCanonicalDigest.Append(builder, "decision.preparationDigest", decision?.preparationDigest);
            StageCanonicalDigest.Append(builder, "decision.commitReceiptId", decision?.commitReceiptId);
            StageCanonicalDigest.Append(
                builder,
                "decision.summaryCommittedAtSequence",
                decision?.summaryCommittedAtSequence ?? 0L);
            StageCanonicalDigest.Append(
                builder,
                "decision.resultCommitReceiptDigest",
                decision?.resultCommitReceiptDigest);
            StageCanonicalDigest.Append(
                builder,
                "decision.receiptEnvelopeChecksum",
                decision?.receiptEnvelopeChecksum);
            return StageCanonicalDigest.Compute(builder.ToString());
        }

        private static bool TryWriteDecisionAtomically(
            string destinationPath,
            StoredResultCommitDecision decision,
            out bool destinationMayExist,
            out string error)
        {
            destinationMayExist = false;
            error = string.Empty;
#if UNITY_INCLUDE_TESTS
            if (injectedTransientWriteFailureCount > 0)
            {
                injectedTransientWriteFailureCount--;
                error = "Injected transient result decision write failure.";
                return false;
            }
#endif
            string temporaryPath = destinationPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                string directory = Path.GetDirectoryName(destinationPath);
                if (string.IsNullOrWhiteSpace(directory))
                {
                    error = "Result commit decision directory could not be resolved.";
                    return false;
                }

                Directory.CreateDirectory(directory);
                byte[] bytes = Utf8NoBom.GetBytes(JsonUtility.ToJson(decision, false));
                using (var stream = new FileStream(
                    temporaryPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }

                try
                {
                    File.Move(temporaryPath, destinationPath);
                }
                catch (IOException) when (File.Exists(destinationPath))
                {
                    destinationMayExist = true;
                    error = "A concurrent result commit created the run decision slot.";
                    return false;
                }

                destinationMayExist = true;
                return true;
            }
            catch (IOException exception)
            {
                destinationMayExist = File.Exists(destinationPath);
                error = $"Result commit decision write is temporarily unavailable: {exception.Message}";
                return false;
            }
            catch (UnauthorizedAccessException exception)
            {
                destinationMayExist = File.Exists(destinationPath);
                error = $"Result commit decision write is not currently authorized: {exception.Message}";
                return false;
            }
            finally
            {
                try
                {
                    if (File.Exists(temporaryPath))
                    {
                        File.Delete(temporaryPath);
                    }
                }
                catch
                {
                }
            }
        }

        private static bool MatchesCandidate(
            StageRunResultCommitReceipt receipt,
            StageRunResultSummary candidate,
            TerminalFinalizationOwnerCoverageRecord coverage,
            StageRunResultCommitPreparation preparation)
        {
            return receipt != null
                && candidate != null
                && coverage != null
                && preparation != null
                && string.Equals(receipt.RunId, candidate.Identity.RunId, StringComparison.Ordinal)
                && string.Equals(
                    receipt.PlayableStageId,
                    candidate.Identity.PlayableStageId,
                    StringComparison.Ordinal)
                && receipt.RouteRevision == candidate.Identity.RouteRevision
                && string.Equals(
                    receipt.RouteDigest,
                    candidate.Identity.RouteSnapshotDigest,
                    StringComparison.Ordinal)
                && string.Equals(
                    receipt.ResultSummaryDigest,
                    candidate.ResultSummaryDigest,
                    StringComparison.Ordinal)
                && string.Equals(
                    receipt.TerminalFinalizationOwnerCoverageRecordId,
                    coverage.TerminalFinalizationOwnerCoverageRecordId,
                    StringComparison.Ordinal)
                && string.Equals(
                    receipt.TerminalFinalizationOwnerCoverageDigest,
                    coverage.CanonicalDigest,
                    StringComparison.Ordinal)
                && receipt.Preparation.Kind == preparation.Kind
                && string.Equals(
                    receipt.Preparation.CanonicalDigest,
                    preparation.CanonicalDigest,
                    StringComparison.Ordinal);
        }

        private static bool ValidateCandidate(
            StageRunResultSummary candidate,
            TerminalFinalizationOwnerCoverageRecord coverage,
            StageRunResultCommitPreparation preparation,
            out string error)
        {
            error = string.Empty;
            if (candidate == null || coverage == null || preparation == null)
            {
                error = "Result candidate, owner coverage, or commit preparation is missing.";
                return false;
            }

            if (!IsSafeRunId(candidate.Identity.RunId)
                || !string.Equals(coverage.RunId, candidate.Identity.RunId, StringComparison.Ordinal)
                || !string.Equals(
                    coverage.PlayableStageId,
                    candidate.Identity.PlayableStageId,
                    StringComparison.Ordinal)
                || coverage.RouteRevision != candidate.Identity.RouteRevision
                || !string.Equals(
                    coverage.RouteSnapshotDigest,
                    candidate.Identity.RouteSnapshotDigest,
                    StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(coverage.TerminalFinalizationAuthorityId)
                || string.IsNullOrWhiteSpace(coverage.TerminalFinalizationAuthorityDigest)
                || coverage.OwnerRowCount != 4
                || coverage.PendingFinalizationOwnerCount != 0
                || !coverage.ZeroPendingFinalizationOwners
                || preparation.Kind != StageRunResultCommitPreparationKind.NotRequired)
            {
                error = "Result commit comparison value does not match the immutable run candidate.";
                return false;
            }

            return true;
        }

        private static string GetDecisionPath(string runId)
        {
            return Path.Combine(GetStorageRoot(), runId + DecisionFileExtension);
        }

        private static string GetStorageRoot()
        {
            if (!string.IsNullOrWhiteSpace(storageRootOverride))
            {
                return storageRootOverride;
            }

            return Path.Combine(
                Application.persistentDataPath,
                "DimensionBrawl",
                "StageRunResultDecisions");
        }

        private static bool IsSafeRunId(string runId)
        {
            if (string.IsNullOrWhiteSpace(runId) || runId.Length > 96)
            {
                return false;
            }

            for (int i = 0; i < runId.Length; i++)
            {
                char value = runId[i];
                if (!char.IsLetterOrDigit(value) && value != '-' && value != '_')
                {
                    return false;
                }
            }

            return true;
        }

#if UNITY_INCLUDE_TESTS
        public static void ConfigureIsolatedTestStorage()
        {
            lock (Gate)
            {
                storageRootOverride = Path.Combine(
                    Application.temporaryCachePath,
                    "DimensionBrawlTests",
                    "StageRunResultDecisions",
                    Guid.NewGuid().ToString("N"));
                Cache.Clear();
                injectedTransientWriteFailureCount = 0;
                injectedTransientReadFailureCount = 0;
            }
        }

        public static void InjectTransientIoFailuresForTests(
            int writeFailureCount,
            int readFailureCount)
        {
            lock (Gate)
            {
                injectedTransientWriteFailureCount = Math.Max(0, writeFailureCount);
                injectedTransientReadFailureCount = Math.Max(0, readFailureCount);
            }
        }

        public static string GetDecisionPathForTests(string runId)
        {
            lock (Gate)
            {
                return GetDecisionPath(runId);
            }
        }

        public static bool SeedConflictingDecisionForTests(
            StageRunIdentity identity,
            out string error)
        {
            error = string.Empty;
            if (identity == null || !IsSafeRunId(identity.RunId))
            {
                error = "A valid run identity is required to seed a conflict.";
                return false;
            }

            lock (Gate)
            {
                string path = GetDecisionPath(identity.RunId);
                var draft = new StageRunResultCommitReceipt(
                    SchemaVersion,
                    $"{identity.RunId}:result-commit:1",
                    identity.RunId,
                    identity.PlayableStageId,
                    identity.RouteRevision,
                    identity.RouteSnapshotDigest,
                    new string('a', 64),
                    $"{identity.RunId}:conflicting-owner-coverage",
                    new string('b', 64),
                    StageRunResultCommitPreparation.NotRequired,
                    FirstCommitSequence,
                    string.Empty,
                    string.Empty);
                string canonicalDigest = draft.ComputeCanonicalDigest();
                var checksummedDraft = new StageRunResultCommitReceipt(
                    draft.SchemaVersion,
                    draft.CommitReceiptId,
                    draft.RunId,
                    draft.PlayableStageId,
                    draft.RouteRevision,
                    draft.RouteDigest,
                    draft.ResultSummaryDigest,
                    draft.TerminalFinalizationOwnerCoverageRecordId,
                    draft.TerminalFinalizationOwnerCoverageDigest,
                    draft.Preparation,
                    draft.SummaryCommittedAtSequence,
                    canonicalDigest,
                    string.Empty);
                string envelopeChecksum = checksummedDraft.ComputeEnvelopeChecksum();
                var conflictingReceipt = new StageRunResultCommitReceipt(
                    checksummedDraft.SchemaVersion,
                    checksummedDraft.CommitReceiptId,
                    checksummedDraft.RunId,
                    checksummedDraft.PlayableStageId,
                    checksummedDraft.RouteRevision,
                    checksummedDraft.RouteDigest,
                    checksummedDraft.ResultSummaryDigest,
                    checksummedDraft.TerminalFinalizationOwnerCoverageRecordId,
                    checksummedDraft.TerminalFinalizationOwnerCoverageDigest,
                    checksummedDraft.Preparation,
                    checksummedDraft.SummaryCommittedAtSequence,
                    checksummedDraft.CanonicalDigest,
                    envelopeChecksum);
                StoredResultCommitDecision decision = CreateDecision(conflictingReceipt);
                Cache.Remove(identity.RunId);
                return TryWriteDecisionAtomically(path, decision, out _, out error);
            }
        }

        public static bool SeedCorruptDecisionForTests(string runId, out string error)
        {
            error = string.Empty;
            if (!IsSafeRunId(runId))
            {
                error = "A valid run ID is required to seed a corrupt decision.";
                return false;
            }

            lock (Gate)
            {
                string path = GetDecisionPath(runId);
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllText(path, "{\"schemaVersion\":1}", Utf8NoBom);
                    Cache.Remove(runId);
                    return true;
                }
                catch (Exception exception)
                {
                    error = exception.Message;
                    return false;
                }
            }
        }
#endif
    }
}
