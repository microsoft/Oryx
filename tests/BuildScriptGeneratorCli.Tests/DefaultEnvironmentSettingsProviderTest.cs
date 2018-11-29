// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Oryx.Tests.Infrastructure;
using Xunit;

namespace BuildScriptGeneratorCli.Tests
{
    public class DefaultEnvironmentSettingsProviderTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRoot;

        public DefaultEnvironmentSettingsProviderTest(TestTempDirTestFixture tempDirFixture)
        {
            _tempDirRoot = tempDirFixture.RootDirPath;
        }

        [Fact]
        public void GetSettings_ReturnsSettings_IgnoringBlankLinesAndComments()
        {
            // Arrange
            var sourceDir = CreateNewDir();
            WriteEnvFile(sourceDir, "   ", "", "#foo=bar", "key1=value1", "key2=value2");
            var provider = CreateProvider(sourceDir);

            // Act
            var settings = provider.ReadSettingsFromFile();

            // Assert
            Assert.NotNull(settings);
            Assert.Equal(2, settings.Count);
            Assert.True(settings.TryGetValue("key1", out var value));
            Assert.Equal("value1", value);
            Assert.True(settings.TryGetValue("key2", out value));
            Assert.Equal("value2", value);
        }

        [Fact]
        public void GetSettings_ReturnsSettings_IgnoringInvalidNameValuePairs()
        {
            // Arrange
            var sourceDir = CreateNewDir();
            WriteEnvFile(sourceDir, "key1=value1", "key2=value2", "=value3");
            var provider = CreateProvider(sourceDir);

            // Act
            var settings = provider.ReadSettingsFromFile();

            // Assert
            Assert.NotNull(settings);
            Assert.Equal(2, settings.Count);
            Assert.True(settings.TryGetValue("key1", out var value));
            Assert.Equal("value1", value);
            Assert.True(settings.TryGetValue("key2", out value));
            Assert.Equal("value2", value);
        }

        [Fact]
        public void GetSettings_ReturnsSettings_ConsideringCaseSensitiveness()
        {
            // Arrange
            var sourceDir = CreateNewDir();
            WriteEnvFile(sourceDir, "key1=value1", "kEy1=value2");
            var provider = CreateProvider(sourceDir);

            // Act
            var settings = provider.ReadSettingsFromFile();

            // Assert
            Assert.NotNull(settings);
            Assert.Equal(2, settings.Count);
            Assert.True(settings.TryGetValue("key1", out var value));
            Assert.Equal("value1", value);
            Assert.True(settings.TryGetValue("kEy1", out value));
            Assert.Equal("value2", value);
        }

        [Fact]
        public void IsValid_ReturnsFalse_IfPreBuildScriptPath_IsNotPresent()
        {
            // Arrange
            var environmentSettings = new EnvironmentSettings();
            environmentSettings.PreBuildScriptPath = Path.Combine(_tempDirRoot, Guid.NewGuid().ToString());
            var sourceDir = CreateNewDir();
            var provider = CreateProvider(sourceDir);

            // Act
            var settings = provider.IsValid(environmentSettings);

            // Assert
            Assert.False(settings);
        }

        [Fact]
        public void IsValid_ReturnsFalse_IfPostBuildScriptPath_IsNotPresent()
        {
            // Arrange
            var environmentSettings = new EnvironmentSettings();
            environmentSettings.PostBuildScriptPath = Path.Combine(_tempDirRoot, Guid.NewGuid().ToString());
            var sourceDir = CreateNewDir();
            var provider = CreateProvider(sourceDir);

            // Act
            var settings = provider.IsValid(environmentSettings);

            // Assert
            Assert.False(settings);
        }

        [Fact]
        public void IsValid_ReturnsTrue_EvenIfSettingsFile_DoesNotHaveOurSettings()
        {
            // Arrange
            var environmentSettings = new EnvironmentSettings();
            var sourceDir = CreateNewDir();
            var provider = CreateProvider(sourceDir);

            // Act
            var settings = provider.IsValid(environmentSettings);

            // Assert
            Assert.True(settings);
        }

        private string CreateNewDir()
        {
            return Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString())).FullName;
        }

        private void WriteEnvFile(string sourceDir, params string[] contentLines)
        {
            File.WriteAllText(Path.Combine(sourceDir, ".env"), string.Join(Environment.NewLine, contentLines));
        }

        private DefaultEnvironmentSettingsProvider CreateProvider(string sourceDir)
        {
            return new DefaultEnvironmentSettingsProvider(
                new TestSourceRepoProvider(sourceDir),
                new TestConsole(),
                NullLogger<DefaultEnvironmentSettingsProvider>.Instance);
        }

        private class TestSourceRepoProvider : ISourceRepoProvider
        {
            private readonly string _sourceDir;

            public TestSourceRepoProvider(string sourceDir)
            {
                _sourceDir = sourceDir;
            }

            public ISourceRepo GetSourceRepo()
            {
                return new LocalSourceRepo(_sourceDir);
            }
        }
    }
}
