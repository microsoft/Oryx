// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGeneratorCli.Logging;
using Xunit;

namespace BuildScriptGeneratorCli.Tests
{
    public class FileLoggerProviderTest : IClassFixture<FileLoggerProviderTest.FileLoggerProviderTestFixutre>
    {
        private readonly string _rooDirPath;

        public FileLoggerProviderTest(FileLoggerProviderTestFixutre testFixture)
        {
            _rooDirPath = testFixture.RootDirPath;
        }

        [Fact]
        public void DoesNotCreateFileLogger_WhenLogFilePathIsNull()
        {
            // Arrange
            var provider = CreateFileLoggerProvider(logFile: null, LogLevel.Critical);

            // Act
            var logger = provider.CreateLogger("category1");

            // Assert
            Assert.IsNotType<FileLogger>(logger);
        }

        [Fact]
        public void DoesNotCreateFileLogger_WhenLogFilePathIsEmpty()
        {
            // Arrange
            var provider = CreateFileLoggerProvider(logFile: string.Empty, LogLevel.Critical);

            // Act
            var logger = provider.CreateLogger("category1");

            // Assert
            Assert.IsNotType<FileLogger>(logger);
        }

        [Fact]
        public void DoesNotFlushMessages_OnDispose_WhenLogFilePathIsNull()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions
            {
                LogFile = null,
                MinimumLogLevel = LogLevel.Critical
            };
            var provider = new TestFileLoggerProvider(Options.Create(options));
            var logger = provider.CreateLogger("category1");
            logger.LogError("message1");

            // Act
            provider.Dispose();

            // Assert
            var testLogger = Assert.IsType<TestLogger>(logger);
            Assert.False(testLogger.IsDisposeCalled); ;
        }

        [Fact]
        public void DoesNotFlushMessages_OnDispose_WhenLogFilePathIsEmpty()
        {
            // Arrange
            var options = new BuildScriptGeneratorOptions
            {
                LogFile = string.Empty,
                MinimumLogLevel = LogLevel.Critical
            };
            var provider = new TestFileLoggerProvider(Options.Create(options));
            var logger = provider.CreateLogger("category1");
            logger.LogError("message1");

            // Act
            provider.Dispose();

            // Assert
            var testLogger = Assert.IsType<TestLogger>(logger);
            Assert.False(testLogger.IsDisposeCalled); ;
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
            Assert.Equal(logFile, fileLogger.LogFile);
            Assert.Equal(LogLevel.Critical, fileLogger.MinimumLogLevel);
        }

        [Fact]
        public void Dispose_CallsDiposesOn_AllCreatedFileLoggers()
        {
            // Arrange & Act
            var logFile = Path.Combine(_rooDirPath, Guid.NewGuid().ToString());
            var provider = CreateFileLoggerProvider(logFile, LogLevel.Warning);
            var logger1 = provider.CreateLogger("category1");
            var logger2 = provider.CreateLogger("category1");

            // Act-1
            logger1.LogError("logger1 message");
            logger2.LogError("logger2 message");

            // Assert-1
            Assert.False(File.Exists(logFile));

            // Act-2
            provider.Dispose();

            // Assert-2
            Assert.True(File.Exists(logFile));
            var lines = File.ReadAllLines(logFile);
            Assert.Equal(2, lines.Length);
            Assert.Contains("logger1 message", lines[0]);
            Assert.Contains("logger2 message", lines[1]);
        }

        private FileLoggerProvider CreateFileLoggerProvider(string logFile, LogLevel minimumLogLevel)
        {
            var options = new BuildScriptGeneratorOptions
            {
                LogFile = logFile,
                MinimumLogLevel = minimumLogLevel
            };
            return new FileLoggerProvider(Options.Create(options));
        }

        public class FileLoggerProviderTestFixutre : IDisposable
        {
            public FileLoggerProviderTestFixutre()
            {
                RootDirPath = Path.Combine(Path.GetTempPath(), "BuildScriptGeneratorCliTests", nameof(FileLoggerProviderTest));

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

        private class TestFileLoggerProvider : FileLoggerProvider
        {
            private readonly bool _throwOnDispose;

            public TestFileLoggerProvider(IOptions<BuildScriptGeneratorOptions> options)
                : base(options)
            {
            }

            public override ILogger CreateLogger(string categoryName)
            {
                return new TestLogger();
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
