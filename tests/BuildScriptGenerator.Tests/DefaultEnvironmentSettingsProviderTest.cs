// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class DefaultEnvironmentSettingsProviderTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRoot;

        public DefaultEnvironmentSettingsProviderTest(TestTempDirTestFixture tempDirFixture)
        {
            _tempDirRoot = tempDirFixture.RootDirPath;
        }

        [Fact]
        public void TryGetAndLoadSettings_TrimsQuotesAndWhitespace()
        {
            // Arrange
            var sourceDir = CreateNewDir();
            var scriptFile = Path.Combine(sourceDir, "a b.sh");
            File.Create(scriptFile);
            var testEnvironment = new TestEnvironment();
            testEnvironment.SetEnvironmentVariable(EnvironmentSettingsKeys.PreBuildScript, " \" a b c \" ");
            testEnvironment.SetEnvironmentVariable(
                EnvironmentSettingsKeys.PreBuildScriptPath,
                $" \"{scriptFile}\" ");
            testEnvironment.SetEnvironmentVariable(EnvironmentSettingsKeys.PostBuildScript, " \" a b c \" ");
            testEnvironment.SetEnvironmentVariable(
                EnvironmentSettingsKeys.PostBuildScriptPath,
                $" \"{scriptFile}\" ");
            var provider = CreateProvider(sourceDir, testEnvironment);

            // Act
            provider.TryGetAndLoadSettings(out var settings);

            // Assert
            Assert.Equal(" a b c ", settings.PreBuildScript);
            Assert.Equal(scriptFile, settings.PreBuildScriptPath);
            Assert.Equal(" a b c ", settings.PostBuildScript);
            Assert.Equal(scriptFile, settings.PostBuildScriptPath);
        }

        [Theory]
        [InlineData("\"")]
        [InlineData("a\"")]
        [InlineData("a\"\"")]
        public void TryGetAndLoadSettings_TrimsOnlyWhenMatchingQuotesAreFound(string value)
        {
            // Arrange
            var sourceDir = CreateNewDir();
            var testEnvironment = new TestEnvironment();
            testEnvironment.SetEnvironmentVariable(EnvironmentSettingsKeys.PreBuildScript, value);
            var provider = CreateProvider(sourceDir, testEnvironment);

            // Act
            provider.TryGetAndLoadSettings(out var settings);

            // Assert
            Assert.Equal(value, settings.PreBuildScript);
        }

        [Fact]
        public void TryGetAndLoadSettings_PrefersPrefixedName_IfPresent()
        {
            // Arrange
            var sourceDir = CreateNewDir();
            var preBuildPath1 = Path.Combine(sourceDir, "foo.sh");
            var preBuildPath2 = Path.Combine(sourceDir, "bar.sh");
            File.Create(preBuildPath1);
            File.Create(preBuildPath2);
            WriteEnvFile(
                sourceDir,
                "PRE_BUILD_SCRIPT_PATH=bar.sh",
                "ORYX_PRE_BUILD_SCRIPT_PATH=foo.sh");
            var provider = CreateProvider(sourceDir);

            // Act
            var result = provider.TryGetAndLoadSettings(out var environmentSettings);

            // Assert
            Assert.True(result);
            Assert.NotNull(environmentSettings);
            Assert.Equal(preBuildPath1, environmentSettings.PreBuildScriptPath);
            Assert.Null(environmentSettings.PostBuildScriptPath);
        }

        [Fact]
        public void TryGetAndLoadSettings_SetsEnvironmentVariablesInCurrentProcess()
        {
            // Arrange
            var sourceDir = CreateNewDir();
            WriteEnvFile(sourceDir, "PRE_BUILD_SCRIPT_PATH=bar.sh");
            var preBuildPath = Path.Combine(sourceDir, "bar.sh");
            File.Create(preBuildPath);
            var testEnvironment = new TestEnvironment();
            var provider = CreateProvider(sourceDir, testEnvironment);

            // Act
            var result = provider.TryGetAndLoadSettings(out var environmentSettings);

            // Assert
            Assert.True(result);
            var envVariable = Assert.Single(testEnvironment.Variables);
            Assert.Equal("PRE_BUILD_SCRIPT_PATH", envVariable.Key);
            Assert.Equal("bar.sh", envVariable.Value);
            Assert.NotNull(environmentSettings);
            Assert.Equal(preBuildPath, environmentSettings.PreBuildScriptPath);
        }

        [Fact]
        public void ReadSettingsFromFile_DoesNotThrow_IfCouldNotFindSettingsFile()
        {
            // Arrange
            var sourceDir = CreateNewDir();
            var provider = CreateProvider(sourceDir);
            var settings = new Dictionary<string, string>();

            // Act
            provider.ReadSettingsFromFile(settings);

            // Assert
            Assert.Empty(settings);
        }

        [Fact]
        public void ReadSettingsFromFile_DoesNotThrow_IfSettingsFileIsEmpty()
        {
            // Arrange
            var sourceDir = CreateNewDir();
            WriteEnvFile(sourceDir);
            var provider = CreateProvider(sourceDir);
            var settings = new Dictionary<string, string>();

            // Act
            provider.ReadSettingsFromFile(settings);

            // Assert
            Assert.Empty(settings);
        }

        [Fact]
        public void ReadSettingsFromFile_ReturnsSettings_IgnoringBlankLinesAndComments()
        {
            // Arrange
            var sourceDir = CreateNewDir();
            WriteEnvFile(sourceDir, "   ", "", "#foo=bar", "key1=value1", "key2=value2");
            var provider = CreateProvider(sourceDir);
            var settings = new Dictionary<string, string>();

            // Act
            provider.ReadSettingsFromFile(settings);

            // Assert
            Assert.Equal(2, settings.Count);
            Assert.True(settings.TryGetValue("key1", out var value));
            Assert.Equal("value1", value);
            Assert.True(settings.TryGetValue("key2", out value));
            Assert.Equal("value2", value);
        }

        [Fact]
        public void ReadSettingsFromFile_ReturnsSettings_IgnoringInvalidNameValuePairs()
        {
            // Arrange
            var sourceDir = CreateNewDir();
            WriteEnvFile(sourceDir, "key1=value1", "key2=value2", "=value3");
            var provider = CreateProvider(sourceDir);
            var settings = new Dictionary<string, string>();

            // Act
            provider.ReadSettingsFromFile(settings);

            // Assert
            Assert.Equal(2, settings.Count);
            Assert.True(settings.TryGetValue("key1", out var value));
            Assert.Equal("value1", value);
            Assert.True(settings.TryGetValue("key2", out value));
            Assert.Equal("value2", value);
        }

        [Fact]
        public void ReadSettingsFromFile_ReturnsSettings_ConsideringCaseSensitiveness()
        {
            // Arrange
            var sourceDir = CreateNewDir();
            WriteEnvFile(sourceDir, "key1=value1", "kEy1=value2");
            var provider = CreateProvider(sourceDir);
            var settings = new Dictionary<string, string>();

            // Act
            provider.ReadSettingsFromFile(settings);

            // Assert
            Assert.Equal(2, settings.Count);
            Assert.True(settings.TryGetValue("key1", out var value));
            Assert.Equal("value1", value);
            Assert.True(settings.TryGetValue("kEy1", out value));
            Assert.Equal("value2", value);
        }

        [Fact]
        public void IsValid_Throws_IfPreBuildScriptPath_IsNotPresent()
        {
            // Arrange
            var environmentSettings = new EnvironmentSettings();
            environmentSettings.PreBuildScriptPath = Path.Combine(_tempDirRoot, Guid.NewGuid().ToString());
            var sourceDir = CreateNewDir();
            var provider = CreateProvider(sourceDir);

            // Act
            var exception = Assert.Throws<InvalidUsageException>(() => provider.IsValid(environmentSettings));

            // Assert
            Assert.Equal(
                $"Pre-build script file '{environmentSettings.PreBuildScriptPath}' does not exist.",
                exception.Message);
        }

        [Fact]
        public void IsValid_Throws_IfPostBuildScriptPath_IsNotPresent()
        {
            // Arrange
            var environmentSettings = new EnvironmentSettings();
            environmentSettings.PostBuildScriptPath = Path.Combine(_tempDirRoot, Guid.NewGuid().ToString());
            var sourceDir = CreateNewDir();
            var provider = CreateProvider(sourceDir);

            // Act
            var exception = Assert.Throws<InvalidUsageException>(() => provider.IsValid(environmentSettings));

            // Assert
            Assert.Equal(
                $"Post-build script file '{environmentSettings.PostBuildScriptPath}' does not exist.",
                exception.Message);
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

        [Fact]
        public void MergeSettingsFromEnvironmentVariables_OverridesExistingSetting()
        {
            // Arrange
            var sourceDir = CreateNewDir();
            var settings = new Dictionary<string, string>(StringComparer.Ordinal);
            settings["key1"] = "value1";
            settings["key2"] = "value2";
            var testEnvironment = new TestEnvironment();
            testEnvironment.SetEnvironmentVariable("key1", "value1-new");
            var provider = CreateProvider(sourceDir, testEnvironment);

            // Act
            provider.MergeSettingsFromEnvironmentVariables(settings);

            // Assert
            Assert.Equal(2, settings.Count);
            Assert.True(settings.TryGetValue("key1", out var value));
            Assert.Equal("value1-new", value);
            Assert.True(settings.TryGetValue("key2", out value));
            Assert.Equal("value2", value);
        }

        [Fact]
        public void MergeSettingsFromEnvironmentVariables_OverrideIsCaseSensitive()
        {
            // Arrange
            var sourceDir = CreateNewDir();
            var settings = new Dictionary<string, string>(StringComparer.Ordinal);
            settings["key1"] = "value1";
            var testEnvironment = new TestEnvironment();
            testEnvironment.SetEnvironmentVariable("kEy1", "value1-new");
            var provider = CreateProvider(sourceDir, testEnvironment);

            // Act
            provider.MergeSettingsFromEnvironmentVariables(settings);

            // Assert
            var kvp = Assert.Single(settings);
            Assert.Equal("key1", kvp.Key);
            Assert.Equal("value1", kvp.Value);
        }

        [Fact]
        public void MergeSettingsFromEnvironmentVariables_DoesNotPopulateSettings_ThatAreNotAlreadyPresent()
        {
            // Arrange
            var sourceDir = CreateNewDir();
            var settings = new Dictionary<string, string>(StringComparer.Ordinal);
            var testEnvironment = new TestEnvironment();
            testEnvironment.SetEnvironmentVariable("key1", "value1");
            var provider = CreateProvider(sourceDir, testEnvironment);

            // Act
            provider.MergeSettingsFromEnvironmentVariables(settings);

            // Assert
            Assert.Empty(settings);
        }

        [Fact]
        public void GetSettings_FollowsPrecedenceOrder()
        {
            // Arrange
            var settings = new Dictionary<string, string>(StringComparer.Ordinal);
            var sourceDir = CreateNewDir();
            // From source repo's build.env file
            WriteEnvFile(sourceDir, "key1=value1-buildenv", "key2=value2-buildenv", "foo=bar");
            // From environment variables
            var environment = new TestEnvironment();
            environment.Variables["key2"] = "value2-envvariable";
            environment.Variables["size"] = "small";
            var provider = CreateProvider(sourceDir, environment);

            // Act
            provider.GetSettings(settings);

            // Assert
            Assert.Equal(3, settings.Count);
            Assert.True(settings.TryGetValue("key1", out var value));
            Assert.Equal("value1-buildenv", value);
            Assert.True(settings.TryGetValue("key2", out value));
            Assert.Equal("value2-envvariable", value);
            Assert.True(settings.TryGetValue("foo", out value));
            Assert.Equal("bar", value);
            Assert.False(settings.TryGetValue("size", out value));
        }

        private string CreateNewDir()
        {
            return Directory.CreateDirectory(Path.Combine(_tempDirRoot, Guid.NewGuid().ToString())).FullName;
        }

        private void WriteEnvFile(string sourceDir, params string[] contentLines)
        {
            File.WriteAllText(Path.Combine(sourceDir, Constants.BuildEnvironmentFileName), string.Join(Environment.NewLine, contentLines));
        }

        private DefaultEnvironmentSettingsProvider CreateProvider(string sourceDir)
        {
            return CreateProvider(sourceDir, new TestEnvironment());
        }

        private DefaultEnvironmentSettingsProvider CreateProvider(string sourceDir, TestEnvironment testEnvironment)
        {
            return new DefaultEnvironmentSettingsProvider(
                new TestSourceRepoProvider(sourceDir),
                testEnvironment,
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
                return new LocalSourceRepo(_sourceDir, NullLoggerFactory.Instance);
            }
        }
    }
}
