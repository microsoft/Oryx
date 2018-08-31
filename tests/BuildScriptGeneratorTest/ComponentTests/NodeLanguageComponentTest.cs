// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

namespace BuildScriptGeneratorTest
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Oryx.BuildScriptGenerator;
    using Microsoft.Oryx.BuildScriptGenerator.SourceRepo;
    using Xunit;

    /// <summary>
    /// Component tests for NodeJs support.
    /// </summary>
    public class NodeLanguageComponentTest
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
            // Arrange & Act
            var buildScriptContent = BuildScriptFromPackageJson(SimplePackageJson);

            // Assert
            // Simple check that at least "npm install" is there
            Assert.Contains("npm install", buildScriptContent);
        }

        [Fact]
        public void MalformedPackageJsonShouldHaveNpmInstall()
        {
            // Arrange & Act
            var buildScriptContent = BuildScriptFromPackageJson(MalformedPackageJson);

            // Assert
            // Simple check that at least "npm install" is there
            Assert.Contains("npm install", buildScriptContent);
        }

        [Fact]
        public void SimplePackageJsonWithNodeVersionShouldHaveNpmInstall()
        {
            // Arrange & Act
            var buildScriptContent = BuildScriptFromPackageJson(SimplePackageJsonWithNodeVersion);

            // Assert
            // Simple check that at least "npm install" is there
            Assert.Contains("npm install", buildScriptContent);
            Assert.Contains($"/opt/nodejs/6.11.0", buildScriptContent);
        }

        [Fact]
        public void SimplePackageJsonWithNpmVersionShouldHaveNpmInstall()
        {
            // Arrange & Act
            var buildScriptContent = BuildScriptFromPackageJson(SimplePackageJsonWithNpmVersion);

            // Assert
            // Simple check that at least "npm install" is there
            Assert.Contains("npm install", buildScriptContent);
            Assert.Contains($"/opt/npm/5.4.2", buildScriptContent);
        }

        public string BuildScriptFromPackageJson(string packageJsonContent)
        {
            var repo = new CachedSourceRepo();
            repo.AddFile(packageJsonContent, "package.json");
            repo.AddFile(SimpleServerJs, "server.js");

            var detector = new LanguageDetector();
            var scriptBuilder = detector.GetBuildScriptBuilder(repo);
            // Check that node could be detected
            Assert.NotNull(scriptBuilder);

            var buildScriptContent = scriptBuilder.GenerateShScript();
            return buildScriptContent;
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
    }
}