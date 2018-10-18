// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGeneratorCli.Logging;
using Oryx.Tests.Infrastructure;
using Xunit;

namespace BuildScriptGeneratorCli.Tests
{
    public class FileLoggerProviderTest : IClassFixture<TestTempDirTestFixure>
    {
        private readonly string _rooDirPath;

        public FileLoggerProviderTest(TestTempDirTestFixure testFixture)
        {
            _rooDirPath = testFixture.RootDirPath;
        }

        [Fact]
        public void CreatesFileLogger_WithExpectedDetails()
        {
            // Arrange
            var logFile = Path.Combine(_rooDirPath, Guid.NewGuid().ToString());
            var provider = CreateFileLoggerProvider(logFile, LogLevel.Critical);

            // Act
            var logger = provider.CreateLogger("category1");

            // Assert
            var fileLogger = Assert.IsType<FileLogger>(logger);
            Assert.Equal("category1", fileLogger.CategoryName);
            Assert.Empty(fileLogger.Messages);
            Assert.Equal(LogLevel.Critical, fileLogger.MinimumLogLevel);
        }

        [Fact]
        public void FlushesLogMessages_OnDispose()
        {
            // Arrange
            var expected = "an error message";
            var logFile = Path.Combine(_rooDirPath, Guid.NewGuid().ToString());
            var provider = CreateFileLoggerProvider(logFile, thresholdLimit: 5, LogLevel.Warning);
            var logger = provider.CreateLogger("category1");

            // Act-1
            logger.LogError(expected);

            // Assert-1
            var fileLogger = Assert.IsType<FileLogger>(logger);
            var message = Assert.Single(fileLogger.Messages);
            Assert.Contains(expected, message);
            Assert.False(File.Exists(logFile));

            // Act-2
            provider.Dispose();

            // Assert-2
            Assert.Empty(fileLogger.Messages);
            Assert.True(File.Exists(logFile));
            var fileContent = File.ReadAllText(logFile);
            Assert.Contains(expected, fileContent);
        }

        [Fact]
        public void FlushesMessages_WhenThresholdIsMet()
        {
            // Arrange
            var logFile = Path.Combine(_rooDirPath, Guid.NewGuid().ToString());
            var provider = CreateFileLoggerProvider(logFile, thresholdLimit: 2, LogLevel.Warning);
            var logger = provider.CreateLogger("category1");

            // Act
            logger.LogError("message1");
            logger.LogError("message2");

            // Assert
            var fileLogger = Assert.IsType<FileLogger>(logger);
            Assert.Empty(fileLogger.Messages);
            Assert.True(File.Exists(logFile));
            var lines = File.ReadAllLines(logFile);
            Assert.Equal(2, lines.Length);
            Assert.Contains("message1", lines[0]);
            Assert.Contains("message2", lines[1]);
        }

        [Fact]
        public void AppendsLogs_IfFileAlreadyExists()
        {
            // Arrange
            var logFile = Path.Combine(_rooDirPath, Guid.NewGuid().ToString());
            var provider = CreateFileLoggerProvider(logFile, thresholdLimit: 2, LogLevel.Warning);
            var logger = provider.CreateLogger("category1");

            // Act
            logger.LogError("message1");
            logger.LogError("message2");

            // The following should append text to the earlier one
            logger.LogError("message3");
            logger.LogError("message4");

            // Assert-2
            var fileLogger = Assert.IsType<FileLogger>(logger);
            Assert.Empty(fileLogger.Messages);
            Assert.True(File.Exists(logFile));
            var lines = File.ReadAllLines(logFile);
            Assert.Contains("message1", lines[0]);
            Assert.Contains("message2", lines[1]);
            Assert.Contains("message3", lines[2]);
            Assert.Contains("message4", lines[3]);
        }

        [Fact]
        public void DoesNotCreateLogFile_WhenThereAreNoMessagesToWrite()
        {
            // Arrange
            var logFile = Path.Combine(_rooDirPath, Guid.NewGuid().ToString());
            var provider = CreateFileLoggerProvider(logFile, LogLevel.Warning);
            var logger = provider.CreateLogger("category1");

            // Act
            provider.Dispose();

            // Assert
            Assert.False(File.Exists(logFile));
        }

        [Fact]
        public void WritesLogToDefaultLogFile_IfNoLogFileProvidedByUser()
        {
            // Arrange
            var tempDirProvider = new TestTempDirectoryProvider(Path.Combine(_rooDirPath, Guid.NewGuid().ToString()));
            var provider = CreateFileLoggerProvider(
                logFile: null, // No explicit log file
                thresholdLimit: 1,
                LogLevel.Warning,
                tempDirProvider);
            var logger = provider.CreateLogger("category1");
            var expectedLogFile = Path.Combine(
                tempDirProvider.GetTempDirectory(),
                FileLoggerProvider.DefaultLogFileName);

            // Act
            logger.LogError("message1");
            logger.LogError("message2");

            // Assert
            var fileLogger = Assert.IsType<FileLogger>(logger);
            Assert.Empty(fileLogger.Messages);
            Assert.True(File.Exists(expectedLogFile));
            var lines = File.ReadAllLines(expectedLogFile);
            Assert.Equal(2, lines.Length);
            Assert.Contains("message1", lines[0]);
            Assert.Contains("message2", lines[1]);
        }

        private FileLoggerProvider CreateFileLoggerProvider(string logFile, LogLevel minimumLogLevel)
        {
            return CreateFileLoggerProvider(
                logFile,
                FileLoggerProvider.DefaultMessageThresholdLimit,
                minimumLogLevel);
        }

        private FileLoggerProvider CreateFileLoggerProvider(
            string logFile,
            int thresholdLimit,
            LogLevel minimumLogLevel)
        {
            return CreateFileLoggerProvider(
                logFile,
                thresholdLimit,
                minimumLogLevel,
                new TestTempDirectoryProvider(Path.Combine(Path.GetTempPath(), "oryxtests", nameof(FileLoggerProviderTest))));
        }

        private FileLoggerProvider CreateFileLoggerProvider(
            string logFile,
            int thresholdLimit,
            LogLevel minimumLogLevel,
            ITempDirectoryProvider tempDirectoryProvider)
        {
            var options = new BuildScriptGeneratorOptions
            {
                LogFile = logFile,
                MinimumLogLevel = minimumLogLevel
            };
            return new FileLoggerProvider(
                tempDirectoryProvider,
                Options.Create(options),
                thresholdLimit);
        }

        private class TestTempDirectoryProvider : ITempDirectoryProvider
        {
            private readonly string _tempDir;

            public TestTempDirectoryProvider(string tempDir)
            {
                _tempDir = tempDir;
            }

            public string GetTempDirectory()
            {
                Directory.CreateDirectory(_tempDir);
                return _tempDir;
            }
        }

        private class TestLogger : ILogger, IDisposable
        {
            public bool IsDisposeCalled { get; private set; }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }

            public void Dispose()
            {
                IsDisposeCalled = true;
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
            }
        }
    }
}
