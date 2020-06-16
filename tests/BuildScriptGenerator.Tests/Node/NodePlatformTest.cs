// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Node;
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
            var detectorResult = new PlatformDetectorResult
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
            var detectorResult = new PlatformDetectorResult
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
            var detectorResult = new PlatformDetectorResult
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
        public void BuildScript_HasSdkInstallScript_IfDynamicInstallIsEnabled_AndSdkIsNotAlreadyInstalled()
        {
            // Arrange
            var nodePlatform = CreateNodePlatform(dynamicInstallIsEnabled: true, sdkAlreadyInstalled: false);
            var repo = new MemorySourceRepo();
            repo.AddFile(string.Empty, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);
            var detectorResult = new PlatformDetectorResult
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
            var detectorResult = new PlatformDetectorResult
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
            var detectorResult = new PlatformDetectorResult
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
            var detectorResult = new PlatformDetectorResult
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
            var detectorResult = new PlatformDetectorResult
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
            var detectorResult = new PlatformDetectorResult
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
            var detectorResult = new PlatformDetectorResult
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
                "key 'RUN_BUILD_COMMAND'.",
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
            var detectorResult = new PlatformDetectorResult
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
        public void Detect_ReturnsDefaultVersionOfVersionProvider_IfNoVersionFoundInPackageJson_OrOptions()
        {
            // Arrange
            var expectedVersion = "8.11.2";
            var detector = CreateNodePlatform(
                supportedNodeVersions: new[] { "6.11.0", expectedVersion, "10.14.0" },
                defaultVersion: expectedVersion);
            var repo = new MemorySourceRepo();
            repo.AddFile(SamplePackageJsonContents.PackageJsonWithNoVersions, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

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
            var detector = CreateNodePlatform(
                supportedNodeVersions: supportedVersions,
                defaultVersion: defaultVersion);
            var repo = new MemorySourceRepo();
            repo.AddFile(SamplePackageJsonContents.PackageJsonWithNoVersions, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NodeConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsResult_WithDefaultVersion_ForMalformedPackageJson()
        {
            // Arrange
            var expectedVersion = "1.2.3";
            var detector = CreateNodePlatform(
                supportedNodeVersions: new[] { expectedVersion },
                defaultVersion: expectedVersion);
            var repo = new MemorySourceRepo();
            repo.AddFile(SamplePackageJsonContents.MalformedPackageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NodeConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedNodeVersion_IsDetected()
        {
            // Arrange
            var unsupportedVersion = "20.20.20";
            var supportedVersion = "6.11.0";
            var detector = CreateNodePlatform(
                supportedNodeVersions: new[] { supportedVersion },
                defaultVersion: supportedVersion);
            var repo = new MemorySourceRepo();
            var context = CreateContext(repo);
            repo.AddFile(
                SamplePackageJsonContents.PackageJsonTemplateWithNodeVersion
                    .Replace("#VERSION_RANGE#", unsupportedVersion),
                NodeConstants.PackageJsonFileName);

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => detector.Detect(context));
            Assert.Equal(
                $"Platform '{NodeConstants.PlatformName}' version '{unsupportedVersion}' is unsupported. " +
                $"Supported versions: {supportedVersion}",
                exception.Message);
        }

        [Fact]
        public void Detect_Throws_WhenUnsupportedPhpVersion_IsSetInOptions()
        {
            // Arrange
            var unsupportedVersion = "20.20.20";
            var supportedVersion = "6.11.0";
            var nodeScriptGeneratorOptions = new NodeScriptGeneratorOptions();
            nodeScriptGeneratorOptions.NodeVersion = unsupportedVersion;
            var detector = CreateNodePlatform(
                supportedNodeVersions: new[] { supportedVersion },
                defaultVersion: supportedVersion,
                nodeScriptGeneratorOptions: nodeScriptGeneratorOptions);
            var repo = new MemorySourceRepo();
            var context = CreateContext(repo);
            repo.AddFile(
                SamplePackageJsonContents.PackageJsonTemplateWithNodeVersion
                    .Replace("#VERSION_RANGE#", supportedVersion),
                NodeConstants.PackageJsonFileName);

            // Act & Assert
            var exception = Assert.Throws<UnsupportedVersionException>(() => detector.Detect(context));
            Assert.Equal(
                $"Platform '{NodeConstants.PlatformName}' version '{unsupportedVersion}' is unsupported. " +
                $"Supported versions: {supportedVersion}",
                exception.Message);
        }

        [Fact]
        public void Detect_ReturnsResult_WithDefaultVersion_ForPackageJsonWithOnlyNpmVersion()
        {
            // Node detector only looks for node version and not the NPM version. The individual script
            // generator looks for npm version.

            // Arrange
            var expectedVersion = "1.2.3";
            var detector = CreateNodePlatform(
                supportedNodeVersions: new[] { expectedVersion },
                defaultVersion: expectedVersion);
            var repo = new MemorySourceRepo();
            repo.AddFile(SamplePackageJsonContents.PackageJsonWithOnlyNpmVersion, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NodeConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsResult_WithNodeVersionFromOptions_ForPackageJsonWithNoNodeVersion()
        {
            // Arrange
            var expectedVersion = "500.500.500";
            var nodeScriptGeneratorOptions = new NodeScriptGeneratorOptions();
            nodeScriptGeneratorOptions.NodeVersion = expectedVersion;
            var detector = CreateNodePlatform(
                supportedNodeVersions: new[] { expectedVersion },
                defaultVersion: expectedVersion,
                nodeScriptGeneratorOptions: nodeScriptGeneratorOptions);
            var repo = new MemorySourceRepo();
            repo.AddFile(SamplePackageJsonContents.PackageJsonWithNoVersions, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NodeConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Fact]
        public void Detect_ReturnsVersionFromOptions_EvenIfPackageJsonHasVersion()
        {
            // Arrange
            var versionInPackageJson = "6.11.0";
            var expectedVersionToBeUsed = "10.14.0";
            var defaultVersion = "8.11.2";
            var nodeScriptGeneratorOptions = new NodeScriptGeneratorOptions();
            nodeScriptGeneratorOptions.NodeVersion = expectedVersionToBeUsed;
            var detector = CreateNodePlatform(
                supportedNodeVersions: new[] { versionInPackageJson, defaultVersion, expectedVersionToBeUsed },
                defaultVersion: defaultVersion,
                nodeScriptGeneratorOptions: nodeScriptGeneratorOptions);
            var repo = new MemorySourceRepo();
            var packageJson = SamplePackageJsonContents.PackageJsonTemplateWithNodeVersion.Replace(
                "#VERSION_RANGE#",
                versionInPackageJson);
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

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
            var detector = CreateNodePlatform(
                supportedNodeVersions: new[] { expectedVersion },
                defaultVersion: expectedVersion);
            var repo = new MemorySourceRepo();
            repo.AddFile(SamplePackageJsonContents.SimpleServerJs, "server.js");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

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
            var detector = CreateNodePlatform(
                supportedNodeVersions: supportedVersions,
                defaultVersion: defaultVersion);
            var repo = new MemorySourceRepo();
            var packageJson = SamplePackageJsonContents.PackageJsonTemplateWithNodeVersion.Replace(
                "#VERSION_RANGE#",
                versionRangeInPackageJson);
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NodeConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        [Theory]
        [InlineData(new[] { "8.11.2", "5.6.9", "8.11.13" }, "5.6.9", ">5", "8.11.13")]
        [InlineData(new[] { "8.9.5", "8.11.2", "5.6.9", "8.11.13" }, "5.6.9", ">8.9", "8.11.13")]
        [InlineData(new[] { "8.9.5", "8.11.2", "11.12.0", "13.12.12", "8.11.13" }, "5.6.9", ">8.9 <13", "11.12.0")]
        public void Detect_ReturnsVersionFromOptions__WithVersionSatisfying_NodeVersionRangeInPackageJson(
            string[] supportedVersions,
            string defaultVersion,
            string versionRangeInOptions,
            string expectedVersion)
        {
            // Arrange
            var nodeScriptGeneratorOptions = new NodeScriptGeneratorOptions();
            nodeScriptGeneratorOptions.NodeVersion = versionRangeInOptions;
            var detector = CreateNodePlatform(
                supportedNodeVersions: supportedVersions,
                defaultVersion: defaultVersion,
                nodeScriptGeneratorOptions: nodeScriptGeneratorOptions);
            var repo = new MemorySourceRepo();
            var packageJson = SamplePackageJsonContents.PackageJsonTemplateWithNodeVersion.Replace(
                "#VERSION_RANGE#",
                "1.1.1");
            repo.AddFile(packageJson, NodeConstants.PackageJsonFileName);
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

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
            var detector = CreateNodePlatform(
                supportedNodeVersions: new[] { expectedVersion },
                defaultVersion: expectedVersion);
            var repo = new MemorySourceRepo();
            repo.AddFile("app.js content", "app.js");
            var context = CreateContext(repo);

            // Act
            var result = detector.Detect(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NodeConstants.PlatformName, result.Platform);
            Assert.Equal(expectedVersion, result.PlatformVersion);
        }

        private TestNodePlatform CreateNodePlatform(
            string[] supportedNodeVersions = null,
            string defaultVersion = null,
            BuildScriptGeneratorOptions commonOptions = null,
            NodeScriptGeneratorOptions nodeScriptGeneratorOptions = null)
        {
            nodeScriptGeneratorOptions = nodeScriptGeneratorOptions ?? new NodeScriptGeneratorOptions();
            commonOptions = commonOptions ?? new BuildScriptGeneratorOptions();
            var versionProvider = new TestNodeVersionProvider(supportedNodeVersions, defaultVersion);
            var environment = new TestEnvironment();
            var detector = new TestNodePlatformDetector(
                versionProvider,
                Options.Create(nodeScriptGeneratorOptions),
                NullLogger<NodePlatformDetector>.Instance,
                environment,
                new TestStandardOutputWriter());
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
                platformInstaller);
        }

        private TestNodePlatform CreateNodePlatform(
            BuildScriptGeneratorOptions commonOptions,
            NodeScriptGeneratorOptions nodeScriptGeneratorOptions,
            NodePlatformInstaller platformInstaller)
        {
            var environment = new TestEnvironment();

            var versionProvider = new TestNodeVersionProvider();
            var detector = new TestNodePlatformDetector(
                versionProvider,
                Options.Create(nodeScriptGeneratorOptions),
                NullLogger<NodePlatformDetector>.Instance,
                environment,
                new TestStandardOutputWriter());
            return new TestNodePlatform(
                Options.Create(commonOptions),
                Options.Create(nodeScriptGeneratorOptions),
                versionProvider,
                NullLogger<NodePlatform>.Instance,
                detector,
                environment,
                platformInstaller);
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
            var nodeScriptGeneratorOptions = new NodeScriptGeneratorOptions();
            var detector = new TestNodePlatformDetector(
                versionProvider,
                Options.Create(nodeScriptGeneratorOptions),
                NullLogger<NodePlatformDetector>.Instance,
                environment,
                new TestStandardOutputWriter());
            return new TestNodePlatform(
                Options.Create(cliOptions),
                Options.Create(nodeScriptGeneratorOptions),
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
                NodePlatformDetector detector,
                IEnvironment environment,
                NodePlatformInstaller nodePlatformInstaller)
                : base(
                      cliOptions,
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

            public override string GetInstallerScriptSnippet(string version)
            {
                return InstallerScript;
            }
        }

        private class TestNodePlatformDetector : NodePlatformDetector
        {
            public TestNodePlatformDetector(
                INodeVersionProvider nodeVersionProvider,
                IOptions<NodeScriptGeneratorOptions> nodeScriptGeneratorOptions,
                ILogger<NodePlatformDetector> logger,
                IEnvironment environment,
                IStandardOutputWriter writer)
                : base(nodeVersionProvider, nodeScriptGeneratorOptions, logger, environment, writer)
            {
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
