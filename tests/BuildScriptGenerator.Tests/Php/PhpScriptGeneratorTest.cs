// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.Tests.Common;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Php
{
    public class PhpScriptGeneratorTest
    {
        private const string ComposerFileWithBuildScript = @"{
          ""name"": ""microsoft/myphpapp"",
          ""version"": ""1.0.0"",
          ""description"": ""test app"",
          ""scripts"": {
            ""build"": ""tsc"",
            ""build:azure"": ""tsc"",
          },
          ""authors"": [{
            ""name"": ""Bla Bla"",
            ""email"": ""blabla@microsoft.com"",
            ""homepage"": ""http://www.microsoft.com"",
            ""role"": ""Developer""
          }],
          ""license"": ""ISC"",
          ""require"": {
            ""monolog/monolog"": ""1.0.*"",
            ""ext-mbstring"": ""*""
          },
          ""require-dev"": {
            ""phpunit"": ""*""
          }
        }";

        private const string MalformedComposerFile = @"{
          ""name"": ""microsoft/myphpapp"",
          ""version"": ""1.0.0"",
          ""description"": ""test app"",
          ""scripts"": {
            ""test"": ""echo ,
            ""start"": ""@php server.php""
          },
          ""license"": ""ISC""
        }";

        private const string NpmInstallCommand = "php /path/to/composer.phar install";

        [Fact]
        public void GeneratedScript_UsesYarnInstallAndRunsNpmBuild_IfYarnLockFileIsPresent_AndBuildNodeIsPresentUnderScripts()
        {
            // Arrange
            var scriptGenerator = GetScriptGenerator(defaultNpmVersion: "6.0.0");
            var repo = new CachedSourceRepo();
            repo.AddFile(ComposerFileWithBuildScript, NodeConstants.ComposerFileFileName);
            repo.AddFile("Yarn lock file content here", NodeConstants.YarnLockFileName);
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";
            var expected = new NodeBashBuildSnippetProperties(
                packageInstallCommand: YarnInstallCommand,
                runBuildCommand: "yarn run build",
                runBuildAzureCommand: "yarn run build:azure");

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(TemplateHelpers.Render(TemplateHelpers.TemplateResource.NodeSnippet, expected), snippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedScript_UsesNpmInstall_IfPackageLockJsonFileIsPresent()
        {
            // Arrange
            var scriptGenerator = GetScriptGenerator(defaultNpmVersion: "6.0.0");
            var repo = new CachedSourceRepo();
            repo.AddFile(ComposerFileWithNoNpmVersion, NodeConstants.ComposerFileFileName);
            repo.AddFile("Package lock json file content here", NodeConstants.PackageLockJsonFileName);
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";
            var expected = new NodeBashBuildSnippetProperties(
                packageInstallCommand: NpmInstallCommand,
                runBuildCommand: null,
                runBuildAzureCommand: null);

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(TemplateHelpers.Render(TemplateHelpers.TemplateResource.NodeSnippet, expected), snippet.BashBuildScriptSnippet);
        }

        [Fact]
        public void GeneratedScript_UsesNpmRunBuild_IfBuildNodeIsPresentUnderScripts()
        {
            // Arrange
            var scriptGenerator = GetScriptGenerator(defaultNpmVersion: "6.0.0");
            var repo = new CachedSourceRepo();
            repo.AddFile(ComposerFileWithBuildScript, NodeConstants.ComposerFileFileName);
            var context = CreateScriptGeneratorContext(repo);
            context.LanguageVersion = "8.2.1";
            var expected = new NodeBashBuildSnippetProperties(
                packageInstallCommand: NpmInstallCommand,
                runBuildCommand: "npm run build",
                runBuildAzureCommand: "npm run build:azure");

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Equal(TemplateHelpers.Render(TemplateHelpers.TemplateResource.NodeSnippet, expected), snippet.BashBuildScriptSnippet);
        }

        private IProgrammingPlatform GetScriptGenerator(string defaultVersion = null)
        {
            var environment = new TestEnvironment();
            environment.Variables[PhpConstants.DefaultPhpRuntimeVersionEnvVarName] = defaultVersion;

            var phpVersionProvider = new TestPhpVersionProvider(new[] { "7.2.15", Common.PhpVersions.Php73Version });

            var scriptGeneratorOptions = Options.Create(new PhpScriptGeneratorOptions());
            var optionsSetup = new PhpScriptGeneratorOptionsSetup(environment);
            optionsSetup.Configure(scriptGeneratorOptions.Value);

            return new PhpPlatform(
                nodeScriptGeneratorOptions,
                nodeVersionProvider,
                NullLogger<NodePlatform>.Instance,
                null);
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
    }

    class TestPhpVersionProvider : IPhpVersionProvider
    {
        public TestPhpVersionProvider(string[] supportedVersions)
        {
            SupportedPhpVersions = supportedVersions;
        }

        public IEnumerable<string> SupportedPhpVersions { get; }
    }
}