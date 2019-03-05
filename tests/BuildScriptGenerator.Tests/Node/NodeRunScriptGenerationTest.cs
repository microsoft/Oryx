// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Node
{
    public class NodeRunScriptGenerationTest
    {
        private const string PackageJsonWithStartScript = @"{
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

        private const string PackageJsonWithMainScript = @"{
          ""name"": ""mynodeapp"",
          ""version"": ""1.0.0"",
          ""description"": ""test app"",
          ""main"": ""server.js"",
          ""scripts"": {
            ""test"": ""echo \""Error: no test specified\"" && exit 1"",
          },
          ""author"": ""Dev"",
          ""license"": ""ISC"",
          ""engines"" : { ""node"" : ""6.11.0"" }
        }";

        private const string PackageJsonWithoutScript = @"{
          ""name"": ""mynodeapp"",
          ""version"": ""1.0.0"",
          ""description"": ""test app"",
          ""scripts"": {
            ""test"": ""echo \""Error: no test specified\"" && exit 1"",
          },
          ""author"": ""Dev"",
          ""license"": ""ISC"",
          ""engines"" : { ""node"" : ""6.11.0"" }
        }";

        [Fact]
        public void RunUserScriptIfProvided()
        {
            // Arrange
            var options = new RunScriptGeneratorOptions();
            options.SourceRepo = new MemorySourceRepo();
            options.UserStartupCommand = "abc.sh";
            var platform = GetPlatform();

            // Act
            var script = platform.GenerateBashRunScript(options);

            // Assert
            Assert.NotNull(script);
            Assert.Contains("abc.sh", script);
        }

        [Fact]
        public void NpmStartIfInPackageJson()
        {
            // Arrange
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithStartScript, NodeConstants.PackageJsonFileName);
            var options = new RunScriptGeneratorOptions();
            options.SourceRepo = repo;
            var platform = GetPlatform();

            // Act
            var script = platform.GenerateBashRunScript(options);

            // Assert
            Assert.NotNull(script);
            Assert.Contains("npm start", script);
        }

        [Fact]
        public void YarnStartIfInPackageJson_AndYarnLockFile()
        {
            // Arrange
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithStartScript, NodeConstants.PackageJsonFileName);
            repo.AddFile(string.Empty, NodeConstants.YarnLockFileName);
            var options = new RunScriptGeneratorOptions();
            options.SourceRepo = repo;
            var platform = GetPlatform();

            // Act
            var script = platform.GenerateBashRunScript(options);

            // Assert
            Assert.NotNull(script);
            Assert.Contains("yarn run start", script);
        }

        [Fact]
        public void NodeStartIfMainJsFile()
        {
            // Arrange
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithMainScript, NodeConstants.PackageJsonFileName);
            var options = new RunScriptGeneratorOptions();
            options.SourceRepo = repo;
            var platform = GetPlatform();

            // Act
            var script = platform.GenerateBashRunScript(options);

            // Assert
            Assert.NotNull(script);
            Assert.Contains("node server.js", script);
        }

        [Fact]
        public void NodeStartIfMainJsFile_customServer()
        {
            // Arrange
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithMainScript, NodeConstants.PackageJsonFileName);
            var options = new RunScriptGeneratorOptions();
            options.SourceRepo = repo;
            options.CustomServerCommand = "pm2 --test";
            var platform = GetPlatform();

            // Act
            var script = platform.GenerateBashRunScript(options);

            // Assert
            Assert.NotNull(script);
            Assert.Contains("pm2 --test server.js", script);
        }

        [Fact]
        public void NodeStartIfMainJsFile_debugBrk()
        {
            // Arrange
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithMainScript, NodeConstants.PackageJsonFileName);
            var options = new RunScriptGeneratorOptions();
            options.SourceRepo = repo;
            options.DebuggingMode = DebuggingMode.Break;
            var platform = GetPlatform();

            // Act
            var script = platform.GenerateBashRunScript(options);

            // Assert
            Assert.NotNull(script);
            Assert.Contains("node --inspect-brk server.js", script);
        }

        [Fact]
        public void NodeStartIfMainJsFile_debug()
        {
            // Arrange
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithMainScript, NodeConstants.PackageJsonFileName);
            var options = new RunScriptGeneratorOptions();
            options.SourceRepo = repo;
            options.DebuggingMode = DebuggingMode.Standard;
            var platform = GetPlatform();

            // Act
            var script = platform.GenerateBashRunScript(options);

            // Assert
            Assert.NotNull(script);
            Assert.Contains("node --inspect server.js", script);
        }

        [Fact]
        public void NodeAddsBenv_IfPlatformVersionSupplied()
        {
            // Arrange
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithMainScript, NodeConstants.PackageJsonFileName);
            var options = new RunScriptGeneratorOptions();
            options.SourceRepo = repo;
            options.DebuggingMode = DebuggingMode.Standard;
            var platform = GetPlatform();

            // Act
            var script = platform.GenerateBashRunScript(options);

            // Assert
            Assert.NotNull(script);
            Assert.DoesNotContain("source /usr/local/bin/benv", script);
        }

        [Fact]
        public void NodeDoesNotAddsBenv_IfNoPlatformVersionSupplied()
        {
            // Arrange
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithMainScript, NodeConstants.PackageJsonFileName);
            var options = new RunScriptGeneratorOptions();
            options.SourceRepo = repo;
            options.DebuggingMode = DebuggingMode.Standard;
            options.PlatformVersion = "10.15";
            var platform = GetPlatform();

            // Act
            var script = platform.GenerateBashRunScript(options);

            // Assert
            Assert.NotNull(script);
            Assert.Contains("source /usr/local/bin/benv node=10.15", script);
        }

        [Theory]
        [InlineData("bin/www")]
        [InlineData("server.js")]
        [InlineData("app.js")]
        [InlineData("index.js")]
        [InlineData("hostingstart.js")]
        public void NodeStartFromListOfCandidateFiles(string filename)
        {
            // Arrange
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithoutScript, NodeConstants.PackageJsonFileName);
            repo.AddFile("content", filename);
            var options = new RunScriptGeneratorOptions();
            options.SourceRepo = repo;
            var platform = GetPlatform();

            // Act
            var script = platform.GenerateBashRunScript(options);

            // Assert
            Assert.NotNull(script);
            Assert.Contains($"node {filename}", script);
        }

        [Fact]
        public void DefaultAppIfNoCommandDetected()
        {
            // Arrange
            var repo = new MemorySourceRepo();
            var options = new RunScriptGeneratorOptions();
            options.SourceRepo = repo;
            options.DefaultAppPath = "default.js";
            var platform = GetPlatform();

            // Act
            var script = platform.GenerateBashRunScript(options);

            // Assert
            Assert.NotNull(script);
            Assert.Contains("node default.js", script);
        }

        private NodePlatform GetPlatform()
        {
            var options = Options.Create(new NodeScriptGeneratorOptions());
            var platform = new NodePlatform(
                nodeScriptGeneratorOptions: options,
                nodeVersionProvider: null,
                logger: NullLogger<NodePlatform>.Instance,
                detector: null);
            return platform;
        }
    }
}