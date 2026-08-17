using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace DimensionBrawl.Editor.AuditionPV.Tests
{
    public sealed class AuditionPvEndCardPlaceholderProducerTests
    {
        private string temporaryRoot;
        private string projectRoot;

        [SetUp]
        public void SetUp()
        {
            temporaryRoot = Path.Combine(
                Path.GetTempPath(),
                "DimensionBrawl_S100_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temporaryRoot);
            DirectoryInfo parent = Directory.GetParent(Path.GetFullPath(Application.dataPath));
            Assert.That(parent, Is.Not.Null);
            projectRoot = parent.FullName;
        }

        [TearDown]
        public void TearDown()
        {
            if (!string.IsNullOrWhiteSpace(temporaryRoot) &&
                Directory.Exists(temporaryRoot))
                Directory.Delete(temporaryRoot, recursive: true);
        }

        [Test]
        public void Contract_MatchesS100EditStartGateWithoutClaimingFinalWording()
        {
            Assert.That(
                AuditionPvEndCardPlaceholderProducer.Width,
                Is.EqualTo(AuditionPvSixtySecondGateManifestValidator.Width));
            Assert.That(
                AuditionPvEndCardPlaceholderProducer.Height,
                Is.EqualTo(AuditionPvSixtySecondGateManifestValidator.Height));
            Assert.That(
                AuditionPvEndCardPlaceholderProducer.GraphicSourceId,
                Is.EqualTo("layout-placeholder"));
            Assert.That(
                AuditionPvEndCardPlaceholderProducer.GraphicProductionStatus,
                Is.EqualTo("layout-placeholder-approved"));
            Assert.That(
                AuditionPvEndCardPlaceholderProducer.PendingApprovalStatus,
                Is.EqualTo("pending-approval"));
            Assert.That(
                AuditionPvEndCardPlaceholderProducer.GateRelativeArtifactPath,
                Is.EqualTo("graphics/" +
                    AuditionPvEndCardPlaceholderProducer.OutputFileName));

            Assert.That(
                AuditionPvEndCardPlaceholderProducer.LayoutZones
                    .Single(value => value.zoneId == "slogan").disposition,
                Is.EqualTo("pending-approval-ae-wording"));
            Assert.That(
                AuditionPvEndCardPlaceholderProducer.LayoutZones
                    .Single(value => value.zoneId == "audition-notice").disposition,
                Is.EqualTo("pending-approval-ae-wording"));
        }

        [Test]
        public void Produce_WritesPhysicalQhdPngAndIsByteDeterministic()
        {
            AuditionPvEndCardPlaceholderResult first =
                AuditionPvEndCardPlaceholderProducer.Produce(
                    projectRoot,
                    temporaryRoot);
            string firstPngHash = AuditionPvSha256.FileHash(first.outputPath);
            string firstReceiptHash = AuditionPvSha256.FileHash(first.receiptPath);
            byte[] header = File.ReadAllBytes(first.outputPath).Take(24).ToArray();

            Assert.That(header.Take(8), Is.EqualTo(new byte[]
                { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }));
            Assert.That(ReadBigEndianInt32(header, 16), Is.EqualTo(2560));
            Assert.That(ReadBigEndianInt32(header, 20), Is.EqualTo(1440));
            Assert.That(first.outputSha256, Is.EqualTo(firstPngHash));

            AuditionPvEndCardPlaceholderResult second =
                AuditionPvEndCardPlaceholderProducer.Produce(
                    projectRoot,
                    temporaryRoot);

            Assert.That(second.outputSha256, Is.EqualTo(firstPngHash));
            Assert.That(AuditionPvSha256.FileHash(second.outputPath), Is.EqualTo(firstPngHash));
            Assert.That(second.receiptSha256, Is.EqualTo(firstReceiptHash));
            Assert.That(AuditionPvSha256.FileHash(second.receiptPath), Is.EqualTo(firstReceiptHash));
        }

        [Test]
        public void Receipt_PinsCanonicalLogosPretendardFontsAndLicense()
        {
            AuditionPvEndCardPlaceholderResult result =
                AuditionPvEndCardPlaceholderProducer.Produce(
                    projectRoot,
                    temporaryRoot);
            AuditionPvEndCardPlaceholderReceipt receipt = result.receipt;

            Assert.That(receipt.schemaVersion,
                Is.EqualTo(AuditionPvEndCardPlaceholderProducer.ReceiptSchema));
            Assert.That(receipt.shotId, Is.EqualTo("PV_S100"));
            Assert.That(receipt.width, Is.EqualTo(2560));
            Assert.That(receipt.height, Is.EqualTo(1440));
            Assert.That(receipt.artifact.path,
                Is.EqualTo(AuditionPvEndCardPlaceholderProducer.GateRelativeArtifactPath));
            Assert.That(receipt.artifact.sha256, Is.EqualTo(result.outputSha256));
            Assert.That(receipt.sloganApprovalStatus, Is.EqualTo("pending-approval"));
            Assert.That(receipt.auditionNoticeApprovalStatus, Is.EqualTo("pending-approval"));
            Assert.That(receipt.finalGraphicStatus, Is.EqualTo("deferred-to-ae-picture-lock"));

            Assert.That(
                receipt.sources.Select(value => value.assetPath),
                Is.EqualTo(AuditionPvEndCardPlaceholderProducer.SourceSpecs
                    .Select(value => value.assetPath)));
            Assert.That(receipt.sources, Has.Length.EqualTo(5));
            foreach (AuditionPvEndCardSourcePin pin in receipt.sources)
            {
                string fullPath = Path.Combine(
                    projectRoot,
                    pin.assetPath.Replace('/', Path.DirectorySeparatorChar));
                Assert.That(File.Exists(fullPath), Is.True, pin.assetPath);
                Assert.That(pin.sha256, Is.EqualTo(AuditionPvSha256.FileHash(fullPath)),
                    pin.assetPath);
            }

            Assert.That(receipt.sources.Single(value =>
                    value.sourceId == "dimension-brawl-logo-kr").role,
                Is.EqualTo("composited-logo-kr"));
            Assert.That(receipt.sources.Single(value =>
                    value.sourceId == "dimension-brawl-sublogo-en").role,
                Is.EqualTo("composited-sublogo-en"));
            Assert.That(receipt.sources.Count(value => value.scope == "font"), Is.EqualTo(2));
            Assert.That(receipt.sources.Single(value => value.scope == "font-license")
                .assetPath, Does.EndWith("Pretendard_LICENSE.txt"));
        }

        private static int ReadBigEndianInt32(byte[] value, int offset) =>
            value[offset] << 24 |
            value[offset + 1] << 16 |
            value[offset + 2] << 8 |
            value[offset + 3];
    }
}
