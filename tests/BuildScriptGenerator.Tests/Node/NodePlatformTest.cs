// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Node;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Node
{
    public class NodePlatformTest
    {
        [Fact]
        public void GeneratedBuildSnippet_HasCustomNpmRunBuildCommand_EvenIfPackageJsonHasBuildNodes()
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
                ""build"": ""build-node"",
                ""build:azure"": ""azure-node"",
              },
            }";
            var expectedText = "custom-npm-run-build";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions { CustomRunBuildCommand = expectedText },
                new NodePlatformInstaller(
                    Options.Create(commonOptions),
                    NullLoggerFactory.Instance));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
            };

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains(expectedText, buildScriptSnippet.BashBuildScriptSnippet);
            Assert.DoesNotContain("npm run build", buildScriptSnippet.BashBuildScriptSnippet);
            Assert.DoesNotContain("npm run build:azure", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_HasNpmRunBuildCommand()
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
                ""build"": ""build-node"",
              },
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions { CustomRunBuildCommand = null },
                new NodePlatformInstaller(
                    Options.Create(commonOptions),
                    NullLoggerFactory.Instance));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
            };

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.DoesNotContain("npm run build:azure", buildScriptSnippet.BashBuildScriptSnippet);
            Assert.Contains("npm run build", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_HasNpmRunBuildAzureCommand()
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
                ""build:azure"": ""build-azure-node"",
              },
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions { CustomRunBuildCommand = null },
                new NodePlatformInstaller(
                    Options.Create(commonOptions),
                    NullLoggerFactory.Instance));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
            };

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains("npm run build:azure", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_HasLernaRunBuildCommand_IfLernaJsonFileExists()
        {
            // Arrange
            const string lernaJson = @"{
              ""version"": ""3.22.1"",
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions { CustomRunBuildCommand = null, EnableNodeMonorepoBuild = true },
                new NodePlatformInstaller(
                    Options.Create(commonOptions),
                    NullLoggerFactory.Instance));
            var repo = new MemorySourceRepo();
            repo.AddFile(string.Empty, NodeConstants.PackageJsonFileName);
            repo.AddFile(lernaJson, NodeConstants.LernaJsonFileName);
            var context = CreateContext(repo);
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
                HasLernaJsonFile = true,
                LernaNpmClient = "npm",
            };

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains("lerna bootstrap", buildScriptSnippet.BashBuildScriptSnippet);
            Assert.Contains("lerna run build", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_ThrowsException_IfBothLernaAndLageFileExists()
        {
            // Arrange
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions { CustomRunBuildCommand = null, EnableNodeMonorepoBuild = true },
                new NodePlatformInstaller(
                    Options.Create(commonOptions),
                    NullLoggerFactory.Instance));
            var repo = new MemorySourceRepo();
            repo.AddFile(string.Empty, NodeConstants.PackageJsonFileName);
            repo.AddFile(string.Empty, NodeConstants.LageConfigJSFileName);
            repo.AddFile(string.Empty, NodeConstants.LernaJsonFileName);
            var context = CreateContext(repo);
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
                HasLernaJsonFile = true,
                HasLageConfigJSFile = true,
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidUsageException>(
                () => nodePlatform.GenerateBashBuildScriptSnippet(context, detectorResult));

            Assert.Equal(
                "Multiple monorepo package management tools are found, please choose to use either Lerna or Lage.",
                exception.Message);
        }

        [Fact]
        public void GeneratedBuildSnippet_CustomBuildCommandWillExecute_WhenOtherCommandsAlsoExist()
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
              },
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions { CustomBuildCommand = "custom build command", CustomRunBuildCommand = "custom run command"},
                new NodePlatformInstaller(
                    Options.Create(commonOptions),
                    NullLoggerFactory.Instance));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
            };

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains("custom build command", buildScriptSnippet.BashBuildScriptSnippet);
            Assert.DoesNotContain("custom run command", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_CustomRunCommandWillExecute_WhenOtherCommandsAlsoExist()
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
              },
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions { CustomRunBuildCommand = "custom command here", EnableNodeMonorepoBuild = true },
                new NodePlatformInstaller(
                    Options.Create(commonOptions),
                    NullLoggerFactory.Instance));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            repo.AddFile(string.Empty, NodeConstants.LernaJsonFileName);
            var context = CreateContext(repo);
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
                HasLernaJsonFile = true,
                LernaNpmClient = "npm",
            };

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains("custom command here", buildScriptSnippet.BashBuildScriptSnippet);
            Assert.DoesNotContain("lerna run build", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_WillSetupYarnTimeOutConfig()
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
              },
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions { CustomRunBuildCommand = null, YarnTimeOutConfig = "60000" },
                new NodePlatformInstaller(
                    Options.Create(commonOptions),
                    NullLoggerFactory.Instance));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
            };

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains("yarn config set network-timeout 60000 -g", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_WillNotBuildMonorepo_IfNodeMonorepoOptionNotEnabled()
        {
            // Arrange
            const string lernaJson = @"{
              ""version"": ""3.22.1"",
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions { CustomRunBuildCommand = null },
                new NodePlatformInstaller(
                    Options.Create(commonOptions),
                    NullLoggerFactory.Instance));
            var repo = new MemorySourceRepo();
            repo.AddFile(string.Empty, NodeConstants.PackageJsonFileName);
            repo.AddFile(lernaJson, NodeConstants.LernaJsonFileName);
            var context = CreateContext(repo);
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
                HasLernaJsonFile = true,
                LernaNpmClient = "npm",
            };

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.DoesNotContain("lerna bootstrap", buildScriptSnippet.BashBuildScriptSnippet);
            Assert.DoesNotContain("lerna run build", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_UsingYarn2Commands()
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
              },
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions { CustomRunBuildCommand = null },
                new NodePlatformInstaller(
                    Options.Create(commonOptions),
                    NullLoggerFactory.Instance));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            repo.AddFile("", NodeConstants.YarnLockFileName);
            repo.AddFile("", ".yarnrc.yml");
            var context = CreateContext(repo);
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
                HasYarnrcYmlFile = true,
                IsYarnLockFileValidYamlFormat = true,
            };

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains("yarn workspaces focus --all", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_UsesYarn_WhenEnginesFieldIsSet()
        {
            // Arrange
            const string packageJson = @"{
              ""engines"": {
                ""yarn"": ""~1.22.11""
              }
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions { CustomRunBuildCommand = null },
                new NodePlatformInstaller(
                    Options.Create(commonOptions),
                    NullLoggerFactory.Instance));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
                HasYarnrcYmlFile = true,
                IsYarnLockFileValidYamlFormat = true,
            };

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains("yarn workspaces focus --all", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void BuildScript_HasSdkInstallScript_IfDynamicInstallIsEnabled_AndSdkIsNotAlreadyInstalled()
        {
            // Arrange
            var nodePlatform = CreateNodePlatform(dynamicInstallIsEnabled: true, sdkAlreadyInstalled: false);
            var repo = new MemorySourceRepo();
            repo.AddFile(string.Empty, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
            };
            // Act
            var buildScriptSnippet = nodePlatform.GetInstallerScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Equal(TestNodePlatformInstaller.InstallerScript, buildScriptSnippet);
        }

        [Fact]
        public void BuildScript_HasNoSdkInstallScript_IfDynamicInstallIsEnabled_AndSdkIsAlreadyInstalled()
        {
            // Arrange
            var nodePlatform = CreateNodePlatform(dynamicInstallIsEnabled: true, sdkAlreadyInstalled: true);
            var repo = new MemorySourceRepo();
            repo.AddFile(string.Empty, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
            };

            // Act
            var buildScriptSnippet = nodePlatform.GetInstallerScriptSnippet(context, detectorResult);

            // Assert
            Assert.Null(buildScriptSnippet);
        }

        [Fact]
        public void BuildScript_DoesNotHaveSdkInstallScript_IfDynamicInstallNotEnabled_AndSdkIsNotAlreadyInstalled()
        {
            // Arrange
            var nodePlatform = CreateNodePlatform(dynamicInstallIsEnabled: false, sdkAlreadyInstalled: false);
            var repo = new MemorySourceRepo();
            repo.AddFile(string.Empty, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
            };

            // Act
            var buildScriptSnippet = nodePlatform.GetInstallerScriptSnippet(context, detectorResult);

            // Assert
            Assert.Null(buildScriptSnippet);
        }

        [Theory]
        [InlineData("true")]
        [InlineData("")]
        public void GeneratedBuildSnippet_DoesNotThrowException_IfPackageJsonHasBuildNode_AndRequireBuildPropertyIsSet(
            string requireBuild)
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
                ""build"": ""build-node"",
              },
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions(),
                new NodePlatformInstaller(
                    Options.Create(commonOptions),
                    NullLoggerFactory.Instance));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.Properties[NodePlatform.RequireBuildPropertyKey] = requireBuild;
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
            };

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains("npm run build", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_DoesNotThrowException_IfPackageJsonHasBuildAzureNode_AndRequireBuildPropertyIsSet()
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
                ""build:azure"": ""azure-node"",
              },
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions(),
                new NodePlatformInstaller(
                    Options.Create(commonOptions),
                    NullLoggerFactory.Instance));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.Properties[NodePlatform.RequireBuildPropertyKey] = "true";
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
            };

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains("npm run build:azure", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_DoesNotThrowException_IfCustomRunCommandIsProvided_AndRequireBuildPropertyIsSet()
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
              },
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions { CustomRunBuildCommand = "custom command here" },
                new NodePlatformInstaller(
                    Options.Create(commonOptions),
                    NullLoggerFactory.Instance));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.Properties[NodePlatform.RequireBuildPropertyKey] = "true";
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
            };

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains("custom command here", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedBuildSnippet_ThrowsException_IfRequireBuildPropertyIsSet_AndNoBuildStepIsProvided()
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
              },
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions(),
                new NodePlatformInstaller(
                    Options.Create(commonOptions),
                    NullLoggerFactory.Instance));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.Properties[NodePlatform.RequireBuildPropertyKey] = "true";
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
            };

            // Act & Assert
            var exception = Assert.Throws<NoBuildStepException>(
                () => nodePlatform.GenerateBashBuildScriptSnippet(context, detectorResult));
            Assert.Equal(
                "Could not find either 'build' or 'build:azure' node under 'scripts' in package.json. " +
                "Could not find value for custom run build command using the environment variable " +
                "key 'RUN_BUILD_COMMAND'." +
                "Could not find tools for building monorepos, no 'lerna.json' or 'lage.config.js' files found.",
                exception.Message);
        }

        [Fact]
        public void GeneratedBuildSnippet_DoesNotThrowException_IfNoBuildStepIsProvided_AndRequireBuildPropertyIsSet_ToFalse()
        {
            // Arrange
            const string packageJson = @"{
              ""main"": ""server.js"",
              ""scripts"": {
              },
            }";
            var commonOptions = new BuildScriptGeneratorOptions();
            var nodePlatform = CreateNodePlatform(
                commonOptions,
                new NodeScriptGeneratorOptions { CustomRunBuildCommand = "custom command here" },
                new NodePlatformInstaller(
                    Options.Create(commonOptions),
                    NullLoggerFactory.Instance));
            var repo = new MemorySourceRepo();
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            context.Properties[NodePlatform.RequireBuildPropertyKey] = "false";
            var detectorResult = new NodePlatformDetectorResult
            {
                Platform = NodeConstants.PlatformName,
                PlatformVersion = "10.10",
            };

            // Act
            var buildScriptSnippet = nodePlatform.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(buildScriptSnippet);
            Assert.Contains("custom command here", buildScriptSnippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void Detect_ReturnsEnvDefaultVersion_IfNoVersionProvidedByDetector()
        {
            // Arrange
            var expectedVersion = "8.11.2";
            var nodeScriptGeneratorOptions = new NodeScriptGeneratorOptions();
            nodeScriptGeneratorOptions.DefaultVersion = expectedVersion;
            var platform = CreateNodePlatform(
                supportedNodeVersions: new[] { "6.11.0", expectedVersion, "10.14.0" },
                defaultVersion: null,
                detectedVersion: null,
                nodeScriptGeneratorOptions: nodeScriptGeneratorOptions);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NodeConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsEnvDefaultVersion_IfNoVersionProvidedByDetector_WithProviderDefaultVersion()
        {
            // Arrange
            var expectedVersion = "8.11.2";
            var nodeScriptGeneratorOptions = new NodeScriptGeneratorOptions();
            nodeScriptGeneratorOptions.DefaultVersion = expectedVersion;
            var platform = CreateNodePlatform(
                supportedNodeVersions: new[] { "6.11.0", expectedVersion, "10.14.0" },
                defaultVersion: "6.11.0",
                detectedVersion: null,
                nodeScriptGeneratorOptions: nodeScriptGeneratorOptions);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NodeConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsNodeVersion_IfDefaultVersionEnvVarProvided()
        {
            // Arrange
            var expectedVersion = "8.11.2";
            var notExpectedVersion = "6.11.0";
            var nodeScriptGeneratorOptions = new NodeScriptGeneratorOptions();
            nodeScriptGeneratorOptions.DefaultVersion = notExpectedVersion;
            nodeScriptGeneratorOptions.NodeVersion = expectedVersion;
            var platform = CreateNodePlatform(
                supportedNodeVersions: new[] { notExpectedVersion, expectedVersion, "10.14.0" },
                defaultVersion: notExpectedVersion,
                detectedVersion: notExpectedVersion,
                nodeScriptGeneratorOptions: nodeScriptGeneratorOptions);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NodeConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsDefaultVersionOfVersionProvider_IfNoVersionProvidedByDetector_OrOptions()
        {
            // Arrange
            var expectedVersion = "8.11.2";
            var platform = CreateNodePlatform(
                supportedNodeVersions: new[] { "6.11.0", expectedVersion, "10.14.0" },
                defaultVersion: expectedVersion,
                detectedVersion: null);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NodeConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Theory]
        [InlineData(new[] { "8.11.1", "8.11.13", "9.10.12" }, "8", "8.11.13")]
        [InlineData(new[] { "8.11.1", "8.11.13", "9.10.12" }, "8.11", "8.11.13")]
        [InlineData(new[] { "8.11.1", "8.11.13", "9.10.12" }, "8.11.1", "8.11.1")]
        public void Detect_ReturnsMaximumSatisfyingVersion_WhenDefaultVersionHasOnlyPartialVersionParts(
            string[] supportedVersions,
            string defaultVersion,
            string expectedVersion)
        {
            // Arrange
            var platform = CreateNodePlatform(
                supportedNodeVersions: supportedVersions,
                defaultVersion: defaultVersion,
                detectedVersion: null);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NodeConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Theory]
        [InlineData(new[] { "8.11.1", "8.11.13", "9.10.12" }, "8", "8.11.13")]
        [InlineData(new[] { "8.11.1", "8.11.13", "9.10.12" }, "8.11", "8.11.13")]
        [InlineData(new[] { "8.11.1", "8.11.13", "9.10.12" }, "8.11.1", "8.11.1")]
        public void Detect_ReturnsMaximumSatisfyingVersion_WhenDefaultVersionEnvVarHasOnlyPartialVersionParts(
            string[] supportedVersions,
            string defaultVersion,
            string expectedVersion)
        {
            // Arrange
            var nodeScriptGeneratorOptions = new NodeScriptGeneratorOptions();
            nodeScriptGeneratorOptions.DefaultVersion = defaultVersion;
            var platform = CreateNodePlatform(
                supportedNodeVersions: supportedVersions,
                defaultVersion: null,
                detectedVersion: null,
                nodeScriptGeneratorOptions: nodeScriptGeneratorOptions);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NodeConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsResult_WithDefaultVersion_WhenNoVersionReturnedFromDetector()
        {
            // Arrange
            var expectedVersion = "1.2.3";
            var platform = CreateNodePlatform(
                supportedNodeVersions: new[] { expectedVersion },
                defaultVersion: expectedVersion,
                detectedVersion: null);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NodeConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedNodeVersion_IsDetected()
        {
            // Arrange
            var detectedVersion = "20.20.20";
            var supportedVersion = "6.11.0";
            var platform = CreateNodePlatform(
                supportedNodeVersions: new[] { supportedVersion },
                defaultVersion: supportedVersion,
                detectedVersion: detectedVersion);
            var context = CreateContext();

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => platform.Detect(context));
            Assert.Equal(
                $"Platform '{NodeConstants.PlatformName}' version '{detectedVersion}' is unsupported. " +
                $"Supported versions: {supportedVersion}",
                exception.Message);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedVersion_IsSetInOptions()
        {
            // Arrange
            var unsupportedVersion = "20.20.20";
            var supportedVersion = "6.11.0";
            var nodeScriptGeneratorOptions = new NodeScriptGeneratorOptions();
            nodeScriptGeneratorOptions.NodeVersion = unsupportedVersion;
            var platform = CreateNodePlatform(
                supportedNodeVersions: new[] { supportedVersion },
                defaultVersion: supportedVersion,
                detectedVersion: supportedVersion,
                nodeScriptGeneratorOptions: nodeScriptGeneratorOptions);
            var context = CreateContext();

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => platform.Detect(context));
            Assert.Equal(
                $"Platform '{NodeConstants.PlatformName}' version '{unsupportedVersion}' is unsupported. " +
                $"Supported versions: {supportedVersion}",
                exception.Message);
        }

        [Fact]
        public void Detect_ReturnsVersionFromOptions_EvenIfDetectorReturnsAVersion()
        {
            // Arrange
            var detectedVersion = "6.11.0";
            var expectedVersionToBeUsed = "10.14.0";
            var defaultVersion = "8.11.2";
            var nodeScriptGeneratorOptions = new NodeScriptGeneratorOptions();
            nodeScriptGeneratorOptions.NodeVersion = expectedVersionToBeUsed;
            var platform = CreateNodePlatform(
                supportedNodeVersions: new[] { detectedVersion, defaultVersion, expectedVersionToBeUsed },
                defaultVersion: defaultVersion,
                detectedVersion: detectedVersion,
                nodeScriptGeneratorOptions: nodeScriptGeneratorOptions);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NodeConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersionToBeUsed, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsDefaultVersion_ForSourceRepoOnlyWithServerJs_AndNoPackageJson()
        {
            // Arrange
            var expectedVersion = "1.2.3";
            var platform = CreateNodePlatform(
                supportedNodeVersions: new[] { expectedVersion },
                defaultVersion: expectedVersion);
            var repo = new MemorySourceRepo();
            repo.AddFile(SamplePackageJsonContents.SimpleServerJs, "server.js");
            var context = CreateContext(repo);

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NodeConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
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
            var platform = CreateNodePlatform(
                supportedNodeVersions: supportedVersions,
                defaultVersion: defaultVersion,
                detectedVersion: versionRangeInPackageJson);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NodeConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Theory]
        [InlineData(new[] { "8.11.2", "5.6.9", "8.11.13" }, "5.6.9", ">5", "8.11.13")]
        [InlineData(new[] { "8.9.5", "8.11.2", "5.6.9", "8.11.13" }, "5.6.9", ">8.9", "8.11.13")]
        [InlineData(new[] { "8.9.5", "8.11.2", "11.12.0", "13.12.12", "8.11.13" }, "5.6.9", ">8.9 <13", "11.12.0")]
        public void Detect_ReturnsVersionFromOptions_WithVersionSatisfying_NodeVersionRangeInPackageJson(
            string[] supportedVersions,
            string defaultVersion,
            string versionRangeInOptions,
            string expectedVersion)
        {
            // Arrange
            var nodeScriptGeneratorOptions = new NodeScriptGeneratorOptions();
            nodeScriptGeneratorOptions.NodeVersion = versionRangeInOptions;
            var platform = CreateNodePlatform(
                supportedNodeVersions: supportedVersions,
                defaultVersion: defaultVersion,
                detectedVersion: "1.1.1",
                nodeScriptGeneratorOptions: nodeScriptGeneratorOptions);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NodeConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);

        }
        [Fact]
        public void Detect_ReturnsResult_WithDefaultVersion_ForSourceRepoOnlyWithAppJs_AndNoPackageJson()
        {
            // Arrange
            var expectedVersion = "1.2.3";
            var platform = CreateNodePlatform(
                supportedNodeVersions: new[] { expectedVersion },
                defaultVersion: expectedVersion,
                detectedVersion: null);
            var context = CreateContext();

            // Act
            var result = platform.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NodeConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        private TestNodePlatform CreateNodePlatform(
            string[] supportedNodeVersions = null,
            string defaultVersion = null,
            string detectedVersion = null,
            BuildScriptGeneratorOptions commonOptions = null,
            NodeScriptGeneratorOptions nodeScriptGeneratorOptions = null)
        {
            nodeScriptGeneratorOptions = nodeScriptGeneratorOptions ?? new NodeScriptGeneratorOptions();
            commonOptions = commonOptions ?? new BuildScriptGeneratorOptions();
            var versionProvider = new TestNodeVersionProvider(supportedNodeVersions, defaultVersion);
            var externalSdkProvider = new ExternalSdkProvider(NullLogger<ExternalSdkProvider>.Instance);
            var environment = new TestEnvironment();
            var detector = new TestNodePlatformDetector(detectedVersion: detectedVersion);
            var platformInstaller = new NodePlatformInstaller(
                Options.Create(commonOptions),
                NullLoggerFactory.Instance);
            return new TestNodePlatform(
                Options.Create(commonOptions),
                Options.Create(nodeScriptGeneratorOptions),
                versionProvider,
                NullLogger<NodePlatform>.Instance,
                detector,
                environment,
                platformInstaller,
                externalSdkProvider,
                TelemetryClientHelper.GetTelemetryClient());
        }

        private TestNodePlatform CreateNodePlatform(
            BuildScriptGeneratorOptions commonOptions,
            NodeScriptGeneratorOptions nodeScriptGeneratorOptions,
            NodePlatformInstaller platformInstaller,
            string detectedVersion = null)
        {
            var environment = new TestEnvironment();

            var versionProvider = new TestNodeVersionProvider();
            var externalSdkProvider = new ExternalSdkProvider(NullLogger<ExternalSdkProvider>.Instance);
            var detector = new TestNodePlatformDetector(detectedVersion: detectedVersion);
        
            return new TestNodePlatform(
                Options.Create(commonOptions),
                Options.Create(nodeScriptGeneratorOptions),
                versionProvider,
                NullLogger<NodePlatform>.Instance,
                detector,
                environment,
                platformInstaller,
                externalSdkProvider, 
                TelemetryClientHelper.GetTelemetryClient());  
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
                sdkAlreadyInstalled,
                NullLoggerFactory.Instance);
            var versionProvider = new TestNodeVersionProvider();
            var externalSdkProvider = new ExternalSdkProvider(NullLogger<ExternalSdkProvider>.Instance);
            var nodeScriptGeneratorOptions = new NodeScriptGeneratorOptions();
            var detector = new TestNodePlatformDetector();
            return new TestNodePlatform(
                Options.Create(cliOptions),
                Options.Create(nodeScriptGeneratorOptions),
                versionProvider,
                NullLogger<NodePlatform>.Instance,
                detector,
                environment,
                installer,
                externalSdkProvider,
                TelemetryClientHelper.GetTelemetryClient());
        }

        private BuildScriptGeneratorContext CreateContext(ISourceRepo sourceRepo = null)
        {
            sourceRepo = sourceRepo ?? new MemorySourceRepo();
            return new BuildScriptGeneratorContext
            {
                SourceRepo = sourceRepo,
                Properties = new Dictionary<string, string>(),
            };
        }

        private class TestNodePlatform : NodePlatform
        {
            public TestNodePlatform(
                IOptions<BuildScriptGeneratorOptions> cliOptions,
                IOptions<NodeScriptGeneratorOptions> nodeScriptGeneratorOptions,
                INodeVersionProvider nodeVersionProvider,
                ILogger<NodePlatform> logger,
                INodePlatformDetector detector,
                IEnvironment environment,
                NodePlatformInstaller nodePlatformInstaller,
                IExternalSdkProvider externalSdkProvider,
                TelemetryClient telemetryClient)
                : base(
                      cliOptions,
                      nodeScriptGeneratorOptions,
                      nodeVersionProvider,
                      logger,
                      detector,
                      environment,
                      nodePlatformInstaller,
                      externalSdkProvider,
                      telemetryClient)
            {
            }
        }

        private class TestNodePlatformInstaller : NodePlatformInstaller
        {
            private readonly bool _sdkIsAlreadyInstalled;
            public static string InstallerScript = "installer-script-snippet";

            public TestNodePlatformInstaller(
                IOptions<BuildScriptGeneratorOptions> cliOptions,
                bool sdkIsAlreadyInstalled,
                ILoggerFactory loggerFactory)
                : base(cliOptions, loggerFactory)
            {
                _sdkIsAlreadyInstalled = sdkIsAlreadyInstalled;
            }

            public override bool IsVersionAlreadyInstalled(string version)
            {
                return _sdkIsAlreadyInstalled;
            }

            public override string GetInstallerScriptSnippet(string version, bool skipSdkBinaryDownload = false)
            {
                return InstallerScript;
            }
        }

        private class TestNodeVersionProvider : INodeVersionProvider
        {
            private readonly string[] _supportedNodeVersions;
            private readonly string _defaultVersion;

            public TestNodeVersionProvider()
            {
            }

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

        private class TestStandardOutputWriter : IStandardOutputWriter
        {
            public void Write(string message)
            {
            }

            public void WriteLine(string message)
            {
            }

            public void WriteLine()
            {
            }
        }
    }
}
