// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Node
{
    public class NodePlatformDetectorTest
    {
        [Fact]
        public void Detect_ReturnsNull_IfSourceDirectory_DoesNotHaveAnyFiles()
        {
            // Arrange
            var version = "8.11.2";
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: new[] { version },
                defaultVersion: version,
                new NodeScriptGeneratorOptions());
            // No files in source directory
            var repo = new MemorySourceRepo();
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsNull_ForSourceRepoWithServerJs_NotInRootDirectory()
        {
            // Arrange
            var version = "8.11.2";
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: new[] { version },
                defaultVersion: version,
                new NodeScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            repo.AddFile(SamplePackageJsonContents.SimpleServerJs, "subDir1", "server.js");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsNull_ForSourceRepoWithAppJs_NotInRootDirectory()
        {
            // Arrange
            var version = "8.11.2";
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: new[] { version },
                defaultVersion: version,
                new NodeScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            repo.AddFile(SamplePackageJsonContents.SimpleServerJs, "subDir1", "app.js");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsNull_ForSourceRepoWithPackageJson_NotInRootDirectory()
        {
            // Arrange
            var version = "8.11.2";
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: new[] { version },
                defaultVersion: version,
                new NodeScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            repo.AddFile(
                SamplePackageJsonContents.PackageJsonWithNodeVersion,
                "subDir1",
                NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsVersionFromPackageJson_IfEnvironmentVariableValueIsNotPresent()
        {
            // Arrange
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: new[] { "6.11.0", "8.11.2" },
                defaultVersion: "8.11.2",
                new NodeScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            var packageJson = SamplePackageJsonContents.PackageJsonTemplateWithNodeVersion.Replace(
                "#VERSION_RANGE#",
                "6.11.0");
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Equal("6.11.0", result.PlatformVersion);
        }

        [Theory]
        [InlineData("default.htm")]
        [InlineData("default.html")]
        [InlineData("default.asp")]
        [InlineData("index.htm")]
        [InlineData("index.html")]
        [InlineData("iisstart.htm")]
        [InlineData("default.aspx")]
        [InlineData("index.php")]
        public void Detect_ReturnsNull_IfIISStartupFileIsPresent(string iisStartupFileName)
        {
            // Arrange
            var version = "8.11.2";
            var sourceRepo = new MemorySourceRepo();
            sourceRepo.AddFile("", iisStartupFileName);
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: new[] { version },
                defaultVersion: version,
                new NodeScriptGeneratorOptions());
            var context = CreateContext(sourceRepo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("default.htm")]
        [InlineData("default.html")]
        [InlineData("default.asp")]
        [InlineData("index.htm")]
        [InlineData("index.html")]
        [InlineData("iisstart.htm")]
        [InlineData("default.aspx")]
        [InlineData("index.php")]
        public void Detect_ReturnsNull_IfServerJs_AndIISStartupFileIsPresent(string iisStartupFileName)
        {
            // Arrange
            var version = "8.11.2";
            var sourceRepo = new MemorySourceRepo();
            sourceRepo.AddFile("", "server.js");
            sourceRepo.AddFile("", iisStartupFileName);
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: new[] { version },
                defaultVersion: version,
                new NodeScriptGeneratorOptions());
            var context = CreateContext(sourceRepo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        private BuildScriptGeneratorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private NodePlatformDetector CreateNodePlatformDetector(
            string[] supportedNodeVersions,
            string defaultVersion,
            NodeScriptGeneratorOptions nodeScriptGeneratorOptions)
        {
            return CreateNodePlatformDetector(
                supportedNodeVersions,
                defaultVersion,
                new TestEnvironment(),
                nodeScriptGeneratorOptions);
        }

        private NodePlatformDetector CreateNodePlatformDetector(
            string[] supportedNodeVersions,
            string defaultVersion,
            IEnvironment environment,
            NodeScriptGeneratorOptions nodeScriptGeneratorOptions)
        {
            nodeScriptGeneratorOptions = nodeScriptGeneratorOptions ?? new NodeScriptGeneratorOptions();

            return new NodePlatformDetector(
                new TestNodeVersionProvider(supportedNodeVersions, defaultVersion),
                Options.Create(nodeScriptGeneratorOptions),
                NullLogger<NodePlatformDetector>.Instance,
                environment,
                new DefaultStandardOutputWriter());
        }

        private class TestNodeVersionProvider : INodeVersionProvider
        {
            private readonly string[] _supportedNodeVersions;
            private readonly string _defaultVersion;

            public TestNodeVersionProvider(string[] supportedNodeVersions, string defaultVersion)
            {
                _supportedNodeVersions = supportedNodeVersions;
                _defaultVersion = defaultVersion;
            }

            public PlatformVersionInfo GetVersionInfo()
            {
                return PlatformVersionInfo.CreateOnDiskVersionInfo(_supportedNodeVersions, _defaultVersion);
            }
        }
    }
}
