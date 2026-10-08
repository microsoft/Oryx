// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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

            using (var document = JsonDocument.Parse(File.ReadAllText(endpointPath)))
            {
                Assert.Equal(
                    new[] { "schemaVersion", "eventType", "operationId", "phase", "timestampUtc" },
                    document.RootElement.EnumerateObject().Select(property => property.Name));
            }
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
            // Arrange
            var directoryPath = this.testDir.CreateChildDir();

            // Act
            var exception = Record.Exception(() =>
                DeploymentProgressWriter.TryAppendPhase(
                    ToEndpoint(directoryPath),
                    "operation-123",
                    "build.execute",
                    DateTimeOffset.UtcNow));
            var result = DeploymentProgressWriter.TryAppendPhase(
                ToEndpoint(directoryPath),
                "operation-123",
                "build.execute",
                DateTimeOffset.UtcNow);

            // Assert
            Assert.Null(exception);
            Assert.False(result);
        }

        [Fact]
        public void TryAppendPhase_ConcurrentWritesRemainCompleteAndBounded()
        {
            // Arrange
            var endpointPath = this.CreateEndpoint();
            var results = new bool[16];
            var timestamp = new DateTimeOffset(2026, 10, 9, 1, 2, 3, TimeSpan.Zero);

            // Act
            Parallel.For(
                0,
                results.Length,
                index =>
                {
                    results[index] = DeploymentProgressWriter.TryAppendPhase(
                        ToEndpoint(endpointPath),
                        $"operation-{index}",
                        "build.execute",
                        timestamp);
                });

            // Assert
            var lines = File.ReadAllLines(endpointPath);
            Assert.Equal(results.Count(result => result), lines.Length);
            Assert.NotEmpty(lines);
            Assert.InRange(new FileInfo(endpointPath).Length, 1, DeploymentProgressWriter.MaxFileBytes);
            Assert.All(
                lines,
                line =>
                {
                    using (var document = JsonDocument.Parse(line))
                    {
                        Assert.Equal("phase_started", document.RootElement.GetProperty("eventType").GetString());
                    }
                });
        }

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
