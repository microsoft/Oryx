// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    public class PrepareEnvironmentCommandTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRootPath;

        public PrepareEnvironmentCommandTest(TestTempDirTestFixture testTempDirTestFixture)
        {
            _tempDirRootPath = testTempDirTestFixture.RootDirPath;
        }

        [Fact]
        public void TryParseReturns_DoesCaseInsensitiveComparisonOfPlatformName()
        {
            // Arrange
            var suppliedPlatformsAndVersions = "PlatForm1=3.2.2, PLATFORM2=3.1.2";
            var p1 = new TestProgrammingPlatform(
                name: "platform1",
                resolvedVersion: "3.2.2");
            var p2 = new TestProgrammingPlatform(
                name: "platform2",
                resolvedVersion: "3.1.2");

            // Act
            var actual = PrepareEnvironmentCommand.TryValidateSuppliedPlatformsAndVersions(
                new[] { p1, p2 },
                suppliedPlatformsAndVersions,
                suppliedPlatformsAndVersionsFile: null,
                new TestConsole(),
                GetContext(),
                out var results);

            // Assert
            Assert.True(actual);
            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
            Assert.Equal("platform1", results[0].Platform);
            Assert.Equal("3.2.2", results[0].PlatformVersion);
            Assert.Equal("platform2", results[1].Platform);
            Assert.Equal("3.1.2", results[1].PlatformVersion);
        }

        [Fact]
        public void TryParseReturns_ReturnsFalseWhenPlatformNameIsNotValid()
        {
            // Arrange
            var suppliedPlatformsAndVersions = "foo=3.2.2, p1=3.1.2";
            var p1 = new TestProgrammingPlatform(
                name: "p1",
                resolvedVersion: "2.1.3");

            // Act
            var actual = PrepareEnvironmentCommand.TryValidateSuppliedPlatformsAndVersions(
                new[] { p1 },
                suppliedPlatformsAndVersions,
                suppliedPlatformsAndVersionsFile: null,
                new TestConsole(),
                GetContext(),
                out var results);

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void TryParseReturnsFalse_IfSuppliedFileDoesNotExist()
        {
            // Arrange
            var versionsToBeInstalledFile = Path.Combine(_tempDirRootPath, "doestNotExist.txt");
            var p1 = new TestProgrammingPlatform(
                name: "p1",
                resolvedVersion: "2.1.3");

            // Act
            var actual = PrepareEnvironmentCommand.TryValidateSuppliedPlatformsAndVersions(
                new[] { p1 },
                suppliedPlatformsAndVersions: null,
                suppliedPlatformsAndVersionsFile: versionsToBeInstalledFile,
                new TestConsole(),
                GetContext(),
                out var results);

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void TryParseReturns_ReturnsFalseWhenPlatformNameIsNotValidAndInputIsFromFile()
        {
            // Arrange
            var newLine = Environment.NewLine;
            var versionsToBeInstalledFile = Path.Combine(_tempDirRootPath, "input.txt");
            File.WriteAllText(
                versionsToBeInstalledFile,
                $"foo=3.2.2{newLine}p1=3.1.2");
            var p1 = new TestProgrammingPlatform(
                name: "p1",
                resolvedVersion: "2.1.3");

            // Act
            var actual = PrepareEnvironmentCommand.TryValidateSuppliedPlatformsAndVersions(
                new[] { p1 },
                suppliedPlatformsAndVersions: null,
                suppliedPlatformsAndVersionsFile: versionsToBeInstalledFile,
                new TestConsole(),
                GetContext(),
                out var results);

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void TryParseReturnsDefaultVersions_WhenNoVersionSetForSinglePlatform()
        {
            // Arrange
            var suppliedPlatformsAndVersions = "p1";
            var p1 = new TestProgrammingPlatform(
                name: "p1",
                resolvedVersion: "2.1.3");
            var p2 = new TestProgrammingPlatform(
                name: "p2",
                resolvedVersion: "3.1.3");

            // Act
            var actual = PrepareEnvironmentCommand.TryValidateSuppliedPlatformsAndVersions(
                new[] { p1, p2 },
                suppliedPlatformsAndVersions,
                suppliedPlatformsAndVersionsFile: null,
                new TestConsole(),
                GetContext(),
                out var results);

            // Assert
            Assert.True(actual);
            Assert.NotNull(results);
            var result = Assert.Single(results);
            Assert.Equal("p1", result.Platform);
            Assert.Equal("2.1.3", result.PlatformVersion);
        }

        [Fact]
        public void TryParseReturnsDefaultVersions_WhenNoVersionSetForMultiplePlatforms()
        {
            // Arrange
            var suppliedPlatformsAndVersions = "p1, p2";
            var p1 = new TestProgrammingPlatform(
                name: "p1",
                resolvedVersion: "2.1.3");
            var p2 = new TestProgrammingPlatform(
                name: "p2",
                resolvedVersion: "3.1.3");

            // Act
            var actual = PrepareEnvironmentCommand.TryValidateSuppliedPlatformsAndVersions(
                new[] { p1, p2 },
                suppliedPlatformsAndVersions,
                suppliedPlatformsAndVersionsFile: null,
                new TestConsole(),
                GetContext(),
                out var results);

            // Assert
            Assert.True(actual);
            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
            Assert.Equal("p1", results[0].Platform);
            Assert.Equal("2.1.3", results[0].PlatformVersion);
            Assert.Equal("p2", results[1].Platform);
            Assert.Equal("3.1.3", results[1].PlatformVersion);
        }

        [Fact]
        public void TryParseReturnsDefaultVersions_WhenNoVersionSetForMultiplePlatformsAndInputIsFromFile()
        {
            // Arrange
            var newLine = Environment.NewLine;
            var versionsToBeInstalledFile = Path.Combine(_tempDirRootPath, "input.txt");
            File.WriteAllText(versionsToBeInstalledFile, $"p1{newLine}p2");
            var p1 = new TestProgrammingPlatform(
                name: "p1",
                resolvedVersion: "2.1.3");
            var p2 = new TestProgrammingPlatform(
                name: "p2",
                resolvedVersion: "3.1.3");

            // Act
            var actual = PrepareEnvironmentCommand.TryValidateSuppliedPlatformsAndVersions(
                new[] { p1, p2 },
                suppliedPlatformsAndVersions: null,
                suppliedPlatformsAndVersionsFile: versionsToBeInstalledFile,
                new TestConsole(),
                GetContext(),
                out var results);

            // Assert
            Assert.True(actual);
            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
            Assert.Equal("p1", results[0].Platform);
            Assert.Equal("2.1.3", results[0].PlatformVersion);
            Assert.Equal("p2", results[1].Platform);
            Assert.Equal("3.1.3", results[1].PlatformVersion);
        }

        [Fact]
        public void TryParse_IgnoresCommentsAndBlankLinesWhenParsingInputFromFile()
        {
            // Arrange
            var newLine = Environment.NewLine;
            var versionsToBeInstalledFile = Path.Combine(_tempDirRootPath, "input.txt");
            File.WriteAllText(
                versionsToBeInstalledFile,
                $"p1{newLine}{newLine}#This is platform2{newLine}p2{newLine}{newLine}");
            var p1 = new TestProgrammingPlatform(
                name: "p1",
                resolvedVersion: "2.1.3");
            var p2 = new TestProgrammingPlatform(
                name: "p2",
                resolvedVersion: "3.1.3");

            // Act
            var actual = PrepareEnvironmentCommand.TryValidateSuppliedPlatformsAndVersions(
                new[] { p1, p2 },
                suppliedPlatformsAndVersions: null,
                suppliedPlatformsAndVersionsFile: versionsToBeInstalledFile,
                new TestConsole(),
                GetContext(),
                out var results);

            // Assert
            Assert.True(actual);
            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
            Assert.Equal("p1", results[0].Platform);
            Assert.Equal("2.1.3", results[0].PlatformVersion);
            Assert.Equal("p2", results[1].Platform);
            Assert.Equal("3.1.3", results[1].PlatformVersion);
        }

        [Fact]
        public void TryParseReturnsResolvedVersions_WhenInputIsFromFile()
        {
            // Arrange
            var newLine = Environment.NewLine;
            var versionsToBeInstalledFile = Path.Combine(_tempDirRootPath, "input.txt");
            File.WriteAllText(versionsToBeInstalledFile, $"p1=2.1.3{newLine}p2=3.1.3{newLine}");
            var p1 = new TestProgrammingPlatform(
                name: "p1",
                detectedVersion: "1.0.0",
                resolvedVersion: "2.1.3");
            var p2 = new TestProgrammingPlatform(
                name: "p2",
                detectedVersion: "1.0.0",
                resolvedVersion: "3.1.3");

            // Act
            var actual = PrepareEnvironmentCommand.TryValidateSuppliedPlatformsAndVersions(
                new[] { p1, p2 },
                suppliedPlatformsAndVersions: null,
                suppliedPlatformsAndVersionsFile: versionsToBeInstalledFile,
                new TestConsole(),
                GetContext(),
                out var results);

            // Assert
            Assert.True(actual);
            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
            Assert.Equal("p1", results[0].Platform);
            Assert.Equal("2.1.3", results[0].PlatformVersion);
            Assert.Equal("p2", results[1].Platform);
            Assert.Equal("3.1.3", results[1].PlatformVersion);
        }

        [Fact]
        public void TryParseReturnsFalseWhenNoInputsAreProvided()
        {
            // Arrange
            var p1 = new TestProgrammingPlatform(
                name: "p1",
                detectedVersion: "1.0.0",
                resolvedVersion: "2.1.3");
            var p2 = new TestProgrammingPlatform(
                name: "p2",
                detectedVersion: "1.0.0",
                resolvedVersion: "3.1.3");

            // Act
            var actual = PrepareEnvironmentCommand.TryValidateSuppliedPlatformsAndVersions(
                new[] { p1, p2 },
                suppliedPlatformsAndVersions: null,
                suppliedPlatformsAndVersionsFile: null,
                new TestConsole(),
                GetContext(),
                out var results);

            // Assert
            Assert.False(actual);
        }

        private BuildScriptGeneratorContext GetContext()
        {
            return new BuildScriptGeneratorContext();
        }


        private class TestProgrammingPlatform : IProgrammingPlatform
        {
            private readonly string _name;
            private readonly bool _canBeDetected;
            private readonly string _detectedVersion;
            private readonly string _resolvedVersion;

            public TestProgrammingPlatform(
                string name,
                string resolvedVersion,
                bool canBeDetected = true,
                string detectedVersion = null)
            {
                _name = name;
                _canBeDetected = canBeDetected;
                _detectedVersion = detectedVersion;
                _resolvedVersion = resolvedVersion;
            }

            public string Name => _name;

            public IEnumerable<string> SupportedVersions => throw new NotImplementedException();

            public PlatformDetectorResult Detect(RepositoryContext context)
            {
                if (!_canBeDetected)
                {
                    return null;
                }

                return new PlatformDetectorResult
                {
                    Platform = Name,
                    PlatformVersion = _detectedVersion
                };
            }

            public BuildScriptSnippet GenerateBashBuildScriptSnippet(
                BuildScriptGeneratorContext
                scriptGeneratorContext,
                PlatformDetectorResult detectorResult)
            {
                throw new NotImplementedException();
            }

            public string GenerateBashRunTimeInstallationScript(RunTimeInstallationScriptGeneratorOptions options)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<string> GetDirectoriesToExcludeFromCopyToBuildOutputDir(
                BuildScriptGeneratorContext scriptGeneratorContext)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<string> GetDirectoriesToExcludeFromCopyToIntermediateDir(
                BuildScriptGeneratorContext scriptGeneratorContext)
            {
                throw new NotImplementedException();
            }

            public string GetInstallerScriptSnippet(
                BuildScriptGeneratorContext context,
                PlatformDetectorResult detectorResult)
            {
                throw new NotImplementedException();
            }

            public IDictionary<string, string> GetToolsToBeSetInPath(
                RepositoryContext context,
                PlatformDetectorResult detectorResult)
            {
                throw new NotImplementedException();
            }

            public bool IsCleanRepo(Oryx.BuildScriptGenerator.ISourceRepo repo)
            {
                throw new NotImplementedException();
            }

            public bool IsEnabled(RepositoryContext ctx)
            {
                throw new NotImplementedException();
            }

            public bool IsEnabledForMultiPlatformBuild(RepositoryContext ctx)
            {
                throw new NotImplementedException();
            }

            public void ResolveVersions(RepositoryContext context, PlatformDetectorResult detectorResult)
            {
                detectorResult.Platform = _name;
                detectorResult.PlatformVersion = _resolvedVersion;
            }
        }
    }
}
