// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Tests;
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

        private const string NpmInstallCommand = NodeConstants.NpmPackageInstallCommand;
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

            var expected = new NodeBashBuildSnippetProperties
            {
                PackageInstallCommand = NpmInstallCommand,
                PackageInstallerVersionCommand = NodeConstants.NpmVersionCommand,
                NpmRunBuildCommand = null,
                NpmRunBuildAzureCommand = null,
                HasProdDependencies = true,
                HasDevDependencies = false,
                ProductionOnlyPackageInstallCommand = string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                CompressedNodeModulesFileName = null,
                CompressNodeModulesCommand = null,
            };

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(
                TemplateHelper.Render(TemplateHelper.TemplateResource.NodeBuildSnippet, expected),
                snippet.BashBuildScriptSnippet);
            Assert.DoesNotContain(".npmrc", snippet.BashBuildScriptSnippet); // No custom registry was specified
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

            var expected = new NodeBashBuildSnippetProperties
            {
                PackageInstallCommand = NpmInstallCommand,
                PackageInstallerVersionCommand = NodeConstants.NpmVersionCommand,
                NpmRunBuildCommand = null,
                NpmRunBuildAzureCommand = null,
                HasProdDependencies = false,
                ProductionOnlyPackageInstallCommand = string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                CompressedNodeModulesFileName = null,
                CompressNodeModulesCommand = null,
            };

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);
            var renderedTemplate = TemplateHelper.Render(TemplateHelper.TemplateResource.NodeBuildSnippet, expected);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(renderedTemplate, snippet.BashBuildScriptSnippet);
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

            var expected = new NodeBashBuildSnippetProperties
            {
                PackageInstallCommand = NpmInstallCommand,
                PackageInstallerVersionCommand = NodeConstants.NpmVersionCommand,
                NpmRunBuildCommand = null,
                NpmRunBuildAzureCommand = null,
                HasProdDependencies = false,
                ProductionOnlyPackageInstallCommand = string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                CompressedNodeModulesFileName = null,
                CompressNodeModulesCommand = null,
            };

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

            var expected = new NodeBashBuildSnippetProperties
            {
                PackageInstallCommand = YarnInstallCommand,
                PackageInstallerVersionCommand = NodeConstants.YarnVersionCommand,
                NpmRunBuildCommand = null,
                NpmRunBuildAzureCommand = null,
                HasProdDependencies = false,
                ProductionOnlyPackageInstallCommand = string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    YarnInstallCommand),
                CompressedNodeModulesFileName = null,
                CompressNodeModulesCommand = null,
                ConfigureYarnCache = true,
            };

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

            var expected = new NodeBashBuildSnippetProperties
            {
                PackageInstallCommand = YarnInstallCommand,
                PackageInstallerVersionCommand = NodeConstants.YarnVersionCommand,
                NpmRunBuildCommand = "yarn run build",
                NpmRunBuildAzureCommand = "yarn run build:azure",
                HasProdDependencies = true,
                HasDevDependencies = true,
                ProductionOnlyPackageInstallCommand = string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    YarnInstallCommand),
                CompressedNodeModulesFileName = null,
                CompressNodeModulesCommand = null,
                ConfigureYarnCache = true,
            };

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

            var expected = new NodeBashBuildSnippetProperties
            {
                PackageInstallCommand = NpmInstallCommand,
                PackageInstallerVersionCommand = NodeConstants.NpmVersionCommand,
                NpmRunBuildCommand = null,
                NpmRunBuildAzureCommand = null,
                HasProdDependencies = false,
                ProductionOnlyPackageInstallCommand = string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                CompressedNodeModulesFileName = null,
                CompressNodeModulesCommand = null,
            };

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

            var expected = new NodeBashBuildSnippetProperties
            {
                PackageInstallCommand = NpmInstallCommand,
                PackageInstallerVersionCommand = NodeConstants.NpmVersionCommand,
                NpmRunBuildCommand = "npm run build",
                NpmRunBuildAzureCommand = "npm run build:azure",
                HasProdDependencies = true,
                HasDevDependencies = true,
                ProductionOnlyPackageInstallCommand = string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                CompressedNodeModulesFileName = null,
                CompressNodeModulesCommand = null,
            };

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

            var expected = new NodeBashBuildSnippetProperties
            {
                PackageInstallCommand = NpmInstallCommand,
                PackageInstallerVersionCommand = NodeConstants.NpmVersionCommand,
                NpmRunBuildCommand = "npm run build",
                NpmRunBuildAzureCommand = "npm run build:azure",
                HasProdDependencies = true,
                HasDevDependencies = true,
                ProductionOnlyPackageInstallCommand = string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                CompressedNodeModulesFileName = "node_modules.tar.gz",
                CompressNodeModulesCommand = "tar -zcf",
            };

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(
                TemplateHelper.Render(TemplateHelper.TemplateResource.NodeBuildSnippet, expected),
                snippet.BashBuildScriptSnippet);
            Assert.Contains("echo Zipping existing 'node_modules' folder", snippet.BashBuildScriptSnippet);
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

            var expected = new NodeBashBuildSnippetProperties
            {
                PackageInstallCommand = NpmInstallCommand,
                PackageInstallerVersionCommand = NodeConstants.NpmVersionCommand,
                NpmRunBuildCommand = "npm run build",
                NpmRunBuildAzureCommand = "npm run build:azure",
                HasProdDependencies = true,
                HasDevDependencies = true,
                ProductionOnlyPackageInstallCommand = string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                CompressedNodeModulesFileName = "node_modules.tar.gz",
                CompressNodeModulesCommand = "tar -zcf",
            };

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

            var expected = new NodeBashBuildSnippetProperties
            {
                PackageInstallCommand = NpmInstallCommand,
                PackageInstallerVersionCommand = NodeConstants.NpmVersionCommand,
                NpmRunBuildCommand = "npm run build",
                NpmRunBuildAzureCommand = "npm run build:azure",
                HasProdDependencies = true,
                HasDevDependencies = true,
                ProductionOnlyPackageInstallCommand = string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                CompressedNodeModulesFileName = "node_modules.zip",
                CompressNodeModulesCommand = "zip -y -q -r",
            };

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

            var expected = new NodeBashBuildSnippetProperties
            {
                PackageInstallCommand = NpmInstallCommand,
                PackageInstallerVersionCommand = NodeConstants.NpmVersionCommand,
                NpmRunBuildCommand = "npm run build",
                NpmRunBuildAzureCommand = "npm run build:azure",
                HasProdDependencies = true,
                HasDevDependencies = true,
                ProductionOnlyPackageInstallCommand = string.Format(
                    NodeConstants.ProductionOnlyPackageInstallCommandTemplate,
                    NpmInstallCommand),
                CompressedNodeModulesFileName = null,
                CompressNodeModulesCommand = null,
            };

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
        public void GeneratedScript_WritesNpmRc_WithCustomRegistry()
        {
            // Arrange
            var scriptGenerator = GetNodePlatformInstance(defaultNpmVersion: "6.0.0");
            var repo = new MemorySourceRepo();
            repo.AddFile(PackageJsonWithNpmVersion, NodeConstants.PackageJsonFileName);
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";
            context.Properties[NodePlatform.RegistryUrlPropertyKey] = "https://example.com/registry/";

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Contains(
                $"echo \"registry={context.Properties[NodePlatform.RegistryUrlPropertyKey]}\" >> ~/.npmrc",
                snippet.BashBuildScriptSnippet);
        }

        private static IProgrammingPlatform GetNodePlatformInstance(
            string defaultNodeVersion = null,
            string defaultNpmVersion = null,
            Dictionary<string, string> otherEnvironment = null)
        {
            var environment = new TestEnvironment();
            environment.Variables[NodeConstants.NodeVersion] = defaultNodeVersion;

            if (otherEnvironment != null)
            {
                foreach (var environmentVariable in otherEnvironment.Keys)
                {
                    environment.Variables[environmentVariable] = otherEnvironment[environmentVariable];
                }
            }

            var nodeVersionProvider = new TestNodeVersionProvider(
                new[] { "6.11.0", NodeVersions.Node8Version, NodeVersions.Node10Version, NodeVersions.Node12Version },
                defaultVersion: NodeVersions.Node12Version);

            var nodeScriptGeneratorOptions = Options.Create(new NodeScriptGeneratorOptions());
            var commonOptions = Options.Create(new BuildScriptGeneratorOptions());

            return new NodePlatform(
                commonOptions,
                nodeScriptGeneratorOptions,
                nodeVersionProvider,
                NullLogger<NodePlatform>.Instance,
                detector: null,
                environment,
                new NodePlatformInstaller(commonOptions, environment));
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

        private class TestNodeVersionProvider : INodeVersionProvider
        {
            private readonly string[] _supportedNodeVersions;
            private readonly string _defaultVersion;

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
    }
}