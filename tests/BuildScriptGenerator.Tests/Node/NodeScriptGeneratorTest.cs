// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    /// <summary>
    /// Component tests for NodeJs support.
    /// </summary>
    public class NodeScriptGeneratorTest
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

        private const string PackageJsonWithNpmVersion = @"{
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

        private const string PakageJsonWithUnsupportedNpmVersion = @"{
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

        //TODO: add tests for node detection files

        [Fact]
        public void GeneratedScript_ForPackageJsonWithNoVersions_MustHaveNpmInstall()
        {
            // Arrange
            var scriptGenerator = GetScriptGenerator();
            var repo = GetSourceRepo(PackageJsonWithNoVersions);
            var context = CreateScriptGeneratorContext(repo);

            // Act-1
            var canGenerateScript = scriptGenerator.CanGenerateScript(context);

            // Assert-1
            Assert.True(canGenerateScript);

            // Act-2
            var generatedScriptContent = scriptGenerator.GenerateBashScript(context);

            // Assert-2
            // Simple check that at least "npm install" is there
            Assert.Contains("npm install", generatedScriptContent);
        }

        [Fact]
        public void GeneratedScript_ForPackageJsonWithNoVersions_MustUseDefaultNodeVersion()
        {
            // Arrange
            var scriptGenerator = GetScriptGenerator(defaultNodeVersion: "8.2.1");
            var repo = GetSourceRepo(PackageJsonWithNoVersions);
            var context = CreateScriptGeneratorContext(repo);

            // Act-1
            var canGenerateScript = scriptGenerator.CanGenerateScript(context);

            // Assert-1
            Assert.True(canGenerateScript);

            // Act-2
            var generatedScriptContent = scriptGenerator.GenerateBashScript(context);

            // Assert-2
            // Simple check that at least "npm install" is there
            Assert.Contains("npm install", generatedScriptContent);
            Assert.Contains($"benv node=8.2.1", generatedScriptContent);
        }

        [Fact]
        public void GeneratedScript_ForPackageJsonWithNoVersions_MustUseDefaultNpmVersion()
        {
            // Arrange
            var scriptGenerator = GetScriptGenerator(defaultNpmVersion: "5.4.2");
            var repo = GetSourceRepo(PackageJsonWithNoVersions);
            var context = CreateScriptGeneratorContext(repo);

            // Act-1
            var canGenerateScript = scriptGenerator.CanGenerateScript(context);

            // Assert-1
            Assert.True(canGenerateScript);

            // Act-2
            var generatedScriptContent = scriptGenerator.GenerateBashScript(context);

            // Assert-2
            // Simple check that at least "npm install" is there
            Assert.Contains("npm install", generatedScriptContent);
            Assert.Contains($"benv npm=5.4.2", generatedScriptContent);
        }

        [Fact]
        public void GeneratedScript_ForPackageJsonWithNoVersions_MustUseUserSuppliedLanguageVersion()
        {
            // Arrange
            var scriptGenerator = GetScriptGenerator();
            var repo = GetSourceRepo(PackageJsonWithNoVersions);
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";

            // Act-1
            var canGenerateScript = scriptGenerator.CanGenerateScript(context);

            // Assert-1
            Assert.True(canGenerateScript);

            // Act-2
            var generatedScriptContent = scriptGenerator.GenerateBashScript(context);

            // Assert-2
            // Simple check that at least "npm install" is there
            Assert.Contains("npm install", generatedScriptContent);
            Assert.Contains($"benv node=8.2.1", generatedScriptContent);
        }

        [Fact(Skip = "TODO: figure out expected behavior")]
        public void GeneratedScript_MustUseSuppliedNodeVersion_IgnoringNodeVersion_InPackageJson()
        {
        }

        [Fact]
        public void GeneratedScript_ForPackageJsonWithNoVersions_MustUseDefaultNpmAndNodeVersions()
        {
            // Arrange
            var scriptGenerator = GetScriptGenerator(defaultNpmVersion: "5.4.2", defaultNodeVersion: "8.2.1");
            var repo = GetSourceRepo(PackageJsonWithNoVersions);
            var context = CreateScriptGeneratorContext(repo);

            // Act-1
            var canGenerateScript = scriptGenerator.CanGenerateScript(context);

            // Assert-1
            Assert.True(canGenerateScript);

            // Act-2
            var generatedScriptContent = scriptGenerator.GenerateBashScript(context);

            // Assert-2
            // Simple check that at least "npm install" is there
            Assert.Contains("npm install", generatedScriptContent);
            Assert.Contains($"benv node=8.2.1 npm=5.4.2", generatedScriptContent);
        }

        [Fact]
        public void ShouldNotGenerateScript_ForUnsupportedNodeVersion()

        {
            // Arrange
            var scriptGenerator = GetScriptGenerator();
            var repo = GetSourceRepo(PakageJsonWithUnsupportedNodeVersion);
            var context = CreateScriptGeneratorContext(repo);

            // Act
            var exception = Assert.Throws<UnsupportedNodeVersionException>(
                () => scriptGenerator.GenerateBashScript(context));

            // Assert
            // Simple check that the message contains the unsupported version.
            Assert.Contains("20.20.20", exception.Message);
        }

        [Fact]
        public void ShouldNotGenerateScript_ForUnsupportedNpmVersion()
        {
            // Arrange
            var scriptGenerator = GetScriptGenerator();
            var repo = GetSourceRepo(PakageJsonWithUnsupportedNpmVersion);
            var context = CreateScriptGeneratorContext(repo);

            // Act
            var exception = Assert.Throws<UnsupportedNpmVersionException>(
                () => scriptGenerator.GenerateBashScript(context));

            // Assert
            // Simple check that the message contains the unsupported version.
            Assert.Contains("20.20.20", exception.Message);
        }

        [Fact]
        public void MalformedPackageJsonShouldHaveNpmInstall()
        {
            // Arrange
            var scriptGenerator = GetScriptGenerator();
            var repo = GetSourceRepo(MalformedPackageJson);
            var context = CreateScriptGeneratorContext(repo);

            // Act-1
            var canGenerateScript = scriptGenerator.CanGenerateScript(context);

            // Assert-1
            Assert.True(canGenerateScript);

            // Act-2
            var generatedScriptContent = scriptGenerator.GenerateBashScript(context);

            // Assert-2
            // Simple check that at least "npm install" is there
            Assert.Contains("npm install", generatedScriptContent);
        }

        [Fact]
        public void GeneratedScript_ForPackageJsonWithNodeVersion_MustUseThatVersion()
        {
            // Arrange
            var scriptGenerator = GetScriptGenerator();
            var repo = GetSourceRepo(PackageJsonWithNodeVersion);
            var context = CreateScriptGeneratorContext(repo);

            // Act-1
            var canGenerateScript = scriptGenerator.CanGenerateScript(context);

            // Assert-1
            Assert.True(canGenerateScript);

            // Act-2
            var generatedScriptContent = scriptGenerator.GenerateBashScript(context);

            // Assert-2
            // Simple check that at least "npm install" is there
            Assert.Contains("npm install", generatedScriptContent);
            Assert.Contains($"benv node=6.11.0", generatedScriptContent);
        }

        [Fact]
        public void GeneratedScript_ForPackageJsonWithNpmVersion_MustUseThatVersion()
        {
            // Arrange & Act
            // Arrange
            var scriptGenerator = GetScriptGenerator();
            var repo = GetSourceRepo(PackageJsonWithNpmVersion);
            var context = CreateScriptGeneratorContext(repo);

            // Act-1
            var canGenerateScript = scriptGenerator.CanGenerateScript(context);

            // Assert-1
            Assert.True(canGenerateScript);

            // Act-2
            var generatedScriptContent = scriptGenerator.GenerateBashScript(context);

            // Assert-2
            // Simple check that at least "npm install" is there
            Assert.Contains("npm install", generatedScriptContent);
            Assert.Contains($"benv npm=5.4.2", generatedScriptContent);
        }

        private IScriptGenerator GetScriptGenerator(string defaultNodeVersion = null, string defaultNpmVersion = null)
        {
            var environment = new TestEnvironemnt();
            environment.Variables[NodeScriptGeneratorOptionsSetup.NodeJsDefaultVersion] = defaultNodeVersion;
            environment.Variables[NodeScriptGeneratorOptionsSetup.NpmDefaultVersion] = defaultNpmVersion;

            var nodeVersionProvider = new TestNodeVersionProvider(
                supportedNodeVersions: new[] { "6.11.0", "8.2.1" },
                supportedNpmVersions: new[] { "5.4.2" });

            var nodeScriptGeneratorOptions = Options.Create(new NodeScriptGeneratorOptions());
            var optionsSetup = new NodeScriptGeneratorOptionsSetup(environment);
            optionsSetup.Configure(nodeScriptGeneratorOptions.Value);

            var scriptGenerator = new NodeScriptGenerator(
                nodeScriptGeneratorOptions,
                nodeVersionProvider,
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

        private static ScriptGeneratorContext CreateScriptGeneratorContext(
            ISourceRepo sourceRepo,
            string languageName = null,
            string languageVersion = null)
        {
            return new ScriptGeneratorContext
            {
                Language = languageName,
                LanguageVersion = languageVersion,
                SourceRepo = sourceRepo
            };
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

            public bool FileExists(params string[] paths)
            {
                var path = Path.Combine(paths);
                return pathToContent.ContainsKey(path);
            }

            public string ReadFile(params string[] paths)
            {
                var path = Path.Combine(paths);
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