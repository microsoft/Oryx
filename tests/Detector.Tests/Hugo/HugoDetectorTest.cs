// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Oryx.Detector.Hugo;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.Detector.Tests.Hugo
{
    public class HugoDetectorTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRootPath;

        public HugoDetectorTest(TestTempDirTestFixture testFixture)
        {
            _tempDirRootPath = testFixture.RootDirPath;
        }

        [Theory]
        [InlineData("config.toml")]
        [InlineData(HugoConstants.ConfigFolderName, "config.toml")]
        public void IsHugoApp_ReturnsTrue_ForAppWithConfigTomlFile(params string[] subPaths)
        {
            // Arrange
            var appDir = CreateAppDir();
            WriteFile("archetypeDir=\"test\"", appDir, subPaths);
            var detector = GetDetector();
            var context = GetContext(appDir);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HugoConstants.PlatformName, result.Platform);
            Assert.Null(result.PlatformVersion);
        }

        [Theory]
        [InlineData("hugo.toml")]
        [InlineData(HugoConstants.ConfigFolderName, "hugo.toml")]
        public void IsHugoApp_ReturnsTrue_ForAppWithNewConfigTomlFile(params string[] subPaths)
        {
            // Arrange
            var appDir = CreateAppDir();
            WriteFile("archetypeDir=\"test\"", appDir, subPaths);
            var detector = GetDetector();
            var context = GetContext(appDir);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HugoConstants.PlatformName, result.Platform);
            Assert.Null(result.PlatformVersion);
        }

        public static TheoryData<string> ConfigurationKeyNameData
        {
            get
            {
                var data = new TheoryData<string>();
                foreach (var keyName in HugoDetector.HugoConfigurationKeys)
                {
                    // Key name match should be case-insensitive
                    data.Add(keyName.ToUpper());
                    data.Add(keyName.ToLower());
                }

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(ConfigurationKeyNameData))]
        public void IsHugoApp_ReturnsTrue_ForAllSupportedConfigurationKeys_InTomlFile(string configurationKeyName)
        {
            foreach (string configFileName in HugoConstants.TomlFileNames)
            {
                // Arrange
                var appDir = CreateAppDir();
                WriteFile($"{configurationKeyName}=\"test\"", appDir, configFileName); // config.toml
                var detector = GetDetector();
                var context = GetContext(appDir);

                // Act
                var result = detector.Detect(context);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(HugoConstants.PlatformName, result.Platform);
                Assert.Equal(string.Empty, result.AppDirectory);
                Assert.Null(result.PlatformVersion);

                // Prepare next iteration
                File.Delete(Path.Combine(appDir, configFileName));
            }
        }

        [Theory]
        [InlineData("config.json")]
        [InlineData(HugoConstants.ConfigFolderName, "config.json")]
        public void IsHugoApp_ReturnsTrue_ForAppWithConfigJsonFile(params string[] subPaths)
        {
            // Arrange
            var appDir = CreateAppDir();
            WriteFile("{ \"archetypeDir\" : \"test\" }", appDir, subPaths);
            var detector = GetDetector();
            var context = GetContext(appDir);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HugoConstants.PlatformName, result.Platform);
            Assert.Equal(string.Empty, result.AppDirectory);
            Assert.Null(result.PlatformVersion);
        }

        [Theory]
        [InlineData("hugo.json")]
        [InlineData(HugoConstants.ConfigFolderName, "hugo.json")]
        public void IsHugoApp_ReturnsTrue_ForAppWithNewConfigJsonFile(params string[] subPaths)
        {
            // Arrange
            var appDir = CreateAppDir();
            WriteFile("{ \"archetypeDir\" : \"test\" }", appDir, subPaths);
            var detector = GetDetector();
            var context = GetContext(appDir);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HugoConstants.PlatformName, result.Platform);
            Assert.Equal(string.Empty, result.AppDirectory);
            Assert.Null(result.PlatformVersion);
        }

        [Theory]
        [MemberData(nameof(ConfigurationKeyNameData))]
        public void IsHugoApp_ReturnsTrue_ForAllSupportedConfigurationKeys_InJsonFile(string configurationKeyName)
        {
            foreach (string configFileName in HugoConstants.JsonFileNames)
            {
                // Arrange
                var appDir = CreateAppDir();
                WriteFile($"{{ \"{configurationKeyName}\" : \"test\" }}", appDir, configFileName);
                var detector = GetDetector();
                var context = GetContext(appDir);

                // Act
                var result = detector.Detect(context);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(HugoConstants.PlatformName, result.Platform);
                Assert.Equal(string.Empty, result.AppDirectory);
                Assert.Null(result.PlatformVersion);

                // Prepare next iteration
                File.Delete(Path.Combine(appDir, configFileName));
            }
        }

        [Theory]
        [InlineData("config.yaml")]
        [InlineData(HugoConstants.ConfigFolderName, "config.yaml")]
        public void IsHugoApp_ReturnsTrue_ForAppWithConfigYamlFile(params string[] subPaths)
        {
            // Arrange
            var appDir = CreateAppDir();
            WriteFile("archetypeDir: test", appDir, subPaths);
            var detector = GetDetector();
            var context = GetContext(appDir);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HugoConstants.PlatformName, result.Platform);
            Assert.Equal(string.Empty, result.AppDirectory);
            Assert.Null(result.PlatformVersion);
        }

        [Theory]
        [InlineData("hugo.yaml")]
        [InlineData(HugoConstants.ConfigFolderName, "hugo.yaml")]
        public void IsHugoApp_ReturnsTrue_ForAppWithNewConfigYamlFile(params string[] subPaths)
        {
            // Arrange
            var appDir = CreateAppDir();
            WriteFile("archetypeDir: test", appDir, subPaths);
            var detector = GetDetector();
            var context = GetContext(appDir);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HugoConstants.PlatformName, result.Platform);
            Assert.Equal(string.Empty, result.AppDirectory);
            Assert.Null(result.PlatformVersion);
        }

        [Theory]
        [InlineData("config.yml")]
        [InlineData(HugoConstants.ConfigFolderName, "config.yml")]
        public void IsHugoApp_ReturnsTrue_ForAppWithConfigYmlFile(params string[] subPaths)
        {
            // Arrange
            var appDir = CreateAppDir();
            WriteFile("archetypeDir: test", appDir, subPaths);
            var detector = GetDetector();
            var context = GetContext(appDir);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HugoConstants.PlatformName, result.Platform);
            Assert.Equal(string.Empty, result.AppDirectory);
            Assert.Null(result.PlatformVersion);
        }

        [Theory]
        [InlineData("hugo.yml")]
        [InlineData(HugoConstants.ConfigFolderName, "hugo.yml")]
        public void IsHugoApp_ReturnsTrue_ForAppWithNewConfigYmlFile(params string[] subPaths)
        {
            // Arrange
            var appDir = CreateAppDir();
            WriteFile("archetypeDir: test", appDir, subPaths);
            var detector = GetDetector();
            var context = GetContext(appDir);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HugoConstants.PlatformName, result.Platform);
            Assert.Equal(string.Empty, result.AppDirectory);
            Assert.Null(result.PlatformVersion);
        }

        [Theory]
        [MemberData(nameof(ConfigurationKeyNameData))]
        public void IsHugoApp_ReturnsTrue_ForAllSupportedConfigurationKeys_InYamlFile(string configurationKeyName)
        {
            foreach (string configFileName in HugoConstants.YamlFileNames)
            {
                // Arrange
                var appDir = CreateAppDir();
                WriteFile($"{configurationKeyName}: test", appDir, configFileName); // config.yaml
                var detector = GetDetector();
                var context = GetContext(appDir);

                // Act
                var result = detector.Detect(context);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(HugoConstants.PlatformName, result.Platform);
                Assert.Equal(string.Empty, result.AppDirectory);
                Assert.Null(result.PlatformVersion);

                // Prepare next iteration
                File.Delete(Path.Combine(appDir, configFileName));
            }
        }

        [Theory]
        [InlineData("invalid text", "config.toml")]
        [InlineData("{", "config.json")]
        [InlineData("\"invalid text", "config.yaml")]
        public void Detect_ReturnsNull_AndDoesNotThrow_ForInvalidConfigurationFiles(
            string fileContent,
            params string[] subPaths)
        {
            // Arrange
            var appDir = CreateAppDir();
            WriteFile(fileContent, appDir, subPaths);
            var detector = GetDetector();
            var context = GetContext(appDir);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        private string CreateAppDir()
        {
            return Directory.CreateDirectory(Path.Combine(_tempDirRootPath, Guid.NewGuid().ToString())).FullName;
        }

        private void WriteFile(string fileContent, string appDir, params string[] subPaths)
        {
            var finalPath = Path.Combine(subPaths);
            finalPath = Path.Combine(appDir, finalPath);
            var dir = new FileInfo(finalPath).Directory.FullName;
            Directory.CreateDirectory(dir);
            File.WriteAllText(finalPath, fileContent);
        }

        private HugoDetector GetDetector()
        {
            return new HugoDetector(NullLogger<HugoDetector>.Instance);
        }

        private DetectorContext GetContext(string appDir)
        {
            return new DetectorContext
            {
                SourceRepo = new LocalSourceRepo(appDir),
            };
        }
    }
}
