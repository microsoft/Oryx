// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGeneratorCli.Logging;
using Xunit;

namespace BuildScriptGeneratorCli.Tests
{
    public class FileLoggerTest : IClassFixture<FileLoggerTest.FileLoggerTestFixutre>
    {
        private readonly string _rooDirPath;

        public FileLoggerTest(FileLoggerTestFixutre testFixutre)
        {
            _rooDirPath = testFixutre.RootDirPath;
        }

        [Fact]
        public void WriteLogs_ByExpandingParameters()
        {
            // Arrange
            var messageFormat = "a warning message with parameters: {param1} {param2}";
            var expected = "a warning message with parameters: foo bar";
            var logFile = Path.Combine(_rooDirPath, Guid.NewGuid().ToString());
            var fileLogger = new FileLogger("category1", logFile, LogLevel.Warning);

            // Act
            fileLogger.LogWarning(messageFormat, "foo", "bar");

            // Assert
            var message = Assert.Single(fileLogger.Messages);
            Assert.Contains(expected, message);
        }

        [Fact]
        public void WritesLogs_WhenLogLevel_IsSameAsMinimumLogLevel()
        {
            // Arrange
            var expected = "a warning message";
            var logFile = Path.Combine(_rooDirPath, Guid.NewGuid().ToString());
            var fileLogger = new FileLogger("category1", logFile, LogLevel.Warning);

            // Act
            fileLogger.LogWarning(expected);

            // Assert
            var message = Assert.Single(fileLogger.Messages);
            Assert.Contains(expected, message);
        }

        [Fact]
        public void WritesLogs_WhenLogLevel_IsGreaterThanMinimumLogLevel()
        {
            // Arrange
            var expected = "an error message";
            var logFile = Path.Combine(_rooDirPath, Guid.NewGuid().ToString());
            var fileLogger = new FileLogger("category1", logFile, LogLevel.Warning);

            // Act
            fileLogger.LogError(expected);

            // Assert
            var message = Assert.Single(fileLogger.Messages);
            Assert.Contains(expected, message);
        }

        [Fact]
        public void DoesNotWritesLogs_WhenLogLevel_IsLessThanMinimumLogLevel()
        {
            // Arrange
            var expected = "a debug message";
            var logFile = Path.Combine(_rooDirPath, Guid.NewGuid().ToString());
            var fileLogger = new FileLogger("category1", logFile, LogLevel.Warning);

            // Act
            fileLogger.LogDebug(expected);

            // Assert
            Assert.Empty(fileLogger.Messages);
        }

        [Fact]
        public void WritesLog_WithExceptionDetails()
        {
            // Arrange
            var logFile = Path.Combine(_rooDirPath, Guid.NewGuid().ToString());
            var fileLogger = new FileLogger("category1", logFile, LogLevel.Warning);

            // Act
            fileLogger.LogError(new InvalidOperationException("foo"), "an error message with exception");

            // Assert
            var message = Assert.Single(fileLogger.Messages);
            Assert.Contains("foo", message);
            Assert.Contains("an error message with exception", message);
        }

        [Fact]
        public void FlushesLogMessages_OnDispose()
        {
            // Arrange
            var expected = "an error message";
            var logFile = Path.Combine(_rooDirPath, Guid.NewGuid().ToString());
            var fileLogger = new FileLogger("category1", logFile, LogLevel.Warning);

            // Act-1
            fileLogger.LogError(expected);

            // Assert-1
            var message = Assert.Single(fileLogger.Messages);
            Assert.Contains(expected, message);
            Assert.False(File.Exists(logFile));

            // Act-2
            fileLogger.Dispose();

            // Assert-2
            Assert.Empty(fileLogger.Messages);
            Assert.True(File.Exists(logFile));
            var fileContent = File.ReadAllText(logFile);
            Assert.Contains(expected, fileContent);
        }

        [Fact]
        public void DoesNotFlushMessages_UntilThresholdIsMet()
        {
            // Arrange
            var expectedMessagePrefix = "an error message";
            var logFile = Path.Combine(_rooDirPath, Guid.NewGuid().ToString());
            var fileLogger = new FileLogger("category1", logFile, LogLevel.Warning);

            // Act
            for (var i = 0; i < FileLogger.DefaultMessageThreshold; i++)
            {
                fileLogger.LogError(expectedMessagePrefix + $"-{i}");
            }

            // Assert
            Assert.Empty(fileLogger.Messages);
            Assert.True(File.Exists(logFile));
            var fileContent = File.ReadAllText(logFile);
            for (var i = 0; i < FileLogger.DefaultMessageThreshold; i++)
            {
                Assert.Contains(expectedMessagePrefix + $"-{i}", fileContent);
            }
        }

        [Fact]
        public void AppendsLogs_IfFileAlreadyExists()
        {
            // Arrange
            var logFile = Path.Combine(_rooDirPath, Guid.NewGuid().ToString());
            var fileLogger = new FileLogger("category1", logFile, LogLevel.Warning);

            // Act
            for (var i = 0; i < FileLogger.DefaultMessageThreshold; i++)
            {
                fileLogger.LogError("before-{i}");
            }

            // The following should append text to the earlier one
            for (var i = 0; i < FileLogger.DefaultMessageThreshold; i++)
            {
                fileLogger.LogError($"later-{i}");
            }

            // Assert-2
            Assert.Empty(fileLogger.Messages);
            Assert.True(File.Exists(logFile));
            var fileContent = File.ReadAllText(logFile);
            Assert.Contains("before-", fileContent);
            Assert.Contains("later-", fileContent);
        }

        [Fact]
        public void WritesLogsToFile_InTheSequenceTheyWereLogged()
        {
            // Arrange
            var logFile = Path.Combine(_rooDirPath, Guid.NewGuid().ToString());
            var fileLogger = new FileLogger("category1", logFile, LogLevel.Warning);

            // Act
            fileLogger.LogWarning("message1");
            fileLogger.LogWarning("message2");
            fileLogger.LogWarning("message3");

            // Assert
            Assert.Equal(3, fileLogger.Messages.Count);
            Assert.Contains("message1", fileLogger.Messages[0]);
            Assert.Contains("message2", fileLogger.Messages[1]);
            Assert.Contains("message3", fileLogger.Messages[2]);

            fileLogger.Dispose();
            Assert.True(File.Exists(logFile));
            var lines = File.ReadAllLines(logFile);
            Assert.Equal(3, lines.Length);
            Assert.Contains("message1", lines[0]);
            Assert.Contains("message2", lines[1]);
            Assert.Contains("message3", lines[2]);
        }

        [Fact]
        public void WritesLog_WithCategoryInformation()
        {
            // Arrange
            var logFile = Path.Combine(_rooDirPath, Guid.NewGuid().ToString());
            var fileLogger = new FileLogger("category1", logFile, LogLevel.Warning);

            // Act
            fileLogger.LogWarning("message1");

            // Assert
            var message = Assert.Single(fileLogger.Messages);
            Assert.Contains("category1", message);
        }

        [Fact]
        public void WritesLog_WithLogLevelInformation()
        {
            // Arrange
            var logFile = Path.Combine(_rooDirPath, Guid.NewGuid().ToString());
            var fileLogger = new FileLogger("category1", logFile, LogLevel.Warning);

            // Act
            fileLogger.LogWarning("message1");

            // Assert
            var message = Assert.Single(fileLogger.Messages);
            Assert.Contains("Warning", message);
        }

        [Fact]
        public void DoesNotCreateLogFile_WhenThereAreNoMessagesToWrite()
        {
            // Arrange
            var logFile = Path.Combine(_rooDirPath, Guid.NewGuid().ToString());
            var fileLogger = new FileLogger("category1", logFile, LogLevel.Warning);

            // Act
            fileLogger.Dispose();

            // Assert
            Assert.False(File.Exists(logFile));
        }

        public class FileLoggerTestFixutre : IDisposable
        {
            public FileLoggerTestFixutre()
            {
                RootDirPath = Path.Combine(Path.GetTempPath(), "BuildScriptGeneratorCliTests", nameof(FileLoggerTest));

                Directory.CreateDirectory(RootDirPath);
            }

            public string RootDirPath { get; }

            public void Dispose()
            {
                if (Directory.Exists(RootDirPath))
                {
                    try
                    {
                        Directory.Delete(RootDirPath, recursive: true);
                    }
                    catch
                    {
                        // Do not throw in dispose
                    }
                }
            }
        }
    }
}
