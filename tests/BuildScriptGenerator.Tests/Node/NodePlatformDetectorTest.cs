// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Node
{
    public class NodePlatformDetectorTest
    {
        private const string PackageJsonWithNoVersions = @"{
          ""name"": ""mynodeapp"",
          ""version"": ""1.0.0"",
          ""description"": ""test app"",
          ""main"": ""server.js"",
          ""scripts"": {
            ""test"": ""echo \""Error: no test specified\"" && exit 1"",
            ""start"": ""node server.js""
          },
          ""author"": ""Dev"",
          ""license"": ""ISC""
        }";

        private const string PackageJsonWithNodeVersion = @"{
          ""name"": ""mynodeapp"",
          ""version"": ""1.0.0"",
          ""description"": ""test app"",
          ""main"": ""server.js"",
          ""scripts"": {
            ""test"": ""echo \""Error: no test specified\"" && exit 1"",
            ""start"": ""node server.js""
          },
          ""author"": ""Dev"",
          ""license"": ""ISC"",
          ""engines"" : { ""node"" : ""6.11.0"" }
        }";

        private const string PackageJsonTemplateWithNodeVersion = @"{
          ""name"": ""mynodeapp"",
          ""version"": ""1.0.0"",
          ""description"": ""test app"",
          ""main"": ""server.js"",
          ""scripts"": {
            ""test"": ""echo \""Error: no test specified\"" && exit 1"",
            ""start"": ""node server.js""
          },
          ""author"": ""Dev"",
          ""license"": ""ISC"",
          ""engines"" : { ""node"" : ""#VERSION_RANGE#"" }
        }";

        private const string PackageJsonWithOnlyNpmVersion = @"{
          ""name"": ""mynodeapp"",
          ""version"": ""1.0.0"",
          ""description"": ""test app"",
          ""main"": ""server.js"",
          ""scripts"": {
            ""test"": ""echo \""Error: no test specified\"" && exit 1"",
            ""start"": ""node server.js""
          },
          ""author"": ""Dev"",
          ""license"": ""ISC"",
          ""engines"" : { ""npm"" : ""5.4.2"" }
        }";

        private const string PakageJsonWithUnsupportedNodeVersion = @"{
          ""name"": ""mynodeapp"",
          ""version"": ""1.0.0"",
          ""description"": ""test app"",
          ""main"": ""server.js"",
          ""scripts"": {
            ""test"": ""echo \""Error: no test specified\"" && exit 1"",

            ""start"": ""node server.js""
          },
          ""author"": ""Dev"",
          ""license"": ""ISC"",
          ""engines"" : { ""node"" : ""20.20.20"" }
        }";

        private const string MalformedPackageJson = @"{
          ""name"": ""mynodeapp"",
          ""version"": ""1.0.0"",
          ""description"": ""test app"",
          ""main"": ""server.js"",
          ""scripts"": {
            ""test"": ""echo ,
            ""start"": ""node server.js""
          },
          ""author"": ""Dev"",
          ""license"": ""ISC""
        }";

        private const string SimpleServerJs = @"
            var http = require(""http"")
            http.createServer(function(req, res) {
                res.writeHead(200, { ""Content-Type"": ""text/plain""});
                res.write(""Test!"");
                res.end();
            }).listen(8888);";

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
        public void Detect_ReturnsResult_WithNodeLtsVersion_ForSourceRepoOnlyWithServerJs()
        {
            // Arrange
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: new[] { NodeConstants.NodeLtsVersion },
                defaultVersion: NodeConstants.NodeLtsVersion,
                new NodeScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            repo.AddFile(SimpleServerJs, "server.js");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Equal(NodeConstants.NodeLtsVersion, result.PlatformVersion);
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
            repo.AddFile(SimpleServerJs, "subDir1", "server.js");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsResult_WithDefaultVersion_ForSourceRepoOnlyWithAppJs()
        {
            // Arrange
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: new[] { NodeConstants.NodeLtsVersion },
                defaultVersion: NodeConstants.NodeLtsVersion,
                new NodeScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            repo.AddFile("app.js content", "app.js");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Equal(NodeConstants.NodeLtsVersion, result.PlatformVersion);
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
            repo.AddFile(SimpleServerJs, "subDir1", "app.js");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsResult_WithDefaultVersion_ForPackageJsonWithNoVersion()
        {
            // Arrange
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: new[] { NodeConstants.NodeLtsVersion },
                defaultVersion: NodeConstants.NodeLtsVersion,
                new NodeScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithNoVersions, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Equal(NodeConstants.NodeLtsVersion, result.PlatformVersion);
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
            repo.AddFile(PackageJsonWithNodeVersion, "subDir1", NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsResult_WithDefaultVersion_ForPackageJsonWithOnlyNpmVersion()
        {
            // Node detector only looks for node version and not the NPM version. The individual script
            // generator looks for npm version.

            // Arrange
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: new[] { NodeConstants.NodeLtsVersion },
                defaultVersion: NodeConstants.NodeLtsVersion,
                new NodeScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithOnlyNpmVersion, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Equal(NodeConstants.NodeLtsVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsResult_WithNodeVersionFromOptions_ForPackageJsonWithNoNodeVersion()
        {
            // Arrange
            var version = "500.500.500";
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: new[] { version },
                defaultVersion: version,
                new NodeScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithNoVersions, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.ResolvedNodeVersion = version;

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Equal(version, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsDefaultVersionOfVersionProvider_IfNoVersionFoundInPackageJson_OrOptions()
        {
            // Arrange
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: new[] { "6.11.0", "8.11.2", "10.14.0" },
                defaultVersion: "8.11.2",
                new NodeScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithNoVersions, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Equal("8.11.2", result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsVersionFromEnvironmentVariable_EvenIfPackageJsonHasVersion()
        {
            // Arrange
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: new[] { "6.11.0", "8.11.2", "10.14.0" },
                defaultVersion: "8.11.2",
                new NodeScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            var packageJson = PackageJsonTemplateWithNodeVersion.Replace("#VERSION_RANGE#", "6.11.0");
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.ResolvedNodeVersion = "10.14.0";

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Equal("10.14.0", result.PlatformVersion);
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
            var packageJson = PackageJsonTemplateWithNodeVersion.Replace("#VERSION_RANGE#", "6.11.0");
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
        [InlineData(new[] { "8.11.2", "5.6.9", "8.11.13" }, "5.6.9", ">5", "8.11.13")]
        [InlineData(new[] { "8.9.5", "8.11.2", "5.6.9", "8.11.13" }, "5.6.9", ">8.9", "8.11.13")]
        [InlineData(new[] { "8.9.5", "8.11.2", "11.12.0", "13.12.12", "8.11.13" }, "5.6.9", ">8.9 <13", "11.12.0")]
        public void Detect_ReturnsResult_WithVersionSatisfying_NodeVersionRangeInPackageJson(
            string[] supportedVersions,
            string defaultVersion,
            string versionRangeInPackageJson,
            string expectedVersion)
        {
            // Arrange
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: supportedVersions,
                defaultVersion: defaultVersion,
                new NodeScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            var packageJson = PackageJsonTemplateWithNodeVersion.Replace("#VERSION_RANGE#", versionRangeInPackageJson);
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsResult_WithDefaultVersion_ForMalformedPackageJson()
        {
            // Arrange
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: new[] { NodeConstants.NodeLtsVersion },
                defaultVersion: NodeConstants.NodeLtsVersion,
                new NodeScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            repo.AddFile(MalformedPackageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Equal(NodeConstants.NodeLtsVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsResult_WithDefaultVersion_ForPackageJsonWithNoExplicitVersionsSpecified()
        {
            // Arrange
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: new[] { NodeConstants.NodeLtsVersion },
                defaultVersion: NodeConstants.NodeLtsVersion,
                new NodeScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithNoVersions, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Equal(NodeConstants.NodeLtsVersion, result.PlatformVersion);
        }

        [Theory]
        [InlineData(new[] { "8.11.1", "8.11.13", "9.10.12" }, "8", "8.11.13")]
        [InlineData(new[] { "8.11.1", "8.11.13", "9.10.12" }, "8.11", "8.11.13")]
        [InlineData(new[] { "8.11.1", "8.11.13", "9.10.12" }, "8.11.1", "8.11.1")]
        public void Detect_ReturnsResult_WithDefaultVersionHavingOnlyMajorAndMinorVersion(
            string[] supportedVersions,
            string defaultVersion,
            string expectedVersion)
        {
            // Arrange
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: supportedVersions,
                defaultVersion: defaultVersion,
                new NodeScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithNoVersions, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedNodeVersion_IsDetected()
        {
            // Arrange
            var version = "6.11.0";
            var detector = CreateNodePlatformDetector(
                supportedNodeVersions: new[] { version },
                defaultVersion: version,
                new NodeScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            var context = CreateContext(repo);
            repo.AddFile(PakageJsonWithUnsupportedNodeVersion, NodeConstants.PackageJsonFileName);

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => detector.Detect(context));
            Assert.Equal(
                $"Platform '{NodeConstants.PlatformName}' version '20.20.20' is unsupported. Supported versions: {version}",
                exception.Message);
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
