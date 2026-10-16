// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Oryx.BuildScriptGenerator.DeploymentProgress;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class DeploymentProgressReporterTests
    {
        private static readonly DateTimeOffset Timestamp =
            new DateTimeOffset(2026, 1, 2, 3, 4, 5, 123, TimeSpan.Zero);

        [Fact]
        public void Reporter_IsExactNoOp_WhenEndpointIsMissing()
        {
            var records = new List<byte[]>();
            var reporter = CreateReporter(endpoint: null, operationId: null, records.Add);

            reporter.ReportBuildStarted();
            reporter.ReportPhaseStarted("script.generate");
            reporter.ReportBuildCompleted("succeeded");

            Assert.False(reporter.IsEnabled);
            Assert.Empty(records);
        }

        [Fact]
        public void Reporter_SerializesGoldenVersionOneRecords()
        {
            var records = new List<byte[]>();
            var reporter = CreateReporter(GetEndpoint(), "operation-1", records.Add);

            reporter.ReportBuildStarted();
            reporter.ReportPhaseStarted("script.generate");
            reporter.ReportPhaseStarted("dependencies.restore");
            reporter.ReportPhaseStarted("build.execute");
            reporter.ReportBuildCompleted("succeeded");

            Assert.Collection(
                records,
                record => Assert.Equal(
                    "{\"schemaVersion\":1,\"eventType\":\"build_started\",\"operationId\":\"operation-1\"," +
                    "\"timestampUtc\":\"2026-01-02T03:04:05.123Z\"}\n",
                    Encoding.UTF8.GetString(record)),
                record => Assert.Equal(
                    "{\"schemaVersion\":1,\"eventType\":\"phase_started\",\"operationId\":\"operation-1\"," +
                    "\"timestampUtc\":\"2026-01-02T03:04:05.123Z\",\"phase\":\"script.generate\"}\n",
                    Encoding.UTF8.GetString(record)),
                record => AssertEvent(record, "phase_started", "dependencies.restore"),
                record => AssertEvent(record, "phase_started", "build.execute"),
                record => Assert.Equal(
                    "{\"schemaVersion\":1,\"eventType\":\"build_completed\",\"operationId\":\"operation-1\"," +
                    "\"timestampUtc\":\"2026-01-02T03:04:05.123Z\",\"outcome\":\"succeeded\"}\n",
                    Encoding.UTF8.GetString(record)));
        }

        [Fact]
        public void Reporter_EmitsOnlyProducerOwnedFields()
        {
            var records = new List<byte[]>();
            var reporter = CreateReporter(GetEndpoint(), "operation-1", records.Add);

            reporter.ReportBuildStarted();
            reporter.ReportPhaseStarted("dependencies.restore");
            reporter.ReportBuildCompleted("succeeded");

            Assert.Collection(
                records,
                record => AssertPropertyNames(
                    record,
                    "schemaVersion",
                    "eventType",
                    "operationId",
                    "timestampUtc"),
                record => AssertPropertyNames(
                    record,
                    "schemaVersion",
                    "eventType",
                    "operationId",
                    "timestampUtc",
                    "phase"),
                record => AssertPropertyNames(
                    record,
                    "schemaVersion",
                    "eventType",
                    "operationId",
                    "timestampUtc",
                    "outcome"));
        }

        [Fact]
        public void Reporter_SuppressesDuplicateAndOutOfOrderTransitions()
        {
            var records = new List<byte[]>();
            var reporter = CreateReporter(GetEndpoint(), "operation-1", records.Add);

            reporter.ReportBuildStarted();
            reporter.ReportPhaseStarted("script.generate");
            reporter.ReportPhaseStarted("script.generate");
            reporter.ReportPhaseStarted("build.execute");
            reporter.ReportPhaseStarted("dependencies.restore");
            reporter.ReportBuildCompleted("failed");
            reporter.ReportPhaseStarted("manifest.write");
            reporter.ReportBuildCompleted("succeeded");

            Assert.Collection(
                records,
                record => AssertEvent(record, "build_started"),
                record => AssertEvent(record, "phase_started", "script.generate"),
                record => AssertEvent(record, "phase_started", "build.execute"),
                record =>
                {
                    AssertEvent(record, "build_completed");
                    using (var document = JsonDocument.Parse(record))
                    {
                        Assert.Equal("failed", document.RootElement.GetProperty("outcome").GetString());
                    }
                });
        }

        [Theory]
        [InlineData("relative.json")]
        [InlineData("http:/tmp/progress.json")]
        [InlineData("file:relative.json")]
        public void Reporter_Disables_ForInvalidEndpoints(string endpoint)
        {
            var records = new List<byte[]>();
            var reporter = CreateReporter(endpoint, "operation-1", records.Add);

            reporter.ReportBuildStarted();

            Assert.False(reporter.IsEnabled);
            Assert.Empty(records);
        }

        [Theory]
        [InlineData("a")]
        [InlineData("operation-1")]
        [InlineData("A.b:c_1")]
        [InlineData("0123456789abcdef0123456789abcdef")]
        public void Reporter_AcceptsAsciiSafeOperationIds(string operationId)
        {
            var records = new List<byte[]>();
            var reporter = CreateReporter(GetEndpoint(), operationId, records.Add);

            reporter.ReportBuildStarted();

            Assert.True(reporter.IsEnabled);
            Assert.Single(records);
        }

        [Theory]
        [InlineData("")]
        [InlineData("-starts-with-punctuation")]
        [InlineData("contains space")]
        [InlineData("unicode-\u00E9")]
        [InlineData("control-\n")]
        public void Reporter_RejectsUnsafeOperationIds(string operationId)
        {
            var records = new List<byte[]>();
            var reporter = CreateReporter(GetEndpoint(), operationId, records.Add);

            reporter.ReportBuildStarted();

            Assert.False(reporter.IsEnabled);
            Assert.Empty(records);
        }

        [Fact]
        public void Reporter_AcceptsMaximumLengthOperationId()
        {
            var records = new List<byte[]>();
            var reporter = CreateReporter(GetEndpoint(), new string('a', 256), records.Add);

            reporter.ReportBuildStarted();

            Assert.Single(records);
        }

        [Fact]
        public void Reporter_RejectsOperationIdOverMaximumLength()
        {
            var reporter = CreateReporter(GetEndpoint(), new string('a', 257), _ => { });

            Assert.False(reporter.IsEnabled);
        }

        [Fact]
        public void Reporter_DisablesPermanently_WhenAppendFails()
        {
            var attempts = 0;
            var reporter = CreateReporter(
                GetEndpoint(),
                "operation-1",
                _ =>
                {
                    attempts++;
                    throw new IOException("customer path and exception must not be logged");
                });

            reporter.ReportBuildStarted();
            reporter.ReportBuildCompleted("failed");

            Assert.False(reporter.IsEnabled);
            Assert.Equal(1, attempts);
        }

        [Fact]
        public void Reporter_LogsOnlyFixedDiagnostic_WhenAppendFails()
        {
            var logger = new CapturingLogger();
            var reporter = new DeploymentProgressReporter(
                GetEndpoint(),
                "operation-1",
                logger,
                () => Timestamp,
                _ => throw new IOException("secret /customer/path"));

            reporter.ReportBuildStarted();

            Assert.Null(logger.Exception);
            Assert.Null(logger.Message);
        }

        [Fact]
        public void HiddenWriter_AppendsOneValidatedRecord_ToPrecreatedFile()
        {
            var path = Path.GetTempFileName();
            try
            {
                var record = GoldenBuildStartedRecord();
                using (var input = new MemoryStream(record))
                {
                    var result = DeploymentProgressReporter.AppendRecordFromStandardInput(
                        input,
                        $"file:{path}");

                    Assert.Equal(0, result);
                    Assert.Equal(record, File.ReadAllBytes(path));
                }
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void HiddenWriter_RejectsMissingFileAndMalformedRecord()
        {
            var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            using (var validInput = new MemoryStream(GoldenBuildStartedRecord()))
            {
                Assert.NotEqual(
                    0,
                    DeploymentProgressReporter.AppendRecordFromStandardInput(
                        validInput,
                        $"file:{missingPath}"));
            }

            var path = Path.GetTempFileName();
            try
            {
                using (var malformedInput = new MemoryStream(Encoding.UTF8.GetBytes("{\"arbitrary\":\"text\"}\n")))
                {
                    Assert.NotEqual(
                        0,
                        DeploymentProgressReporter.AppendRecordFromStandardInput(
                            malformedInput,
                            $"file:{path}"));
                    Assert.Empty(File.ReadAllBytes(path));
                }
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void HiddenWriter_RejectsEveryUnsupportedRecordShape()
        {
            var common =
                "\"eventType\":\"build_started\",\"operationId\":\"operation-1\"," +
                "\"timestampUtc\":\"2026-01-02T03:04:05.123Z\"";
            var invalidRecords = new[]
            {
                $"{{\"schemaVersion\":2,{common}}}\n",
                $"{{\"schemaVersion\":1,{common},\"extra\":1}}\n",
                $"{{\"schemaVersion\":1,{common},\"attemptId\":\"attempt-1\"}}\n",
                $"{{\"schemaVersion\":1,{common},\"eventType\":\"build_started\"}}\n",
                "{\"schemaVersion\":1,\"eventType\":\"unknown\",\"operationId\":\"operation-1\"," +
                    "\"timestampUtc\":\"2026-01-02T03:04:05.123Z\"}\n",
                "{\"schemaVersion\":1,\"eventType\":\"phase_started\",\"operationId\":\"operation-1\"," +
                    "\"timestampUtc\":\"2026-01-02T03:04:05.123Z\",\"phase\":\"unknown\"}\n",
                "{\"schemaVersion\":1,\"eventType\":\"build_completed\",\"operationId\":\"operation-1\"," +
                    "\"timestampUtc\":\"2026-01-02T03:04:05.123Z\",\"outcome\":\"unknown\"}\n",
                "{\"schemaVersion\":1,\"eventType\":\"build_started\",\"operationId\":\"operation-1\"," +
                    "\"timestampUtc\":\"not-a-timestamp\"}\n",
                "{\"schemaVersion\":1,\"eventType\":\"build_started\",\"operationId\":\"operation-1\"," +
                    "\"timestampUtc\":\"2026-01-02T03:04:05.123Z\"}",
                new string('x', DeploymentProgressConstants.MaxRecordBytes) + "\n",
            };
            var path = Path.GetTempFileName();
            try
            {
                foreach (var invalidRecord in invalidRecords)
                {
                    using (var input = new MemoryStream(Encoding.UTF8.GetBytes(invalidRecord)))
                    {
                        Assert.NotEqual(
                            0,
                            DeploymentProgressReporter.AppendRecordFromStandardInput(
                                input,
                                $"file:{path}"));
                        Assert.Empty(File.ReadAllBytes(path));
                    }
                }
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void HiddenWriter_RejectsSymlinkAndSymlinkAncestor()
        {
            if (!OperatingSystem.IsLinux())
            {
                return;
            }

            var root = Path.Combine(Path.GetTempPath(), "oryx-progress-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var targetPath = Path.Combine(root, "target.jsonl");
                File.WriteAllText(targetPath, string.Empty);
                var symlinkPath = Path.Combine(root, "link.jsonl");
                File.CreateSymbolicLink(symlinkPath, targetPath);

                using (var input = new MemoryStream(GoldenBuildStartedRecord()))
                {
                    Assert.NotEqual(
                        0,
                        DeploymentProgressReporter.AppendRecordFromStandardInput(
                            input,
                            $"file:{symlinkPath}"));
                }

                var realDirectory = Path.Combine(root, "real");
                Directory.CreateDirectory(realDirectory);
                var ancestorTargetPath = Path.Combine(realDirectory, "progress.jsonl");
                File.WriteAllText(ancestorTargetPath, string.Empty);
                var directorySymlink = Path.Combine(root, "linked-directory");
                Directory.CreateSymbolicLink(directorySymlink, realDirectory);

                using (var input = new MemoryStream(GoldenBuildStartedRecord()))
                {
                    Assert.NotEqual(
                        0,
                        DeploymentProgressReporter.AppendRecordFromStandardInput(
                            input,
                            $"file:{Path.Combine(directorySymlink, "progress.jsonl")}"));
                }

                Assert.Empty(File.ReadAllBytes(targetPath));
                Assert.Empty(File.ReadAllBytes(ancestorTargetPath));
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Fact]
        public void HiddenWriter_RejectsFileAtGlobalByteLimit()
        {
            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(path, new byte[DeploymentProgressConstants.MaxFileBytes]);
                using (var input = new MemoryStream(GoldenBuildStartedRecord()))
                {
                    Assert.NotEqual(
                        0,
                        DeploymentProgressReporter.AppendRecordFromStandardInput(input, $"file:{path}"));
                    Assert.Equal(DeploymentProgressConstants.MaxFileBytes, new FileInfo(path).Length);
                }
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void BoundedWriter_ReturnsWithinBudgetAndKillsBlockedChild()
        {
            var process = new BlockingWriterProcess();
            var factory = new TestWriterProcessFactory(process);
            var writer = new BoundedDeploymentProgressWriter(Path.GetTempFileName(), factory);
            var stopwatch = Stopwatch.StartNew();

            Assert.Throws<TimeoutException>(() => writer.AppendRecord(GoldenBuildStartedRecord()));

            Assert.True(process.Killed);
            Assert.True(stopwatch.Elapsed < BoundedDeploymentProgressWriter.ProcessTimeout +
                BoundedDeploymentProgressWriter.TerminationTimeout +
                TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void BoundedWriter_ReturnsWithinBudget_WhenChildLaunchBlocks()
        {
            var process = new SuccessfulWriterProcess();
            var launchGate = new ManualResetEventSlim(initialState: false);
            var factory = new BlockingStartWriterProcessFactory(process, launchGate);
            var writer = new BoundedDeploymentProgressWriter(Path.GetTempFileName(), factory);
            var stopwatch = Stopwatch.StartNew();

            Assert.Throws<TimeoutException>(() => writer.AppendRecord(GoldenBuildStartedRecord()));

            Assert.True(stopwatch.Elapsed < BoundedDeploymentProgressWriter.ProcessTimeout +
                BoundedDeploymentProgressWriter.TerminationTimeout +
                TimeSpan.FromSeconds(1));
            launchGate.Set();
            Assert.True(SpinWait.SpinUntil(() => process.Killed, TimeSpan.FromSeconds(1)));
            Assert.Empty(process.WrittenBytes);
        }

        [Fact]
        public void BoundedWriter_AllowsSlowStartupWithinBudget()
        {
            var process = new SuccessfulWriterProcess();
            var factory = new DelayedStartWriterProcessFactory(process, TimeSpan.FromMilliseconds(100));
            var writer = new BoundedDeploymentProgressWriter(Path.GetTempFileName(), factory);
            var record = GoldenBuildStartedRecord();

            writer.AppendRecord(record);

            Assert.Equal(record, process.WrittenBytes);
            Assert.False(process.Killed);
        }

        [Fact]
        public void WriterProcessFactory_ConstructsDotNetHostAndAppHostCommands()
        {
            var dotnetStartInfo = DeploymentProgressWriterProcessFactory.CreateStartInfo(
                Path.Combine("runtime", "dotnet.exe"),
                new[] { "GenerateBuildScript.dll" },
                Path.GetFullPath("progress.jsonl"));

            Assert.Equal(
                new[]
                {
                    "GenerateBuildScript.dll",
                    DeploymentProgressWriterProcessFactory.DeploymentProgressWriteCommandName,
                },
                dotnetStartInfo.ArgumentList);
            Assert.True(dotnetStartInfo.RedirectStandardInput);
            Assert.True(dotnetStartInfo.RedirectStandardOutput);
            Assert.True(dotnetStartInfo.RedirectStandardError);
            Assert.Equal(
                $"file:{Path.GetFullPath("progress.jsonl")}",
                dotnetStartInfo.Environment[DeploymentProgressConstants.EndpointEnvironmentVariable]);

            var appHostStartInfo = DeploymentProgressWriterProcessFactory.CreateStartInfo(
                Path.Combine("runtime", "oryx.exe"),
                Array.Empty<string>(),
                Path.GetFullPath("progress.jsonl"));

            Assert.Equal(
                new[] { DeploymentProgressWriterProcessFactory.DeploymentProgressWriteCommandName },
                appHostStartInfo.ArgumentList);
            Assert.Throws<InvalidOperationException>(() =>
                DeploymentProgressWriterProcessFactory.CreateStartInfo(
                    Path.Combine("runtime", "dotnet"),
                    Array.Empty<string>(),
                    Path.GetFullPath("progress.jsonl")));
        }

        private static DeploymentProgressReporter CreateReporter(
            string endpoint,
            string operationId,
            Action<byte[]> appendRecord)
        {
            return new DeploymentProgressReporter(
                endpoint,
                operationId,
                NullLogger<DeploymentProgressReporter>.Instance,
                () => Timestamp,
                appendRecord);
        }

        private static string GetEndpoint()
        {
            return $"file:{Path.Combine(Path.GetTempPath(), "oryx-progress.jsonl")}";
        }

        private static byte[] GoldenBuildStartedRecord()
        {
            return Encoding.UTF8.GetBytes(
                "{\"schemaVersion\":1,\"eventType\":\"build_started\",\"operationId\":\"operation-1\"," +
                "\"timestampUtc\":\"2026-01-02T03:04:05.123Z\"}\n");
        }

        private static void AssertEvent(byte[] record, string eventType, string phase = null)
        {
            Assert.InRange(record.Length, 1, DeploymentProgressConstants.MaxRecordBytes);
            Assert.Equal((byte)'\n', record[record.Length - 1]);
            using (var document = JsonDocument.Parse(record))
            {
                var root = document.RootElement;
                Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
                Assert.Equal(eventType, root.GetProperty("eventType").GetString());
                Assert.Equal("operation-1", root.GetProperty("operationId").GetString());
                Assert.Equal(Timestamp, root.GetProperty("timestampUtc").GetDateTimeOffset());
                if (phase != null)
                {
                    Assert.Equal(phase, root.GetProperty("phase").GetString());
                }
            }
        }

        private static void AssertPropertyNames(byte[] record, params string[] expectedNames)
        {
            using (var document = JsonDocument.Parse(record))
            {
                var actualNames = new List<string>();
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    actualNames.Add(property.Name);
                }

                Assert.Equal(expectedNames, actualNames);
            }
        }

        private sealed class TestWriterProcessFactory : IDeploymentProgressWriterProcessFactory
        {
            private readonly IDeploymentProgressWriterProcess process;

            public TestWriterProcessFactory(IDeploymentProgressWriterProcess process)
            {
                this.process = process;
            }

            public IDeploymentProgressWriterProcess Start(string endpointPath)
            {
                return this.process;
            }
        }

        private sealed class BlockingStartWriterProcessFactory : IDeploymentProgressWriterProcessFactory
        {
            private readonly IDeploymentProgressWriterProcess process;
            private readonly ManualResetEventSlim launchGate;

            public BlockingStartWriterProcessFactory(
                IDeploymentProgressWriterProcess process,
                ManualResetEventSlim launchGate)
            {
                this.process = process;
                this.launchGate = launchGate;
            }

            public IDeploymentProgressWriterProcess Start(string endpointPath)
            {
                this.launchGate.Wait();
                return this.process;
            }
        }

        private sealed class DelayedStartWriterProcessFactory : IDeploymentProgressWriterProcessFactory
        {
            private readonly TimeSpan delay;
            private readonly IDeploymentProgressWriterProcess process;

            public DelayedStartWriterProcessFactory(
                IDeploymentProgressWriterProcess process,
                TimeSpan delay)
            {
                this.process = process;
                this.delay = delay;
            }

            public IDeploymentProgressWriterProcess Start(string endpointPath)
            {
                Thread.Sleep(this.delay);
                return this.process;
            }
        }

        private sealed class BlockingWriterProcess : IDeploymentProgressWriterProcess
        {
            private readonly MemoryStream standardInput = new MemoryStream();

            public Stream StandardInput => this.standardInput;

            public int ExitCode => 1;

            public bool Killed { get; private set; }

            public bool WaitForExit(TimeSpan timeout)
            {
                if (!this.Killed)
                {
                    Thread.Sleep(timeout);
                    return false;
                }

                return true;
            }

            public void Kill()
            {
                this.Killed = true;
            }

            public void Dispose()
            {
                this.standardInput.Dispose();
            }
        }

        private sealed class SuccessfulWriterProcess : IDeploymentProgressWriterProcess
        {
            private readonly MemoryStream standardInput = new MemoryStream();

            public Stream StandardInput => this.standardInput;

            public int ExitCode => 0;

            public bool Killed { get; private set; }

            public byte[] WrittenBytes => this.standardInput.ToArray();

            public bool WaitForExit(TimeSpan timeout)
            {
                return true;
            }

            public void Kill()
            {
                this.Killed = true;
            }

            public void Dispose()
            {
                this.standardInput.Dispose();
            }
        }

        private sealed class CapturingLogger : ILogger<DeploymentProgressReporter>
        {
            public Exception Exception { get; private set; }

            public string Message { get; private set; }

            public IDisposable BeginScope<TState>(TState state)
            {
                return NullScope.Instance;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception exception,
                Func<TState, Exception, string> formatter)
            {
                this.Exception = exception;
                this.Message = formatter(state, exception);
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new NullScope();

            public void Dispose()
            {
            }
        }
    }
}
