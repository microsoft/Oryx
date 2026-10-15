// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Oryx.BuildScriptGeneratorCli.DeploymentProgress;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    public class DeploymentProgressWriterTests : IClassFixture<TestTempDirTestFixture>
    {
        private readonly TestTempDirTestFixture testDir;

        public DeploymentProgressWriterTests(TestTempDirTestFixture testDir)
        {
            this.testDir = testDir;
        }

        [Fact]
        public void TryAppendPhase_WritesExactVersionOneRecord()
        {
            // Arrange
            var endpointPath = this.CreateEndpoint();
            var timestamp = new DateTimeOffset(2026, 10, 9, 1, 2, 3, TimeSpan.Zero)
                .AddTicks(4567000);

            // Act
            var result = DeploymentProgressWriter.TryAppendPhase(
                ToEndpoint(endpointPath),
                "operation-123",
                "build.execute",
                timestamp);

            // Assert
            Assert.True(result);
            Assert.Equal(
                "{\"schemaVersion\":1,\"eventType\":\"phase_started\"," +
                "\"operationId\":\"operation-123\",\"phase\":\"build.execute\"," +
                "\"timestampUtc\":\"2026-10-09T01:02:03.4567000Z\"}\n",
                File.ReadAllText(endpointPath));
        }

        [Fact]
        public void TryAppendPhase_DoesNotCreateMissingEndpoint()
        {
            // Arrange
            var endpointPath = this.testDir.GenerateRandomChildDirPath();

            // Act
            var result = DeploymentProgressWriter.TryAppendPhase(
                ToEndpoint(endpointPath),
                "operation-123",
                "build.execute",
                DateTimeOffset.UtcNow);

            // Assert
            Assert.False(result);
            Assert.False(File.Exists(endpointPath));
        }

        [Theory]
        [InlineData(null, "operation-123", "build.execute")]
        [InlineData("", "operation-123", "build.execute")]
        [InlineData("relative.jsonl", "operation-123", "build.execute")]
        [InlineData("file:relative.jsonl", "operation-123", "build.execute")]
        [InlineData("VALID_ENDPOINT", null, "build.execute")]
        [InlineData("VALID_ENDPOINT", "", "build.execute")]
        [InlineData("VALID_ENDPOINT", "operation id", "build.execute")]
        [InlineData("VALID_ENDPOINT", "opération", "build.execute")]
        [InlineData("VALID_ENDPOINT", "operation-123", "unknown")]
        public void TryAppendPhase_RejectsMalformedConfiguration(
            string endpoint,
            string operationId,
            string phase)
        {
            // Arrange
            var endpointPath = this.CreateEndpoint();
            endpoint = endpoint == "VALID_ENDPOINT" ? ToEndpoint(endpointPath) : endpoint;

            // Act
            var result = DeploymentProgressWriter.TryAppendPhase(
                endpoint,
                operationId,
                phase,
                DateTimeOffset.UtcNow);

            // Assert
            Assert.False(result);
            Assert.Empty(File.ReadAllBytes(endpointPath));
        }

        [Fact]
        public void TryAppendPhase_EnforcesOperationIdAndFileBounds()
        {
            // Arrange
            var operationId = "a" + new string('b', 255);
            var firstEndpointPath = this.CreateEndpoint();
            var timestamp = new DateTimeOffset(2026, 10, 9, 1, 2, 3, TimeSpan.Zero);

            // Act
            var firstResult = DeploymentProgressWriter.TryAppendPhase(
                ToEndpoint(firstEndpointPath),
                operationId,
                "build.execute",
                timestamp);
            var record = File.ReadAllBytes(firstEndpointPath);

            var boundedEndpointPath = this.CreateEndpoint();
            File.WriteAllBytes(
                boundedEndpointPath,
                Enumerable.Repeat((byte)'x', DeploymentProgressWriter.MaxFileBytes - record.Length).ToArray());
            var boundedResult = DeploymentProgressWriter.TryAppendPhase(
                ToEndpoint(boundedEndpointPath),
                operationId,
                "build.execute",
                timestamp);
            var overflowResult = DeploymentProgressWriter.TryAppendPhase(
                ToEndpoint(boundedEndpointPath),
                operationId,
                "post_build",
                timestamp);

            // Assert
            Assert.True(firstResult);
            Assert.InRange(record.Length, 1, DeploymentProgressWriter.MaxRecordBytes);
            Assert.True(boundedResult);
            Assert.False(overflowResult);
            Assert.Equal(DeploymentProgressWriter.MaxFileBytes, new FileInfo(boundedEndpointPath).Length);

            Assert.False(DeploymentProgressWriter.TryAppendPhase(
                ToEndpoint(this.CreateEndpoint()),
                operationId + "c",
                "build.execute",
                timestamp));
        }

        [Fact]
        public void TryAppendPhase_FailsOpenForDirectoryEndpoint()
        {
            var directoryPath = this.testDir.CreateChildDir();

            Assert.False(DeploymentProgressWriter.TryAppendPhase(
                ToEndpoint(directoryPath),
                "operation-123",
                "build.execute",
                DateTimeOffset.UtcNow));
        }

        [EnableOnPlatform("LINUX")]
        public void TryAppendPhase_RejectsUnixDeviceEndpoint()
        {
            Assert.False(DeploymentProgressWriter.TryAppendPhase(
                "file:/dev/null",
                "operation-123",
                "build.execute",
                DateTimeOffset.UtcNow));
        }

        [EnableOnPlatform("LINUX")]
        public void TryAppendPhase_RejectsUnixHardLinkEndpoint()
        {
            var targetPath = this.CreateEndpoint();
            var hardLinkPath = this.testDir.GenerateRandomChildDirPath();
            Assert.Equal(0, Link(targetPath, hardLinkPath));

            Assert.False(DeploymentProgressWriter.TryAppendPhase(
                ToEndpoint(hardLinkPath),
                "operation-123",
                "build.execute",
                DateTimeOffset.UtcNow));
            Assert.Empty(File.ReadAllBytes(targetPath));
        }

        [DllImport("libc", EntryPoint = "link", SetLastError = true)]
        private static extern int Link(
            [MarshalAs(UnmanagedType.LPUTF8Str)] string targetPath,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string linkPath);

        private static string ToEndpoint(string path)
        {
            return "file:" + path;
        }

        private string CreateEndpoint()
        {
            var path = this.testDir.GenerateRandomChildDirPath();
            File.WriteAllBytes(path, Array.Empty<byte>());
            return path;
        }
    }
}
