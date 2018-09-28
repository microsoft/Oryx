// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGeneratorCli.Logging;
using Xunit;

namespace BuildScriptGeneratorCli.Tests
{
    public class FileLoggerTest
    {
        [Fact]
        public void WriteLogs_ByExpandingParameters()
        {
            // Arrange
            var messageFormat = "a warning message with parameters: {param1} {param2}";
            var expected = "a warning message with parameters: foo bar";
            var fileLogger = new FileLogger("category1", new List<string>(), LogLevel.Warning);

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
            var fileLogger = new FileLogger("category1", new List<string>(), LogLevel.Warning);

            // Act-1
            var isEnabled = fileLogger.IsEnabled(LogLevel.Warning);

            // Assert-1
            Assert.True(isEnabled);

            // Act-2
            fileLogger.LogWarning(expected);

            // Assert-2
            var message = Assert.Single(fileLogger.Messages);
            Assert.Contains(expected, message);
        }

        [Fact]
        public void WritesLogs_WhenLogLevel_IsGreaterThanMinimumLogLevel()
        {
            // Arrange
            var expected = "an error message";
            var fileLogger = new FileLogger("category1", new List<string>(), LogLevel.Warning);

            // Act-1
            var isEnabled = fileLogger.IsEnabled(LogLevel.Error);

            // Assert-1
            Assert.True(isEnabled);

            // Act-2
            fileLogger.LogError(expected);

            // Assert-2
            var message = Assert.Single(fileLogger.Messages);
            Assert.Contains(expected, message);
        }

        [Fact]
        public void DoesNotWritesLogs_WhenLogLevel_IsLessThanMinimumLogLevel()
        {
            // Arrange
            var expected = "a debug message";
            var fileLogger = new FileLogger("category1", new List<string>(), LogLevel.Warning);

            // Act-1
            var isEnabled = fileLogger.IsEnabled(LogLevel.Debug);

            // Assert-1
            Assert.False(isEnabled);

            // Act-2
            fileLogger.LogDebug(expected);

            // Assert-2
            Assert.Empty(fileLogger.Messages);
        }

        [Fact]
        public void WritesLog_WithExceptionDetails()
        {
            // Arrange
            var fileLogger = new FileLogger("category1", new List<string>(), LogLevel.Warning);

            // Act
            fileLogger.LogError(new InvalidOperationException("foo"), "an error message with exception");

            // Assert
            var message = Assert.Single(fileLogger.Messages);
            Assert.Contains("foo", message);
            Assert.Contains("an error message with exception", message);
        }

        [Fact]
        public void WritesLog_WithCategoryInformation()
        {
            // Arrange
            var fileLogger = new FileLogger("category1", new List<string>(), LogLevel.Warning);

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
            var fileLogger = new FileLogger("category1", new List<string>(), LogLevel.Warning);

            // Act
            fileLogger.LogWarning("message1");

            // Assert
            var message = Assert.Single(fileLogger.Messages);
            Assert.Contains("Warning", message);
        }
    }
}
