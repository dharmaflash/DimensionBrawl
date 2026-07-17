import { createHash } from "node:crypto";
import {
  closeSync,
  createReadStream,
  existsSync,
  openSync,
  readFileSync,
  readSync,
  statSync,
  writeFileSync,
} from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import { inflateRawSync } from "node:zlib";

const AUDITOR_ID = "P1B-PGR-HI3-LICENSE-SIGNAL-AUDITOR-01";
const AUDIT_ID = "P1B-PGR-HI3-LICENSE-SIGNAL-AUDIT-01";
const here = dirname(fileURLToPath(import.meta.url));
const arkRoot = "C:/Ark/SubcultureGameData";
const outputPath = join(here, "P1B_PGR_HI3_LICENSE_SIGNAL_AUDIT.json");

const repositories = [
  {
    ordinal: 0,
    repositoryId: "alt3ri-pgr-data-856a0e45",
    game: "Punishing: Gray Raven",
    upstream: "alt3ri/PGR_Data",
    upstreamUrl: "https://github.com/alt3ri/PGR_Data",
    revision: "856a0e4534d0854fa440040e961b74a97ba732e2",
    committedAt: "2026-05-29T23:28:20Z",
    snapshotDate: "2026-06-14",
    snapshotRoot: "games/punishing-gray-raven/raw/alt3ri-pgr-data/2026-06-14",
    sourceRecord: { path: "source-record.md", sizeBytes: 1118, sha256: "23cecc493fe4e69f59f73520e7da694c22ac76fc2283deb88070d165c37725ee" },
    manifest: { path: "manifest.yml", sizeBytes: 1645, sha256: "00f535d4bb159a0f9a43a824bda3e9fad721ae3074717b01d0b68c4f1e86400d" },
    archive: { path: "files/PGR_Data-master.zip", sizeBytes: 186357961, sha256: "04bc01ce0abd92b6ef49405b74f045b0a9d5b0902795088f13bd6dc19fa81a88", entryCount: 57002 },
    rootReadme: { entry: "PGR_Data-master/README.md", sizeBytes: 220, sha256: "875c9cb94feddcda015464b90c65549fb52806a6a59e49fd47816c2c18958ef7" },
    expectedLicenseLikeEntries: [],
    expectedManifestSignal: 'license: "unknown"',
    selectedContribution: { replacementCandidateOutputs: 4, helperInputFiles: 0, helperInputBytes: 0 },
    factualDisposition: "no-license-grant-detected-in-exact-snapshot",
  },
  {
    ordinal: 1,
    repositoryId: "nairieberry-hi3-data-01d7afb",
    game: "Honkai Impact 3rd",
    upstream: "nairieberry/HonkaiImpactData",
    upstreamUrl: "https://github.com/nairieberry/HonkaiImpactData",
    revision: "01d7afbaf99ff7d3d027e27fe9a4b363a2db7cc1",
    committedAt: "2021-04-10T22:17:18Z",
    snapshotDate: "2026-06-15",
    snapshotRoot: "games/honkai-impact-3rd/raw/nairieberry-honkaiimpactdata/2026-06-15",
    sourceRecord: { path: "source-record.md", sizeBytes: 938, sha256: "eaf1785b6fb0cddcecca59047d7de229a4130f5f67512c3e6839848c8286352e" },
    manifest: { path: "manifest.yml", sizeBytes: 2004, sha256: "11e0be6d8e9f431d6f16da48213001edc033673e0709358798e5efe52d000faa" },
    archive: { path: "files/nairieberry-HonkaiImpactData-master.zip", sizeBytes: 121793389, sha256: "4184868dfcb9ebf2a07060e8f5c599df31c5e01ca1c20fbdffe952d6d6cafd6d", entryCount: 7099 },
    rootReadme: { entry: "HonkaiImpactData-master/README.md", sizeBytes: 170, sha256: "e58b933deeb5ad09c95abe2cce6abcea57ccd27ff91feff3ac235860e552165a" },
    expectedLicenseLikeEntries: [],
    expectedManifestSignal: 'license: "none-detected"',
    selectedContribution: { replacementCandidateOutputs: 3, helperInputFiles: 1138, helperInputBytes: 301292992 },
    factualDisposition: "no-license-grant-detected-in-exact-snapshot",
  },
  {
    ordinal: 2,
    repositoryId: "devilpromt-hi3-data-e92b3bd",
    game: "Honkai Impact 3rd",
    upstream: "DevilProMT/BH3-Data",
    upstreamUrl: "https://github.com/DevilProMT/BH3-Data",
    revision: "e92b3bdb413e74241f6f4a417a786c2704055997",
    committedAt: "2025-01-25T04:48:13Z",
    snapshotDate: "2026-06-15",
    snapshotRoot: "games/honkai-impact-3rd/raw/devilpromt-bh3-data/2026-06-15",
    sourceRecord: { path: "source-record.md", sizeBytes: 899, sha256: "ccdd783e53d93fb078db806be964958da47ceb8a7cd88fda483e2b3d0e2d9d36" },
    manifest: { path: "manifest.yml", sizeBytes: 1939, sha256: "f0d99eb5a0d6ce8a8b5716b04248129e7220e4bdd5b989041b1f8d5412d2135c" },
    archive: { path: "files/DevilProMT-BH3-Data-main.zip", sizeBytes: 30555318, sha256: "6c9ee52e068805b1a8d4a0e7cfb0de7d75959c7a18cc5449475e740141267ea3", entryCount: 2192 },
    rootReadme: { entry: "BH3-Data-main/README.md", sizeBytes: 131, sha256: "f1056950e83518f57e0900ed81344c54b55cc606c5dd201ee8e4dcc3f051773e" },
    expectedLicenseLikeEntries: [],
    expectedManifestSignal: 'license: "none-detected"',
    selectedContribution: { replacementCandidateOutputs: 0, helperInputFiles: 371, helperInputBytes: 155164987 },
    factualDisposition: "no-license-grant-detected-in-exact-snapshot",
  },
  {
    ordinal: 3,
    repositoryId: "msktmi-hi3-data-1debfbd",
    game: "Honkai Impact 3rd",
    upstream: "MskTmi/ElysianRealm-Data",
    upstreamUrl: "https://github.com/MskTmi/ElysianRealm-Data",
    revision: "1debfbd44dc823b1864bc8a88f84c64c9a61499c",
    committedAt: "2026-06-05T12:59:23Z",
    snapshotDate: "2026-06-15",
    snapshotRoot: "games/honkai-impact-3rd/raw/msktmi-elysianrealm-data/2026-06-15",
    sourceRecord: { path: "source-record.md", sizeBytes: 909, sha256: "97d7c4a414232254b9f2f86f171e71e6a2cc616fae492129cd30e2884bfa586e" },
    manifest: { path: "manifest.yml", sizeBytes: 1970, sha256: "204584b565ad899a1bb3e3433c6a3cfb9e75a30442261662c0103888bce20437" },
    archive: { path: "files/MskTmi-ElysianRealm-Data-master.zip", sizeBytes: 49267918, sha256: "5c7a6a67c1e07803d8865cea1254416bb0d27558f2a83ecb268facc745dbe5ab", entryCount: 257 },
    rootReadme: { entry: "ElysianRealm-Data-master/README.md", sizeBytes: 12240, sha256: "491ed61583374d1144a5af0ca7960fa4c04b6bf44375476566d8c3342bd85cd1" },
    expectedLicenseLikeEntries: [
      { entry: "ElysianRealm-Data-master/LICENSE", sizeBytes: 35181, sha256: "6da1054eef20b8949622f2acc5a89c3243ff3b3d7aa8c2bb8fa5c04d15113c00" },
    ],
    expectedManifestSignal: 'license: "AGPL-3.0"',
    selectedContribution: { replacementCandidateOutputs: 0, helperInputFiles: 0, helperInputBytes: 0 },
    factualDisposition: "agpl-3.0-repository-license-signal-present-zero-selected-helper-inputs",
  },
];

function fail(message) {
  throw new Error(`${AUDITOR_ID}: ${message}`);
}

function assert(condition, message) {
  if (!condition) fail(message);
}

function sha256(bytes) {
  return createHash("sha256").update(bytes).digest("hex");
}

async function sha256File(path) {
  const hash = createHash("sha256");
  await new Promise((resolvePromise, rejectPromise) => {
    const stream = createReadStream(path);
    stream.on("data", (chunk) => hash.update(chunk));
    stream.on("error", rejectPromise);
    stream.on("end", resolvePromise);
  });
  return hash.digest("hex");
}

function canonicalize(value) {
  if (value === null || typeof value !== "object") return JSON.stringify(value);
  if (Array.isArray(value)) return `[${value.map(canonicalize).join(",")}]`;
  return `{${Object.keys(value).sort().map((key) => `${JSON.stringify(key)}:${canonicalize(value[key])}`).join(",")}}`;
}

function canonicalDigest(value) {
  return sha256(Buffer.from(canonicalize(value), "utf8"));
}

function arkPath(snapshotRoot, relativePath) {
  return join(arkRoot, ...snapshotRoot.split("/"), ...relativePath.split("/"));
}

async function readPinned(snapshotRoot, file, label) {
  const path = arkPath(snapshotRoot, file.path);
  const stat = statSync(path);
  assert(stat.size === file.sizeBytes, `${label} size changed: ${stat.size}`);
  const actualSha256 = await sha256File(path);
  assert(actualSha256 === file.sha256, `${label} SHA-256 changed: ${actualSha256}`);
  return readFileSync(path);
}

function readZipDirectory(path, expectedEntryCount) {
  const fd = openSync(path, "r");
  try {
    const size = statSync(path).size;
    const tailLength = Math.min(size, 65557);
    const tail = Buffer.alloc(tailLength);
    readSync(fd, tail, 0, tailLength, size - tailLength);
    let eocd = -1;
    for (let index = tail.length - 22; index >= 0; index -= 1) {
      if (tail.readUInt32LE(index) === 0x06054b50) {
        eocd = index;
        break;
      }
    }
    assert(eocd >= 0, `ZIP EOCD not found for ${path}`);
    const entryCount = tail.readUInt16LE(eocd + 10);
    const centralSize = tail.readUInt32LE(eocd + 12);
    const centralOffset = tail.readUInt32LE(eocd + 16);
    assert(entryCount !== 0xffff && centralSize !== 0xffffffff && centralOffset !== 0xffffffff, `ZIP64 is outside this audit contract: ${path}`);
    assert(entryCount === expectedEntryCount, `ZIP entry count changed for ${path}: ${entryCount}`);

    const central = Buffer.alloc(centralSize);
    readSync(fd, central, 0, centralSize, centralOffset);
    const entries = [];
    let cursor = 0;
    for (let ordinal = 0; ordinal < entryCount; ordinal += 1) {
      assert(central.readUInt32LE(cursor) === 0x02014b50, `central entry ${ordinal} signature changed for ${path}`);
      const flags = central.readUInt16LE(cursor + 8);
      const compressionMethod = central.readUInt16LE(cursor + 10);
      const compressedSize = central.readUInt32LE(cursor + 20);
      const uncompressedSize = central.readUInt32LE(cursor + 24);
      const nameLength = central.readUInt16LE(cursor + 28);
      const extraLength = central.readUInt16LE(cursor + 30);
      const commentLength = central.readUInt16LE(cursor + 32);
      const localHeaderOffset = central.readUInt32LE(cursor + 42);
      const name = central.subarray(cursor + 46, cursor + 46 + nameLength).toString((flags & 0x0800) !== 0 ? "utf8" : "utf8");
      entries.push({ name, compressionMethod, compressedSize, uncompressedSize, localHeaderOffset });
      cursor += 46 + nameLength + extraLength + commentLength;
    }
    assert(cursor === central.length, `central directory length changed for ${path}`);
    return { fd, entries, release: () => closeSync(fd) };
  } catch (error) {
    closeSync(fd);
    throw error;
  }
}

function extractZipEntry(directory, name) {
  const entry = directory.entries.find((candidate) => candidate.name === name);
  assert(entry, `ZIP entry missing: ${name}`);
  const header = Buffer.alloc(30);
  readSync(directory.fd, header, 0, header.length, entry.localHeaderOffset);
  assert(header.readUInt32LE(0) === 0x04034b50, `local header signature changed: ${name}`);
  const nameLength = header.readUInt16LE(26);
  const extraLength = header.readUInt16LE(28);
  const compressed = Buffer.alloc(entry.compressedSize);
  readSync(directory.fd, compressed, 0, compressed.length, entry.localHeaderOffset + 30 + nameLength + extraLength);
  const bytes = entry.compressionMethod === 0
    ? compressed
    : entry.compressionMethod === 8
      ? inflateRawSync(compressed)
      : fail(`unsupported ZIP compression method ${entry.compressionMethod}: ${name}`);
  assert(bytes.length === entry.uncompressedSize, `ZIP entry uncompressed size changed: ${name}`);
  return bytes;
}

function isLicenseLikeEntry(name) {
  return /(^|\/)(license|licence|copying|notice|copyright)([._-][^/]*)?$/i.test(name);
}

const observations = [];
for (const repository of repositories) {
  const sourceRecordBytes = await readPinned(repository.snapshotRoot, repository.sourceRecord, `${repository.repositoryId} source record`);
  const manifestBytes = await readPinned(repository.snapshotRoot, repository.manifest, `${repository.repositoryId} manifest`);
  assert(manifestBytes.toString("utf8").includes(repository.expectedManifestSignal), `${repository.repositoryId} manifest license signal changed`);

  const archivePath = arkPath(repository.snapshotRoot, repository.archive.path);
  const archiveStat = statSync(archivePath);
  assert(archiveStat.size === repository.archive.sizeBytes, `${repository.repositoryId} archive size changed`);
  const archiveSha256 = await sha256File(archivePath);
  assert(archiveSha256 === repository.archive.sha256, `${repository.repositoryId} archive SHA-256 changed: ${archiveSha256}`);
  const directory = readZipDirectory(archivePath, repository.archive.entryCount);
  try {
    const detectedLicenseEntries = directory.entries.filter((entry) => isLicenseLikeEntry(entry.name)).map((entry) => entry.name).sort();
    const expectedLicenseEntries = repository.expectedLicenseLikeEntries.map((entry) => entry.entry).sort();
    assert(JSON.stringify(detectedLicenseEntries) === JSON.stringify(expectedLicenseEntries), `${repository.repositoryId} license-like entry set changed`);

    const readmeBytes = extractZipEntry(directory, repository.rootReadme.entry);
    assert(readmeBytes.length === repository.rootReadme.sizeBytes, `${repository.repositoryId} README size changed`);
    assert(sha256(readmeBytes) === repository.rootReadme.sha256, `${repository.repositoryId} README SHA-256 changed`);
    const readmeLicenseSignalCount = (readmeBytes.toString("utf8").match(/license|licence|copyright|spdx/gi) ?? []).length;
    assert(readmeLicenseSignalCount === 0, `${repository.repositoryId} README license signal changed`);

    for (const expected of repository.expectedLicenseLikeEntries) {
      const licenseBytes = extractZipEntry(directory, expected.entry);
      assert(licenseBytes.length === expected.sizeBytes, `${repository.repositoryId} license size changed`);
      assert(sha256(licenseBytes) === expected.sha256, `${repository.repositoryId} license SHA-256 changed`);
    }

    observations.push({
      ordinal: repository.ordinal,
      repositoryId: repository.repositoryId,
      game: repository.game,
      upstream: repository.upstream,
      upstreamUrl: repository.upstreamUrl,
      revision: repository.revision,
      committedAt: repository.committedAt,
      snapshotDate: repository.snapshotDate,
      snapshotRoot: repository.snapshotRoot,
      sourceRecord: repository.sourceRecord,
      manifest: repository.manifest,
      archive: repository.archive,
      rootReadme: { ...repository.rootReadme, licenseSignalCount: readmeLicenseSignalCount },
      licenseLikeEntries: repository.expectedLicenseLikeEntries,
      manifestLicenseSignal: repository.expectedManifestSignal,
      selectedContribution: repository.selectedContribution,
      factualDisposition: repository.factualDisposition,
      admissionEffect: "none",
    });
  } finally {
    directory.release();
  }

  assert(sourceRecordBytes.length === repository.sourceRecord.sizeBytes, `${repository.repositoryId} source record read changed`);
}

const auditWithoutDigest = {
  schemaVersion: 1,
  auditId: AUDIT_ID,
  status: "pass-factual-license-signal-inventory-admission-unchanged",
  recordedAt: "2026-07-16T05:20:00+09:00",
  scope: "Exact retained repository snapshots used by the PGR four-output replacement, HI3 three-output replacement, and HI3 two-helper provenance candidates.",
  observations,
  verifiedCounts: {
    repositories: observations.length,
    archivesWithNoLicenseLikeEntry: observations.filter((observation) => observation.licenseLikeEntries.length === 0).length,
    archivesWithExplicitLicenseLikeEntry: observations.filter((observation) => observation.licenseLikeEntries.length > 0).length,
    replacementCandidateOutputs: observations.reduce((sum, observation) => sum + observation.selectedContribution.replacementCandidateOutputs, 0),
    helperInputFiles: observations.reduce((sum, observation) => sum + observation.selectedContribution.helperInputFiles, 0),
    helperInputBytes: observations.reduce((sum, observation) => sum + observation.selectedContribution.helperInputBytes, 0),
  },
  admissionState: {
    verifiedSupportingCandidates: 9,
    replacementCandidatesOutsidePacketInScope: 7,
    historicalHelperSourcesInsidePacketInScope: 2,
    formalHelperAdmissions: 0,
    admittedSupportingSources: 0,
    requiredSupportingSources: 9,
    liveRows: 0,
    liveCrosswalkCells: 0,
    productAdoptionEffect: "none",
    elevenSourceAtomicAdmissionEffect: "none",
    licenseDisposition: "policy-or-rights-review-required-before-any-admission",
  },
  boundaries: [
    "This is a factual signal inventory, not legal advice and not a determination of copyright ownership, fair use, or permission.",
    "No LICENSE-like file or README license signal means only that no explicit repository grant was detected in the exact retained snapshot; it does not create permission.",
    "The MskTmi AGPL-3.0 repository signal does not establish rights to third-party game data or media, and that snapshot contributes zero selected helper inputs.",
    "No repository bytes, authored strings, opaque IDs, formulas, or media are promoted into DimensionBrawl product assets by this audit.",
    "The seven versioned replacement candidates remain outside packet.inScopeSourceIds. The two retained historical helper source IDs are already in packet.inScopeSourceIds, but their formal admission remains open; none of the nine supporting candidates is admitted.",
    "All live claims and crosswalk cells remain empty until an explicit policy or rights review and the atomic gate both pass.",
  ],
  nextDecision: "Keep candidate provenance for internal structural comparison, retain admission at zero, and obtain an explicit policy/rights disposition or replace the evidence lineage with an admissible source before atomic LiveAcceptance.",
};
const audit = { ...auditWithoutDigest, canonicalAuditDigest: canonicalDigest(auditWithoutDigest) };
const outputText = `${JSON.stringify(audit, null, 2)}\n`;
assert(!outputText.includes("\r") && outputText.endsWith("\n") && !outputText.endsWith("\n\n"), "output normalization changed");

if (process.argv.includes("--verify")) {
  assert(existsSync(outputPath), "audit output is missing");
  assert(readFileSync(outputPath, "utf8") === outputText, "audit output bytes differ from reconstruction");
  console.log(`PASS ${AUDIT_ID}`);
} else {
  writeFileSync(outputPath, outputText, "utf8");
  console.log(`WROTE ${AUDIT_ID}`);
}
console.log(`repositories=${observations.length} noLicenseSignal=3 explicitLicenseSignal=1`);
console.log("verifiedCandidates=9 admittedSupporting=0 liveRows=0 liveCells=0 admissionEffect=none");
console.log(`canonicalAuditDigest=${audit.canonicalAuditDigest}`);
console.log(`auditSizeBytes=${Buffer.byteLength(outputText, "utf8")}`);
console.log(`auditSha256=${sha256(Buffer.from(outputText, "utf8"))}`);
