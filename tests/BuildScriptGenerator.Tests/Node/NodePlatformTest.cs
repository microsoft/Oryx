// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Node
{
    public class NodePlatformTest
    {
        private const string PackageJson = @"{
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

        [Fact]
        public void BuildScript_HasSdkInstallScript_IfDynamicInstallIsEnabled_AndSdkIsNotAlreadyInstalled()
        {
            // Arrange
            var nodePlatform = CreateNodePlatform(dynamicInstallIsEnabled: true, sdkAlreadyInstalled: false);
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.NodeVersion = "10.10";

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.NotNull(buildScriptSnippet.PlatformInstallationScriptSnippet);
            Assert.Equal(
                TestNodePlatformInstaller.InstallerScript,
                buildScriptSnippet.PlatformInstallationScriptSnippet);
        }

        [Fact]
        public void BuildScript_HasNoSdkInstallScript_IfDynamicInstallIsEnabled_AndSdkIsAlreadyInstalled()
        {
            // Arrange
            var nodePlatform = CreateNodePlatform(dynamicInstallIsEnabled: true, sdkAlreadyInstalled: true);
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.NodeVersion = "10.10";

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Null(buildScriptSnippet.PlatformInstallationScriptSnippet);
        }

        [Fact]
        public void BuildScript_DoesNotHaveSdkInstallScript_IfDynamicInstallNotEnabled_AndSdkIsNotAlreadyInstalled()
        {
            // Arrange
            var nodePlatform = CreateNodePlatform(dynamicInstallIsEnabled: false, sdkAlreadyInstalled: false);
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.NodeVersion = "10.10";

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Null(buildScriptSnippet.PlatformInstallationScriptSnippet);
        }

        private TestNodePlatform CreateNodePlatform(
            bool dynamicInstallIsEnabled,
            bool sdkAlreadyInstalled)
        {
            var cliOptions = new BuildScriptGeneratorOptions();
            cliOptions.EnableDynamicInstall = dynamicInstallIsEnabled;
            var environment = new TestEnvironment();
            var installer = new TestNodePlatformInstaller(
                Options.Create(cliOptions),
                environment,
                sdkAlreadyInstalled);

            var versionProvider = new TestNodeVersionProvider();
            var nodeGeneratorOptions = new NodeScriptGeneratorOptions();
            var detector = new TestNodeLanguageDetector(
                versionProvider,
                Options.Create(nodeGeneratorOptions),
                NullLogger<NodeLanguageDetector>.Instance,
                environment,
                new TestStandardOutputWriter());

            return new TestNodePlatform(
                Options.Create(cliOptions),
                Options.Create(nodeGeneratorOptions),
                versionProvider,
                NullLogger<NodePlatform>.Instance,
                detector,
                environment,
                installer);
        }

        private BuildScriptGeneratorContext CreateContext(ISourceRepo sourceRepo)
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = sourceRepo,
            };
        }

        private class TestNodePlatform : NodePlatform
        {
            public TestNodePlatform(
                IOptions<BuildScriptGeneratorOptions> commonOptions,
                IOptions<NodeScriptGeneratorOptions> nodeScriptGeneratorOptions,
                INodeVersionProvider nodeVersionProvider,
                ILogger<NodePlatform> logger,
                NodeLanguageDetector detector,
                IEnvironment environment,
                NodePlatformInstaller nodePlatformInstaller)
                : base(
                      commonOptions,
                      nodeScriptGeneratorOptions,
                      nodeVersionProvider,
                      logger,
                      detector,
                      environment,
                      nodePlatformInstaller)
            {
            }
        }

        private class TestNodePlatformInstaller : NodePlatformInstaller
        {
            private readonly bool _sdkIsAlreadyInstalled;
            public static string InstallerScript = "installer-script-snippet";

            public TestNodePlatformInstaller(
                IOptions<BuildScriptGeneratorOptions> commonOptions,
                IEnvironment environment,
                bool sdkIsAlreadyInstalled)
                : base(commonOptions, environment)
            {
                _sdkIsAlreadyInstalled = sdkIsAlreadyInstalled;
            }

            public override bool IsVersionAlreadyInstalled(string version)
            {
                return _sdkIsAlreadyInstalled;
            }

            public override string GetInstallerScriptSnippet(string version)
            {
                return InstallerScript;
            }
        }

        private class TestNodeLanguageDetector : NodeLanguageDetector
        {
            public TestNodeLanguageDetector(
                INodeVersionProvider nodeVersionProvider,
                IOptions<NodeScriptGeneratorOptions> options,
                ILogger<NodeLanguageDetector> logger,
                IEnvironment environment,
                IStandardOutputWriter writer)
                : base(nodeVersionProvider, options, logger, environment, writer)
            {
            }
        }

        private class TestNodeVersionProvider : INodeVersionProvider
        {
            public PlatformVersionInfo GetVersionInfo()
            {
                throw new System.NotImplementedException();
            }
        }

        private class TestStandardOutputWriter : IStandardOutputWriter
        {
            public void Write(string message)
            {
            }

            public void WriteLine(string message)
            {
            }
        }
    }
}
