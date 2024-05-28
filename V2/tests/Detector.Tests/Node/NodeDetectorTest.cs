// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Detector.Node;
using Xunit;

namespace Microsoft.Oryx.Detector.Tests.Node
{
    public class NodeDetectorTest
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

        private const string PackageJsonWithFrameworks = @"{
          ""name"": ""mynodeapp"",
          ""version"": ""1.0.0"",
          ""main"": ""server.js"",
          ""author"": ""Dev"",
          ""license"": ""ISC"",
          ""devDependencies"": {
            ""aurelia-cli"": ""1.3.1"",
            ""svelte"": ""3.0.0"",
          },
          ""dependencies"": {
            ""jquery"": ""3.5.1"",
            ""react"": ""16.12.0"",
            ""@remix-run/testDependency"": ""1.2.3"",
            ""@remix-run/duplicateRemixDependency"": ""1.2.3"",
          },
          ""engines"" : { ""npm"" : ""5.4.2"" }
        }";

        private const string PackageJsonWithFrameworksRemovesAngularAndVueJs = @"{
          ""name"": ""mynodeapp"",
          ""version"": ""1.0.0"",
          ""main"": ""server.js"",
          ""author"": ""Dev"",
          ""license"": ""ISC"",
          ""dependencies"": {
            ""jquery"": ""3.5.1"",
            ""@angular/testDependency"": ""1.2.3"",
            ""@angular/duplicateDependency"": ""1.2.3"",
            ""gatsby"": ""1.2.3"",
            ""vuepress"": ""4.5.6"",
            ""vue"": ""4.5.6"",
          },
          ""engines"" : { ""npm"" : ""5.4.2"" }
        }";

        private const string PackageJsonWithFrameworksRemovesReactAndVueJs = @"{
          ""name"": ""mynodeapp"",
          ""version"": ""1.0.0"",
          ""main"": ""server.js"",
          ""author"": ""Dev"",
          ""license"": ""ISC"",
          ""dependencies"": {
            ""jquery"": ""3.5.1"",
            ""react"": ""16.12.0"",
            ""gatsby"": ""1.2.3"",
            ""vuepress"": ""4.5.6"",
            ""vue"": ""4.5.6"",
          },
          ""engines"" : { ""npm"" : ""5.4.2"" }
        }";

        private const string SimpleServerJs = @"
            var http = require(""http"")
            http.createServer(function(req, res) {
                res.writeHead(200, { ""Content-Type"": ""text/plain""});
                res.write(""Test!"");
                res.end();
            }).listen(8888);";

        private const string LernaJsonWithNpmClient = @"{
          ""name"": ""mynodeapp"",
          ""version"": ""1.0.0"",
          ""npmClient"": ""yarn""
        }";

        private const string SampleValidYAMLYarnLockfile = "name: mynodeapp";

        private const string SampleInvalidYAMLYarnLockfile = @"{
         abcdefg 1234 - :: []
            -- 5
        }";

        [Fact]
        public void Detect_ReturnsNull_IfSourceDirectory_DoesNotHaveAnyFiles()
        {
            // Arrange
            var detector = CreateNodePlatformDetector();
            // No files in source directory
            var repo = new MemorySourceRepo();
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsNullVersion_ForSourceRepoOnlyWithServerJs()
        {
            // Arrange
            var detector = CreateNodePlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(SimpleServerJs, "server.js");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
        }

        [Fact]
        public void Detect_Returns_ForSourceRepoHasYarnrcYmlFile_InRootDirectory()
        {
            // Arrange
            var detector = CreateNodePlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(SampleValidYAMLYarnLockfile, NodeConstants.YarnLockFileName);
            repo.AddFile("", ".yarnrc.yml");
            var context = CreateContext(repo);

            // Act
            var result = (NodePlatformDetectorResult)detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
            Assert.True(result.HasYarnrcYmlFile);
        }

        [Fact]
        public void Detect_Returns_ForSourceRepoHasValidyamlYarnlockFile_InRootDirectory()
        {
            // Arrange
            var detector = CreateNodePlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(SampleValidYAMLYarnLockfile, NodeConstants.YarnLockFileName);
            var context = CreateContext(repo);

            // Act
            var result = (NodePlatformDetectorResult)detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
            Assert.True(result.IsYarnLockFileValidYamlFormat);
        }

        [Fact]
        public void Detect_ReturnsFalse_ForSourceRepoHasInvalidyamlYarnlockFile()
        {
            // Arrange
            var detector = CreateNodePlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(SampleInvalidYAMLYarnLockfile, NodeConstants.YarnLockFileName);
            var context = CreateContext(repo);

            // Act
            var result = (NodePlatformDetectorResult)detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
            Assert.False(result.IsYarnLockFileValidYamlFormat);
        }

        [Fact]
        public void Detect_ReturnsNull_ForSourceRepoWithServerJs_NotInRootDirectory()
        {
            // Arrange
            var detector = CreateNodePlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(SimpleServerJs, "subDir1", "server.js");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsNullVersion_ForSourceRepoOnlyWithAppJs()
        {
            // Arrange
            var detector = CreateNodePlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile("app.js content", "app.js");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
        }

        [Fact]
        public void Detect_ReturnsNull_ForSourceRepoWithAppJs_NotInRootDirectory()
        {
            // Arrange
            var detector = CreateNodePlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(SimpleServerJs, "subDir1", "app.js");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsNullVersion_ForPackageJsonWithNoVersion()
        {
            // Arrange
            var detector = CreateNodePlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithNoVersions, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
        }

        [Fact]
        public void Detect_ReturnsNull_ForSourceRepoWithPackageJson_NotInRootDirectory()
        {
            // Arrange
            var detector = CreateNodePlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithNodeVersion, "subDir1", NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsNullVersion_ForPackageJsonWithOnlyNpmVersion()
        {
            // Node detector only looks for node version and not the NPM version. The individual script
            // generator looks for npm version.

            // Arrange
            var detector = CreateNodePlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithOnlyNpmVersion, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
        }

        [Fact]
        public void Detect_ReturnsVersionFromPackageJson_IfEnvironmentVariableValueIsNotPresent()
        {
            // Arrange
            var detector = CreateNodePlatformDetector();
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
            Assert.Equal(string.Empty, result.AppDirectory);
        }

        [Fact]
        public void Detect_ReturnsNullVersion_ForMalformedPackageJson()
        {
            // Arrange
            var detector = CreateNodePlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(MalformedPackageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
        }

        [Fact]
        public void Detect_ReturnsNullVersion_ForPackageJsonWithNoExplicitVersionsSpecified()
        {
            // Arrange
            var detector = CreateNodePlatformDetector();
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithNoVersions, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
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
            var sourceRepo = new MemorySourceRepo();
            sourceRepo.AddFile("", iisStartupFileName);
            var detector = CreateNodePlatformDetector();
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
            var sourceRepo = new MemorySourceRepo();
            sourceRepo.AddFile("", "server.js");
            sourceRepo.AddFile("", iisStartupFileName);
            var detector = CreateNodePlatformDetector();
            var context = CreateContext(sourceRepo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Detect_ReturnsFrameworkInfos_IfKeywordExistsInPackageJsonFile()
        {
            // Arrange
            var sourceRepo = new MemorySourceRepo();
            sourceRepo.AddFile(PackageJsonWithFrameworks, NodeConstants.PackageJsonFileName);
            sourceRepo.AddFile("", NodeConstants.FlutterYamlFileName);
            var context = CreateContext(sourceRepo);
            var options = new DetectorOptions
            {
                DisableFrameworkDetection = false,
            };
            var detector = CreateNodePlatformDetector(options);
            List<string> expectedFrameworkNames = new List<string>()
            {
                "Aurelia", "Svelte", "jQuery", "React", "Remix", "Flutter"
            };
            List<string> expectedFrameworkVersions = new List<string>()
            {
                "1.3.1", "3.0.0", "3.5.1", "16.12.0", "1.2.3", ""
            };

            // Act
            var result = (NodePlatformDetectorResult)detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
            Assert.Equal(expectedFrameworkNames, result.Frameworks.Select(x => x.Framework).ToList());
            Assert.Equal(expectedFrameworkVersions, result.Frameworks.Select(x => x.FrameworkVersion).ToList());
        }

        [Theory]
        [InlineData(PackageJsonWithFrameworksRemovesAngularAndVueJs)]
        [InlineData(PackageJsonWithFrameworksRemovesReactAndVueJs)]
        public void Detect_ReturnsFrameworkInfos_RemovesAngularAndReact(string packageJson)
        {
            // Arrange
            var sourceRepo = new MemorySourceRepo();
            sourceRepo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(sourceRepo);
            var options = new DetectorOptions
            {
                DisableFrameworkDetection = false,
            };
            var detector = CreateNodePlatformDetector(options);
            List<string> expectedFrameworkNames = new List<string>()
            {
                "jQuery", "Gatsby", "VuePress"
            };
            List<string> expectedFrameworkVersions = new List<string>()
            {
                "3.5.1", "1.2.3", "4.5.6"
            };
            // Act
            var result = (NodePlatformDetectorResult)detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("nodejs", result.Platform);
            Assert.Null(result.PlatformVersion);
            Assert.Equal(string.Empty, result.AppDirectory);
            Assert.Equal(expectedFrameworkNames, result.Frameworks.Select(x => x.Framework).ToList());
            Assert.Equal(expectedFrameworkVersions, result.Frameworks.Select(x => x.FrameworkVersion).ToList());
        }

        [Fact]
        public void Detect_ReturnsLernaNpmClientName_IfLernaJsonFileExists()
        {
            // Arrange
            var sourceRepo = new MemorySourceRepo();
            sourceRepo.AddFile("", NodeConstants.PackageJsonFileName);
            sourceRepo.AddFile("", NodeConstants.YarnLockFileName);
            sourceRepo.AddFile(LernaJsonWithNpmClient, NodeConstants.LernaJsonFileName);
            var detector = CreateNodePlatformDetector();
            var context = CreateContext(sourceRepo);

            // Act
            var result = (NodePlatformDetectorResult)detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.HasLernaJsonFile);
            Assert.Equal(NodeConstants.YarnToolName, result.LernaNpmClient);
        }

        private DetectorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new DetectorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private NodeDetector CreateNodePlatformDetector(DetectorOptions options = null)
        {
            options = options ?? new DetectorOptions();
            return new NodeDetector(
                NullLogger<NodeDetector>.Instance, Options.Create(options));
        }

    }
}
