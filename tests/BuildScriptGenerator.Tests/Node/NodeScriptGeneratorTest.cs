// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Oryx.Tests.Infrastructure;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    /// <summary>
    /// Component tests for NodeJs support.
    /// </summary>
    public class NodeScriptGeneratorTest
    {
        private const string PackageJsonWithNoNpmVersion = @"{
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
          ""engines"" : { ""npm"" : ""5.4.2"" },
          ""dependencies"": { ""foo"" : ""1.0.0 - 2.9999.9999"", ""bar"" : "">=1.0.2 <2.1.2"" }
        }";

        private const string PackageJsonWithUnsupportedNpmVersion = @"{
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
          ""engines"" : { ""node"" : ""100.100.100"" }
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

        [Fact]
        public void TryGenerateBashScript_ReturnsFalse_WhenPackageJsonHas_UnsupportedNpmVersion()
        {
            // Arrange
            var scriptGenerator = GetScriptGenerator(defaultNpmVersion: "5.4.2");
            var repo = new CachedSourceRepo();
            repo.AddFile(PackageJsonWithUnsupportedNpmVersion, "package.json");
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";

            // Act
            var canGenerateScript = scriptGenerator.TryGenerateBashScript(context, out var generatedScriptContent);
        }

        [Fact]
        public void GeneratedScript_HasNpmVersion_SpecifiedInPackageJson()
        {
            // Arrange
            var scriptGenerator = GetScriptGenerator(defaultNpmVersion: "6.0.0");
            var repo = new CachedSourceRepo();
            repo.AddFile(PackageJsonWithNpmVersion, "package.json");
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";

            // Act
            var canGenerateScript = scriptGenerator.TryGenerateBashScript(context, out var generatedScriptContent);

            // Assert
            Assert.True(canGenerateScript);
            Assert.Contains("npm install", generatedScriptContent);
            Assert.Contains("benv node=8.2.1 npm=5.4.2", generatedScriptContent);
        }

        [Fact]
        public void GeneratedScript_HasDefaultNpmVersion_IfPackageJsonDoesNotHaveOne()
        {
            // Arrange
            var scriptGenerator = GetScriptGenerator(defaultNpmVersion: "6.0.0");
            var repo = new CachedSourceRepo();
            repo.AddFile(PackageJsonWithNoNpmVersion, "package.json");
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";

            // Act
            var canGenerateScript = scriptGenerator.TryGenerateBashScript(context, out var generatedScriptContent);

            // Assert
            Assert.True(canGenerateScript);
            Assert.Contains("npm install", generatedScriptContent);
            Assert.Contains("benv node=8.2.1 npm=6.0.0", generatedScriptContent);
        }

        [Fact]
        public void GeneratesScript_WithDefaultNpmVersion_ForMalformedPackageJson()
        {
            // Arrange
            var scriptGenerator = GetScriptGenerator(defaultNpmVersion: "5.4.2");
            var repo = new CachedSourceRepo();
            repo.AddFile(MalformedPackageJson, "package.json");
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";

            // Act
            var canGenerateScript = scriptGenerator.TryGenerateBashScript(context, out var generatedScriptContent);

            // Assert
            Assert.True(canGenerateScript);
            Assert.Contains("npm install", generatedScriptContent);
            Assert.Contains("benv node=8.2.1 npm=5.4.2", generatedScriptContent);
        }

        [Fact]
        public void GeneratedScript_UsesYarnInstall_IfYarnLockFileIsPresent()
        {
            // Arrange
            var scriptGenerator = GetScriptGenerator(defaultNpmVersion: "6.0.0");
            var repo = new CachedSourceRepo();
            repo.AddFile(PackageJsonWithNoNpmVersion, "package.json");
            repo.AddFile("Yarn lock file content here", "yarn.lock");
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";

            // Act
            var canGenerateScript = scriptGenerator.TryGenerateBashScript(context, out var generatedScriptContent);

            // Assert
            Assert.True(canGenerateScript);
            Assert.Contains("yarn install", generatedScriptContent);
            Assert.DoesNotContain("npm install", generatedScriptContent);
            Assert.Contains("benv node=8.2.1", generatedScriptContent);
        }

        [Fact]
        public void GeneratedScript_UsesNpmInstall_IfPackageLockJsonFileIsPresent()
        {
            // Arrange
            var scriptGenerator = GetScriptGenerator(defaultNpmVersion: "6.0.0");
            var repo = new CachedSourceRepo();
            repo.AddFile(PackageJsonWithNoNpmVersion, "package.json");
            repo.AddFile("Package lock json file content here", "package-lock.json");
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";

            // Act
            var canGenerateScript = scriptGenerator.TryGenerateBashScript(context, out var generatedScriptContent);

            // Assert
            Assert.True(canGenerateScript);
            Assert.Contains("npm install", generatedScriptContent);
            Assert.DoesNotContain("yarn install", generatedScriptContent);
            Assert.Contains("benv node=8.2.1 npm=6.0.0", generatedScriptContent);
        }

        private ILanguageScriptGenerator GetScriptGenerator(string defaultNodeVersion = null, string defaultNpmVersion = null)
        {
            var environment = new TestEnvironment();
            environment.Variables[NodeScriptGeneratorOptionsSetup.NodeJsDefaultVersion] = defaultNodeVersion;
            environment.Variables[NodeScriptGeneratorOptionsSetup.NpmDefaultVersion] = defaultNpmVersion;

            var nodeVersionProvider = new TestNodeVersionProvider(
                supportedNodeVersions: new[] { "6.11.0", "8.2.1" },
                supportedNpmVersions: new[] { "5.4.2", "6.0.0" });

            var nodeScriptGeneratorOptions = Options.Create(new NodeScriptGeneratorOptions());
            var optionsSetup = new NodeScriptGeneratorOptionsSetup(environment);
            optionsSetup.Configure(nodeScriptGeneratorOptions.Value);

            return new NodeScriptGenerator(nodeScriptGeneratorOptions, nodeVersionProvider, NullLogger<NodeScriptGenerator>.Instance);
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

            public void AddFile(string content, params string[] paths)
            {
                var filePath = Path.Combine(paths);
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

            public IEnumerable<string> EnumerateFiles(string searchPattern, bool searchSubDirectories)
            {
                throw new System.NotImplementedException();
            }

            public string[] ReadAllLines(params string[] paths)
            {
                throw new System.NotImplementedException();
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