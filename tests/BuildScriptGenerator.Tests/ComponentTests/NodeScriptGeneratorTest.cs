// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

namespace BuildScriptGenerator.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;
    using Microsoft.Oryx.BuildScriptGenerator;
    using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
    using Microsoft.Oryx.BuildScriptGenerator.Node;
    using Microsoft.Oryx.BuildScriptGenerator.SourceRepo;
    using Xunit;

    /// <summary>
    /// Component tests for NodeJs support.
    /// </summary>
    public class NodeScriptGeneratorTest
    {
        private const string SimplePackageJson = @"{
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

        private const string SimplePackageJsonWithNodeVersion = @"{
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

        private const string SimplePackageJsonWithNpmVersion = @"{
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

        private const string UnsupportedNodeJSVersion = @"{
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

        private const string UnsupportedNpmVersion = @"{
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
          ""engines"" : { ""npm"" : ""20.20.20"" }
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
        public void SimplePackageJsonShouldHaveNpmInstall()
        {
            // Arrange
            var repo = GetSourceRepo(SimplePackageJson);
            var scriptGenerator = GetScriptGenerator(repo);

            // Act-1
            var canGenerateScript = scriptGenerator.CanGenerateShScript();

            // Assert-1
            Assert.True(canGenerateScript);

            // Act-2
            var generatedScriptContent = scriptGenerator.GenerateShScript();

            // Assert-2
            // Simple check that at least "npm install" is there
            Assert.Contains("npm install", generatedScriptContent);
        }

        [Fact]
        public void SimplePackageJsonShouldSetDefaultNodeVersionToBenv()
        {
            // Arrange
            var repo = GetSourceRepo(SimplePackageJson);
            var scriptGenerator = GetScriptGenerator(repo, defaultNodeVersion: "8.2.1");

            // Act-1
            var canGenerateScript = scriptGenerator.CanGenerateShScript();

            // Assert-1
            Assert.True(canGenerateScript);

            // Act-2
            var generatedScriptContent = scriptGenerator.GenerateShScript();

            // Assert-2
            // Simple check that at least "npm install" is there
            Assert.Contains("npm install", generatedScriptContent);
            Assert.Contains($"benv node=8.2.1", generatedScriptContent);
        }

        [Fact]
        public void SimplePackageJsonShouldSetDefaultNpmVersionToBenv()
        {
            // Arrange
            var repo = GetSourceRepo(SimplePackageJson);
            var scriptGenerator = GetScriptGenerator(repo, defaultNpmVersion: "5.4.2");

            // Act-1
            var canGenerateScript = scriptGenerator.CanGenerateShScript();

            // Assert-1
            Assert.True(canGenerateScript);

            // Act-2
            var generatedScriptContent = scriptGenerator.GenerateShScript();

            // Assert-2
            // Simple check that at least "npm install" is there
            Assert.Contains("npm install", generatedScriptContent);
            Assert.Contains($"benv npm=5.4.2", generatedScriptContent);
        }

        [Fact]
        public void ShouldNotGenerateScript_ForUnsupportedNodeVersion()

        {
            // Arrange
            var repo = GetSourceRepo(UnsupportedNodeJSVersion);
            var scriptGenerator = GetScriptGenerator(repo);

            // Act
            var exception = Assert.Throws<UnsupportedNodeVersionException>(() => scriptGenerator.GenerateShScript());

            // Assert
            // Simple check that the message contains the unsupported version.
            Assert.Contains("20.20.20", exception.Message);
        }

        [Fact]
        public void ShouldNotGenerateScript_ForUnsupportedNpmVersion()

        {
            // Arrange
            var repo = GetSourceRepo(UnsupportedNpmVersion);
            var scriptGenerator = GetScriptGenerator(repo);

            // Act
            var exception = Assert.Throws<UnsupportedNpmVersionException>(() => scriptGenerator.GenerateShScript());

            // Assert
            // Simple check that the message contains the unsupported version.
            Assert.Contains("20.20.20", exception.Message);
        }

        [Fact]
        public void MalformedPackageJsonShouldHaveNpmInstall()
        {
            // Arrange
            var repo = GetSourceRepo(MalformedPackageJson);
            var scriptGenerator = GetScriptGenerator(repo);

            // Act-1
            var canGenerateScript = scriptGenerator.CanGenerateShScript();

            // Assert-1
            Assert.True(canGenerateScript);

            // Act-2
            var generatedScriptContent = scriptGenerator.GenerateShScript();

            // Assert-2
            // Simple check that at least "npm install" is there
            Assert.Contains("npm install", generatedScriptContent);
        }

        [Fact]
        public void SimplePackageJsonWithNodeVersionShouldSetNodeVersionToBenv()
        {
            // Arrange
            var repo = GetSourceRepo(SimplePackageJsonWithNodeVersion);
            var scriptGenerator = GetScriptGenerator(repo);

            // Act-1
            var canGenerateScript = scriptGenerator.CanGenerateShScript();

            // Assert-1
            Assert.True(canGenerateScript);

            // Act-2
            var generatedScriptContent = scriptGenerator.GenerateShScript();

            // Assert-2
            // Simple check that at least "npm install" is there
            Assert.Contains("npm install", generatedScriptContent);
            Assert.Contains($"benv node=6.11.0", generatedScriptContent);
        }

        [Fact]
        public void SimplePackageJsonWithNpmVersionShouldSetNpmVersionToBenv()
        {
            // Arrange & Act
            // Arrange
            var repo = GetSourceRepo(SimplePackageJsonWithNpmVersion);
            var scriptGenerator = GetScriptGenerator(repo);

            // Act-1
            var canGenerateScript = scriptGenerator.CanGenerateShScript();

            // Assert-1
            Assert.True(canGenerateScript);

            // Act-2
            var generatedScriptContent = scriptGenerator.GenerateShScript();

            // Assert-2
            // Simple check that at least "npm install" is there
            Assert.Contains("npm install", generatedScriptContent);
            Assert.Contains($"benv npm=5.4.2", generatedScriptContent);
        }

        private IScriptGenerator GetScriptGenerator(
            ISourceRepo sourceRepo,
            string defaultNodeVersion = null,
            string defaultNpmVersion = null)
        {
            var environment = new TestEnvironemnt();
            environment.Variables[NodeScriptGeneratorOptionsSetup.NodeJsDefaultVersion] = defaultNodeVersion;
            environment.Variables[NodeScriptGeneratorOptionsSetup.NpmDefaultVersion] = defaultNpmVersion;

            var nodeVersionProvider = new TestNodeVersionProvider(
                supportedNodeVersions: new[] { "6.11.0", "8.2.1" },
                supportedNpmVersions: new[] { "5.4.2" });

            var nodeScriptGeneratorOptions = Options.Create(new NodeScriptGeneratorOptions());
            var optionsSetup = new NodeScriptGeneratorOptionsSetup(environment, nodeVersionProvider);
            optionsSetup.Configure(nodeScriptGeneratorOptions.Value);

            var scriptGenerator = new NodeScriptGenerator(
                sourceRepo,
                Options.Create(new BuildScriptGeneratorOptions()),
                nodeScriptGeneratorOptions,
                new NodeVersionResolver(nodeScriptGeneratorOptions),
                NullLogger<NodeScriptGenerator>.Instance);
            return scriptGenerator;
        }

        private ISourceRepo GetSourceRepo(string packageJsonContent)
        {
            var repo = new CachedSourceRepo();
            repo.AddFile(packageJsonContent, "package.json");
            repo.AddFile(SimpleServerJs, "server.js");
            return repo;
        }

        private class CachedSourceRepo : ISourceRepo
        {
            private Dictionary<string, string> pathToContent = new Dictionary<string, string>();

            public void AddFile(string content, params string[] path)
            {
                var filePath = Path.Combine(path);
                pathToContent[filePath] = content;
            }

            public string RootPath => string.Empty;

            public bool FileExists(params string[] pathToFile)
            {
                var path = Path.Combine(pathToFile);
                return pathToContent.ContainsKey(path);
            }

            public string ReadFile(string path)
            {
                return pathToContent[path];
            }
        }

        private class TestEnvironemnt : IEnvironment
        {
            public Dictionary<string, string> Variables { get; } = new Dictionary<string, string>();

            public string GetEnvironmentVariable(string name)
            {
                return Variables[name];
            }
        }

        private class TestNodeVersionProvider : INodeVersionProvider
        {
            public TestNodeVersionProvider(string[] supportedNodeVersions, string[] supportedNpmVersions)
            {
                SupportedNodeVersions = supportedNodeVersions;
                SupportedNpmVersions = supportedNpmVersions;
            }

            public IEnumerable<string> SupportedNodeVersions { get; }

            public IEnumerable<string> SupportedNpmVersions { get; }
        }
    }
}