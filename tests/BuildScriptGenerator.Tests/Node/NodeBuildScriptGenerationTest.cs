// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Node
{
    /// <summary>
    /// Component tests for NodeJs support.
    /// </summary>
    public class NodeBuildScriptGenerationTest
    {
        private const string PackageJsonWithBuildScript = @"{
          ""name"": ""mynodeapp"",
          ""version"": ""1.0.0"",
          ""description"": ""test app"",
          ""main"": ""server.js"",
          ""scripts"": {
            ""build"": ""tsc"",
            ""build:azure"": ""tsc"",
          },
          ""author"": ""Dev"",
          ""license"": ""ISC"",
          ""dependencies"": {
            ""@types/node"": ""7.0.22"",
            ""express"": ""4.15.3"",
          },
          ""devDependencies"": {
            ""@types/mocha"": ""2.2.41"",
            ""@types/node"": ""7.0.22"",
            ""tsc"": ""1.20150623.0"",
            ""typescript"": ""2.3.3"",
            ""typescript-eslint-parser"": ""3.0.0""
          }
        }";

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

        private const string NpmInstallCommand = "npm install";
        private const string YarnInstallCommand = "yarn install --prefer-offline";

        [Fact]
        public void GeneratedScript_HasNpmVersion_SpecifiedInPackageJson()
        {
            // Arrange
            var scriptGenerator = GetNodePlatformInstance(defaultNpmVersion: "6.0.0");
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithNpmVersion, NodeConstants.PackageJsonFileName);
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";
            var expected = new NodeBashBuildSnippetProperties(
                packageInstallCommand: NpmInstallCommand,
                runBuildCommand: null,
                runBuildAzureCommand: null,
                hasProductionOnlyDependencies: false,
                productionOnlyPackageInstallCommand: string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                compressedNodeModulesFileName: null,
                compressNodeModulesCommand: null);

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(
                TemplateHelper.Render(TemplateHelper.TemplateResource.NodeBuildSnippet, expected),
                snippet.BashBuildScriptSnippet);
            Assert.True(scriptGenerator.IsCleanRepo(repo));
        }

        [Fact]
        public void GeneratedScript_HasDefaultNpmVersion_IfPackageJsonDoesNotHaveOne()
        {
            // Arrange
            var scriptGenerator = GetNodePlatformInstance(defaultNpmVersion: "6.0.0");
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithNoNpmVersion, NodeConstants.PackageJsonFileName);
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";
            var expected = new NodeBashBuildSnippetProperties(
                packageInstallCommand: NpmInstallCommand,
                runBuildCommand: null,
                runBuildAzureCommand: null,
                hasProductionOnlyDependencies: false,
                productionOnlyPackageInstallCommand: string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                compressedNodeModulesFileName: null,
                compressNodeModulesCommand: null);

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(
                TemplateHelper.Render(TemplateHelper.TemplateResource.NodeBuildSnippet, expected),
                snippet.BashBuildScriptSnippet);
            Assert.True(scriptGenerator.IsCleanRepo(repo));
        }

        [Fact]
        public void GeneratesScript_WithDefaultNpmVersion_ForMalformedPackageJson()
        {
            // Arrange
            var scriptGenerator = GetNodePlatformInstance(defaultNpmVersion: "5.4.2");
            var repo = new MemorySourceRepo();
            repo.AddFile(MalformedPackageJson, NodeConstants.PackageJsonFileName);
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";
            var expected = new NodeBashBuildSnippetProperties(
                packageInstallCommand: NpmInstallCommand,
                runBuildCommand: null,
                runBuildAzureCommand: null,
                hasProductionOnlyDependencies: false,
                productionOnlyPackageInstallCommand: string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                compressedNodeModulesFileName: null,
                compressNodeModulesCommand: null);

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(
                TemplateHelper.Render(TemplateHelper.TemplateResource.NodeBuildSnippet, expected),
                snippet.BashBuildScriptSnippet);
            Assert.True(scriptGenerator.IsCleanRepo(repo));
        }

        [Fact]
        public void GeneratedScript_UsesYarnInstall_IfYarnLockFileIsPresent()
        {
            // Arrange
            var scriptGenerator = GetNodePlatformInstance(defaultNpmVersion: "6.0.0");
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithNoNpmVersion, NodeConstants.PackageJsonFileName);
            repo.AddFile("Yarn lock file content here", NodeConstants.YarnLockFileName);
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";
            var expected = new NodeBashBuildSnippetProperties(
                packageInstallCommand: YarnInstallCommand,
                runBuildCommand: null,
                runBuildAzureCommand: null,
                hasProductionOnlyDependencies: false,
                productionOnlyPackageInstallCommand: string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    YarnInstallCommand),
                compressedNodeModulesFileName: null,
                compressNodeModulesCommand: null,
                configureYarnCache: true);

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(
                TemplateHelper.Render(TemplateHelper.TemplateResource.NodeBuildSnippet, expected),
                snippet.BashBuildScriptSnippet);
            Assert.True(scriptGenerator.IsCleanRepo(repo));
        }

        [Fact]
        public void GeneratedScript_UsesYarnInstallAndRunsNpmBuild_IfYarnLockIsPresent_AndHasBuildNodeUnderScripts()
        {
            // Arrange
            var scriptGenerator = GetNodePlatformInstance(defaultNpmVersion: "6.0.0");
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithBuildScript, NodeConstants.PackageJsonFileName);
            repo.AddFile("Yarn lock file content here", NodeConstants.YarnLockFileName);
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";
            var expected = new NodeBashBuildSnippetProperties(
                packageInstallCommand: YarnInstallCommand,
                runBuildCommand: "yarn run build",
                runBuildAzureCommand: "yarn run build:azure",
                hasProductionOnlyDependencies: true,
                productionOnlyPackageInstallCommand: string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    YarnInstallCommand),
                compressedNodeModulesFileName: null,
                compressNodeModulesCommand: null,
                configureYarnCache: true);

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(
                TemplateHelper.Render(TemplateHelper.TemplateResource.NodeBuildSnippet, expected),
                snippet.BashBuildScriptSnippet);
            Assert.True(scriptGenerator.IsCleanRepo(repo));
        }

        [Fact]
        public void GeneratedScript_UsesNpmInstall_IfPackageLockJsonFileIsPresent()
        {
            // Arrange
            var scriptGenerator = GetNodePlatformInstance(defaultNpmVersion: "6.0.0");
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithNoNpmVersion, NodeConstants.PackageJsonFileName);
            repo.AddFile("Package lock json file content here", NodeConstants.PackageLockJsonFileName);
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";
            var expected = new NodeBashBuildSnippetProperties(
                packageInstallCommand: NpmInstallCommand,
                runBuildCommand: null,
                runBuildAzureCommand: null,
                hasProductionOnlyDependencies: false,
                productionOnlyPackageInstallCommand: string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                compressedNodeModulesFileName: null,
                compressNodeModulesCommand: null);

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(
                TemplateHelper.Render(TemplateHelper.TemplateResource.NodeBuildSnippet, expected),
                snippet.BashBuildScriptSnippet);
            Assert.True(scriptGenerator.IsCleanRepo(repo));
        }

        [Fact]
        public void GeneratedScript_UsesNpmRunBuild_IfBuildNodeIsPresentUnderScripts()
        {
            // Arrange
            var scriptGenerator = GetNodePlatformInstance(defaultNpmVersion: "6.0.0");
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithBuildScript, NodeConstants.PackageJsonFileName);
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";
            var expected = new NodeBashBuildSnippetProperties(
                packageInstallCommand: NpmInstallCommand,
                runBuildCommand: "npm run build",
                runBuildAzureCommand: "npm run build:azure",
                hasProductionOnlyDependencies: true,
                productionOnlyPackageInstallCommand: string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                compressedNodeModulesFileName: null,
                compressNodeModulesCommand: null);

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(
                TemplateHelper.Render(TemplateHelper.TemplateResource.NodeBuildSnippet, expected),
                snippet.BashBuildScriptSnippet);
            Assert.True(scriptGenerator.IsCleanRepo(repo));
        }

        [Fact]
        public void GeneratedScript_ZipsNodeModules_IfZipNodeProperty_IsTarGz()
        {
            // Arrange
            var scriptGenerator = GetNodePlatformInstance(defaultNpmVersion: "6.0.0");
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithBuildScript, NodeConstants.PackageJsonFileName);
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";
            context.Properties["compress_node_modules"] = "tar-gz";
            var expected = new NodeBashBuildSnippetProperties(
                packageInstallCommand: NpmInstallCommand,
                runBuildCommand: "npm run build",
                runBuildAzureCommand: "npm run build:azure",
                hasProductionOnlyDependencies: true,
                productionOnlyPackageInstallCommand: string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                compressedNodeModulesFileName: "node_modules.tar.gz",
                compressNodeModulesCommand: "tar -zcf");

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Contains("echo Zipping existing 'node_modules' folder", snippet.BashBuildScriptSnippet);
            Assert.Equal(
                TemplateHelper.Render(TemplateHelper.TemplateResource.NodeBuildSnippet, expected),
                snippet.BashBuildScriptSnippet);
            Assert.True(scriptGenerator.IsCleanRepo(repo));
        }

        [Fact]
        public void GeneratedScript_ZipsNodeModules_IfZipNodeProperty_IsNull()
        {
            // Arrange
            var scriptGenerator = GetNodePlatformInstance(defaultNpmVersion: "6.0.0");
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithBuildScript, NodeConstants.PackageJsonFileName);
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";
            context.Properties["compress_node_modules"] = null;
            var expected = new NodeBashBuildSnippetProperties(
                packageInstallCommand: NpmInstallCommand,
                runBuildCommand: "npm run build",
                runBuildAzureCommand: "npm run build:azure",
                hasProductionOnlyDependencies: true,
                productionOnlyPackageInstallCommand: string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                compressedNodeModulesFileName: "node_modules.tar.gz",
                compressNodeModulesCommand: "tar -zcf");

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Contains("echo Zipping existing 'node_modules' folder", snippet.BashBuildScriptSnippet);
            Assert.Equal(
                TemplateHelper.Render(TemplateHelper.TemplateResource.NodeBuildSnippet, expected),
                snippet.BashBuildScriptSnippet);
            Assert.True(scriptGenerator.IsCleanRepo(repo));
        }

        [Fact]
        public void GeneratedScript_ZipsNodeModules_IfZipNodeProperty_IsZip()
        {
            // Arrange
            var scriptGenerator = GetNodePlatformInstance(defaultNpmVersion: "6.0.0");
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithBuildScript, NodeConstants.PackageJsonFileName);
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";
            context.Properties["compress_node_modules"] = "zip";
            var expected = new NodeBashBuildSnippetProperties(
                packageInstallCommand: NpmInstallCommand,
                runBuildCommand: "npm run build",
                runBuildAzureCommand: "npm run build:azure",
                hasProductionOnlyDependencies: true,
                productionOnlyPackageInstallCommand: string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                compressedNodeModulesFileName: "node_modules.zip",
                compressNodeModulesCommand: "zip -y -q -r");

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Contains("echo Zipping existing 'node_modules' folder", snippet.BashBuildScriptSnippet);
            Assert.Equal(
                TemplateHelper.Render(TemplateHelper.TemplateResource.NodeBuildSnippet, expected),
                snippet.BashBuildScriptSnippet);
            Assert.True(scriptGenerator.IsCleanRepo(repo));
        }

        [Fact]
        public void GeneratedScript_DoesNotZipNodeModules_IfZipNodeModulesEnvironmentVariable_False()
        {
            // Arrange
            var scriptGenerator = GetNodePlatformInstance(defaultNpmVersion: "6.0.0");
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithBuildScript, NodeConstants.PackageJsonFileName);
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";
            var expected = new NodeBashBuildSnippetProperties(
                packageInstallCommand: NpmInstallCommand,
                runBuildCommand: "npm run build",
                runBuildAzureCommand: "npm run build:azure",
                hasProductionOnlyDependencies: true,
                productionOnlyPackageInstallCommand: string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                compressedNodeModulesFileName: null,
                compressNodeModulesCommand: null);

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(
                TemplateHelper.Render(TemplateHelper.TemplateResource.NodeBuildSnippet, expected),
                snippet.BashBuildScriptSnippet);
            Assert.True(scriptGenerator.IsCleanRepo(repo));
        }

        [Theory]
        [MemberData(nameof(TestValueGenerator.GetNodeVersions_DoesNotSupportDebugging),
            MemberType = typeof(TestValueGenerator))]
        public void GeneratedScript_DoesNotConfigureAppInsights_IfAppInsightsEnvironmentVariable_NotSet(
            string nodeVersion)
        {
            // Arrange
            var scriptGenerator = GetNodePlatformInstance(defaultNodeVersion: nodeVersion);
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithBuildScript, NodeConstants.PackageJsonFileName);
            var context = CreateScriptGeneratorContext(repo);
            context.NodeVersion = nodeVersion;
            var expected = new NodeBashBuildSnippetProperties(
                packageInstallCommand: NpmInstallCommand,
                runBuildCommand: "npm run build",
                runBuildAzureCommand: "npm run build:azure",
                hasProductionOnlyDependencies: true,
                productionOnlyPackageInstallCommand: string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                compressedNodeModulesFileName: null,
                compressNodeModulesCommand: null,
                appInsightsInjectCommand: null,
                appInsightsPackageName: "applicationinsights",
                appInsightsLoaderFileName: "oryx-appinsightsloader.js");

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.DoesNotContain("applicationinsights", snippet.BashBuildScriptSnippet);
            Assert.Equal(
                TemplateHelper.Render(TemplateHelper.TemplateResource.NodeBuildSnippet, expected),
                snippet.BashBuildScriptSnippet);
            Assert.True(scriptGenerator.IsCleanRepo(repo));
        }

        [Theory]
        [MemberData(nameof(TestValueGenerator.GetNodeVersions_DoesNotSupportDebugging),
            MemberType = typeof(TestValueGenerator))]
        public void GeneratedScript_DoesNotConfigureAppInsights_IfNodeVersionCondition_Unsatisfied(string nodeVersion)
        {
            // Arrange
            var otherEnvironment = new Dictionary<string, string> { { Constants.AppInsightsKey, "xyz" } };
            var scriptGenerator = GetNodePlatformInstance(
                defaultNodeVersion: nodeVersion, otherEnvironment: otherEnvironment);
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithBuildScript, NodeConstants.PackageJsonFileName);
            var context = CreateScriptGeneratorContext(repo);
            context.NodeVersion = nodeVersion;
            var expected = new NodeBashBuildSnippetProperties(
                packageInstallCommand: NpmInstallCommand,
                runBuildCommand: "npm run build",
                runBuildAzureCommand: "npm run build:azure",
                hasProductionOnlyDependencies: true,
                productionOnlyPackageInstallCommand: string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                compressedNodeModulesFileName: null,
                compressNodeModulesCommand: null,
                appInsightsInjectCommand: null,
                appInsightsPackageName: "applicationinsights",
                appInsightsLoaderFileName: "oryx-appinsightsloader.js");

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.DoesNotContain("applicationinsights", snippet.BashBuildScriptSnippet);
            Assert.Equal(
                TemplateHelper.Render(TemplateHelper.TemplateResource.NodeBuildSnippet, expected),
                snippet.BashBuildScriptSnippet);
            Assert.True(scriptGenerator.IsCleanRepo(repo));
        }

        [Theory]
        [MemberData(nameof(TestValueGenerator.GetNodeVersions_SupportDebugging),
            MemberType = typeof(TestValueGenerator))]
        public void GeneratedScript_ConfigureAppInsights_Condition_Satisfied(string version)
        {
            // Condition is node version have to be 8 or newer or node version is 6.12.0
            // As we don't support 6.12.0 in our build image we condition remains node 8 or newer
            // Arrange
            var otherEnvironment = new Dictionary<string, string> { { Constants.AppInsightsKey, "xyz" } };
            var scriptGenerator = GetNodePlatformInstance(
                defaultNodeVersion: version, otherEnvironment: otherEnvironment);
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithBuildScript, NodeConstants.PackageJsonFileName);
            var context = CreateScriptGeneratorContext(repo);
            context.NodeVersion = version;
            //context.LanguageVersion = version;
            var expected = new NodeBashBuildSnippetProperties(
                packageInstallCommand: NpmInstallCommand,
                runBuildCommand: "npm run build",
                runBuildAzureCommand: "npm run build:azure",
                hasProductionOnlyDependencies: true,
                productionOnlyPackageInstallCommand: string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                compressedNodeModulesFileName: null,
                compressNodeModulesCommand: null,
                appInsightsInjectCommand: "npm install --save applicationinsights",
                appInsightsPackageName: "applicationinsights",
                appInsightsLoaderFileName: "oryx-appinsightsloader.js");

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Contains("applicationinsights", snippet.BashBuildScriptSnippet);
            Assert.Equal(
                TemplateHelper.Render(TemplateHelper.TemplateResource.NodeBuildSnippet, expected),
                snippet.BashBuildScriptSnippet);
            Assert.True(scriptGenerator.IsCleanRepo(repo));
        }

        private IProgrammingPlatform GetNodePlatformInstance(
            string defaultNodeVersion = null,
            string defaultNpmVersion = null,
            Dictionary<string, string> otherEnvironment = null)
        {
            var environment = new TestEnvironment();
            environment.Variables[NodeScriptGeneratorOptionsSetup.NodeJsDefaultVersion] = defaultNodeVersion;
            environment.Variables[NodeScriptGeneratorOptionsSetup.NpmDefaultVersion] = defaultNpmVersion;

            if (otherEnvironment != null)
            {
                foreach (var environmentVariable in otherEnvironment.Keys)
                {
                    environment.Variables[environmentVariable] = otherEnvironment[environmentVariable];
                }
            }
            
            var nodeVersionProvider = new TestVersionProvider(new[] { "6.11.0", "8.2.1" }, new[] { "5.4.2", "6.0.0" });

            var nodeScriptGeneratorOptions = Options.Create(new NodeScriptGeneratorOptions());
            var optionsSetup = new NodeScriptGeneratorOptionsSetup(environment);
            optionsSetup.Configure(nodeScriptGeneratorOptions.Value);

            return new NodePlatform(
                nodeScriptGeneratorOptions,
                nodeVersionProvider,
                NullLogger<NodePlatform>.Instance,
                null,
                environment);
        }

        private static BuildScriptGeneratorContext CreateScriptGeneratorContext(
            ISourceRepo sourceRepo,
            string languageName = null,
            string languageVersion = null)
        {
            return new BuildScriptGeneratorContext
            {
                Language = languageName,
                LanguageVersion = languageVersion,
                SourceRepo = sourceRepo,
                Properties = new Dictionary<string, string>()
            };
        }
    }
}