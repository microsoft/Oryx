// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Node
{
    public class NodeLanguageDetectorTest
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
            var detector = CreateNodeLanguageDetector(
                supportedNodeVersions: new[] { "8.11.2" },
                supportedNpmVersions: new[] { "5.4.2" });
            // No files in source directory
            var repo = new CachedSourceRepo();

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsResult_WithNodeLtsVersion_ForSourceRepoOnlyWithServerJs()
        {
            // Arrange
            var detector = CreateNodeLanguageDetector(
                supportedNodeVersions: new[] { "8.11.2" },
                supportedNpmVersions: new[] { "5.4.2" });
            var repo = new CachedSourceRepo();
            repo.AddFile(SimpleServerJs, "server.js");

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Language);
            Assert.Equal("8.11.2", result.LanguageVersion);
        }

        [Fact]
        public void Detect_ReturnsNull_ForSourceRepoWithServerJs_NotInRootDirectory()
        {
            // Arrange
            var detector = CreateNodeLanguageDetector(
                supportedNodeVersions: new[] { "8.11.2" },
                supportedNpmVersions: new[] { "5.4.2" });
            var repo = new CachedSourceRepo();
            repo.AddFile(SimpleServerJs, "subDir1", "server.js");

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsResult_WithNodeLtsVersion_ForSourceRepoOnlyWithAppJs()
        {
            // Arrange
            var detector = CreateNodeLanguageDetector(
                supportedNodeVersions: new[] { "8.11.2" },
                supportedNpmVersions: new[] { "5.4.2" });
            var repo = new CachedSourceRepo();
            repo.AddFile("app.js content", "app.js");

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Language);
            Assert.Equal("8.11.2", result.LanguageVersion);
        }

        [Fact]
        public void Detect_ReturnsNull_ForSourceRepoWithAppJs_NotInRootDirectory()
        {
            // Arrange
            var detector = CreateNodeLanguageDetector(
                supportedNodeVersions: new[] { "8.11.2" },
                supportedNpmVersions: new[] { "5.4.2" });
            var repo = new CachedSourceRepo();
            repo.AddFile(SimpleServerJs, "subDir1", "app.js");

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsResult_WithNodeLtsVersion_ForPackageJsonWithNoVersion()
        {
            // Arrange
            var detector = CreateNodeLanguageDetector(
                supportedNodeVersions: new[] { "8.11.2" },
                supportedNpmVersions: new[] { "5.4.2" });
            var repo = new CachedSourceRepo();
            repo.AddFile(PackageJsonWithNoVersions, Constants.PackageJsonFileName);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Language);
            Assert.Equal("8.11.2", result.LanguageVersion);
        }

        [Fact]
        public void Detect_ReturnsNull_ForSourceRepoWithPackageJson_NotInRootDirectory()
        {
            // Arrange
            var detector = CreateNodeLanguageDetector(
                supportedNodeVersions: new[] { "8.11.2" },
                supportedNpmVersions: new[] { "5.4.2" });
            var repo = new CachedSourceRepo();
            repo.AddFile(PackageJsonWithNodeVersion, "subDir1", Constants.PackageJsonFileName);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsResult_WithNodeLtsVersion_ForPackageJsonWithOnlyNpmVersion()
        {
            // Node detector only looks for node version and not the NPM version. The individual script
            // generator looks for npm version.

            // Arrange
            var detector = CreateNodeLanguageDetector(
                supportedNodeVersions: new[] { "8.11.2" },
                supportedNpmVersions: new[] { "5.4.2" });
            var repo = new CachedSourceRepo();
            repo.AddFile(PackageJsonWithOnlyNpmVersion, Constants.PackageJsonFileName);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Language);
            Assert.Equal("8.11.2", result.LanguageVersion);
        }

        [Fact]
        public void Detect_ReturnsResult_WithNodeVersionFromEnvironmentVariable_ForPackageJsonWithNoNodeVersion()
        {
            // Arrange
            var environment = new TestEnvironment();
            environment.Variables[NodeScriptGeneratorOptionsSetup.NodeJsDefaultVersion] = "500.500.500";
            var detector = CreateNodeLanguageDetector(
                supportedNodeVersions: new[] { "500.500.500" },
                supportedNpmVersions: new[] { "5.4.2" },
                environment);
            var repo = new CachedSourceRepo();
            repo.AddFile(PackageJsonWithNoVersions, Constants.PackageJsonFileName);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Language);
            Assert.Equal("500.500.500", result.LanguageVersion);
        }

        [Fact]
        public void Detect_ReturnsResult_WithNodeVersionSpecified_InPackageJson()
        {
            // Arrange
            var environment = new TestEnvironment();
            environment.Variables[NodeScriptGeneratorOptionsSetup.NodeJsDefaultVersion] = "8.11.2";
            var detector = CreateNodeLanguageDetector(
                supportedNodeVersions: new[] { "6.11.0", "8.11.2" },
                supportedNpmVersions: new[] { "5.4.2" },
                environment);
            var repo = new CachedSourceRepo();
            repo.AddFile(PackageJsonWithNodeVersion, Constants.PackageJsonFileName);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Language);
            Assert.Equal("6.11.0", result.LanguageVersion);
        }

        [Fact]
        public void Detect_ReturnsResult_WithNodeLtsVersion_ForMalformedPackageJson()
        {
            // Arrange
            var detector = CreateNodeLanguageDetector(
                supportedNodeVersions: new[] { "8.11.2" },
                supportedNpmVersions: new[] { "5.4.2" });
            var repo = new CachedSourceRepo();
            repo.AddFile(MalformedPackageJson, Constants.PackageJsonFileName);

            // Act
            var result = detector.Detect(repo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Language);
            Assert.Equal("8.11.2", result.LanguageVersion);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedNodeVersion_IsDetected()
        {
            // Arrange
            var detector = CreateNodeLanguageDetector(
                supportedNodeVersions: new[] { "6.11.0" },
                supportedNpmVersions: new[] { "5.4.2" });
            var repo = new CachedSourceRepo();
            repo.AddFile(PakageJsonWithUnsupportedNodeVersion, Constants.PackageJsonFileName);

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => detector.Detect(repo));
            Assert.Equal(
                $"Target Node.js version '20.20.20' is unsupported. Supported versions are: 6.11.0",
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
            var sourceRepo = new CachedSourceRepo();
            sourceRepo.AddFile("", iisStartupFileName);
            var detector = CreateNodeLanguageDetector(
                supportedNodeVersions: new[] { "8.11.2" },
                supportedNpmVersions: new[] { "5.4.2" });

            // Act
            var result = detector.Detect(sourceRepo);

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
            var sourceRepo = new CachedSourceRepo();
            sourceRepo.AddFile("", "server.js");
            sourceRepo.AddFile("", iisStartupFileName);
            var detector = CreateNodeLanguageDetector(
                supportedNodeVersions: new[] { "8.11.2" },
                supportedNpmVersions: new[] { "5.4.2" });

            // Act
            var result = detector.Detect(sourceRepo);

            // Assert
            Assert.Null(result);
        }

        private NodeLanguageDetector CreateNodeLanguageDetector(
            string[] supportedNodeVersions,
            string[] supportedNpmVersions)
        {
            return CreateNodeLanguageDetector(supportedNodeVersions, supportedNpmVersions, new TestEnvironment());
        }

        private NodeLanguageDetector CreateNodeLanguageDetector(
            string[] supportedNodeVersions,
            string[] supportedNpmVersions,
            IEnvironment environment)
        {
            var optionsSetup = new NodeScriptGeneratorOptionsSetup(environment);
            var options = new NodeScriptGeneratorOptions();
            optionsSetup.Configure(options);

            return new NodeLanguageDetector(
                new TestNodeVersionProvider(
                    supportedNodeVersions: supportedNodeVersions,
                    supportedNpmVersions: supportedNpmVersions),
                Options.Create(options),
                NullLogger<NodeLanguageDetector>.Instance);
        }
    }
}
