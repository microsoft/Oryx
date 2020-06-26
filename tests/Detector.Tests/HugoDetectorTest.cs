// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Oryx.Detector.Hugo;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.Detector.Tests
{
    public class HugoDetectorTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRootPath;

        public HugoDetectorTest(TestTempDirTestFixture testFixture)
        {
            _tempDirRootPath = testFixture.RootDirPath;
        }

        [Theory]
        [InlineData(HugoConstants.TomlFileName)]
        [InlineData(HugoConstants.ConfigFolderName, HugoConstants.TomlFileName)]
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
        [InlineData(HugoConstants.JsonFileName)]
        [InlineData(HugoConstants.ConfigFolderName, HugoConstants.JsonFileName)]
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
            Assert.Null(result.PlatformVersion);
        }

        [Theory]
        [InlineData(HugoConstants.YamlFileName)]
        [InlineData(HugoConstants.ConfigFolderName, HugoConstants.YamlFileName)]
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
            Assert.Null(result.PlatformVersion);
        }

        [Theory]
        [InlineData(HugoConstants.YmlFileName)]
        [InlineData(HugoConstants.ConfigFolderName, HugoConstants.YmlFileName)]
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
            Assert.Null(result.PlatformVersion);
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
