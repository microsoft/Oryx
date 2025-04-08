// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.Detector.Php;
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

        [Fact]
        public void GeneratedScript_UsesComposerInstall()
        {
            // Arrange
            var scriptGenerator = GetScriptGenerator(
                defaultVersion: "7.3",
                new BuildScriptGeneratorOptions(),
                new PhpScriptGeneratorOptions());
            var repo = new MemorySourceRepo();
            repo.AddFile(ComposerFileWithBuildScript, PhpConstants.ComposerFileName);
            var context = CreateBuildScriptGeneratorContext(repo);
            var detectorResult = new PhpPlatformDetectorResult
            {
                Platform = PhpConstants.PlatformName,
                PlatformVersion = "10.10.10",
            };

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context, detectorResult);

            // Assert
            Assert.NotNull(snippet);
            Assert.Contains("$composer install", snippet.BashBuildScriptSnippet);
        }

        //[Fact]
        //public void GeneratedScript_UsesNpmInstall_IfPackageLockJsonFileIsPresent()
        //{
        //    // Arrange
        //    var scriptGenerator = GetScriptGenerator(defaultNpmVersion: "6.0.0");
        //    var repo = new MemorySourceRepo();
        //    repo.AddFile(ComposerFileWithNoNpmVersion, NodeConstants.ComposerFileFileName);
        //    repo.AddFile("Package lock json file content here", NodeConstants.PackageLockJsonFileName);
        //    var context = CreateBuildScriptGeneratorContext(repo);
        //    context.LanguageVersion = "8.2.1";
        //    var expected = new NodeBashBuildSnippetProperties(
        //        packageInstallCommand: NpmInstallCommand,
        //        runBuildCommand: null,
        //        runBuildAzureCommand: null);

        //    // Act
        //    var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

        //    // Assert
        //    Assert.NotNull(snippet);
        //    Assert.Equal(TemplateHelpers.Render(
        //        TemplateHelpers.TemplateResource.NodeSnippet, expected), snippet.BashBuildScriptSnippet);
        //}

        //[Fact]
        //public void GeneratedScript_UsesNpmRunBuild_IfBuildNodeIsPresentUnderScripts()
        //{
        //    // Arrange
        //    var scriptGenerator = GetScriptGenerator(defaultNpmVersion: "6.0.0");
        //    var repo = new MemorySourceRepo();
        //    repo.AddFile(ComposerFileWithBuildScript, NodeConstants.ComposerFileFileName);
        //    var context = CreateBuildScriptGeneratorContext(repo);
        //    context.LanguageVersion = "8.2.1";
        //    var expected = new NodeBashBuildSnippetProperties(
        //        packageInstallCommand: NpmInstallCommand,
        //        runBuildCommand: "npm run build",
        //        runBuildAzureCommand: "npm run build:azure");

        //    // Act
        //    var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

        //    // Assert
        //    Assert.NotNull(snippet);
        //    Assert.Equal(TemplateHelpers.Render(
        //        TemplateHelpers.TemplateResource.NodeSnippet, expected), snippet.BashBuildScriptSnippet);
        //}

        private IProgrammingPlatform GetScriptGenerator(
            string defaultVersion,
            BuildScriptGeneratorOptions commonOptions,
            PhpScriptGeneratorOptions phpScriptGeneratorOptions)
        {
            var phpVersionProvider = new TestPhpVersionProvider(
                supportedPhpVersions: new[] { "7.2.15", PhpVersions.Php73Version });
            var externalSdkProvider = new ExternalSdkProvider(NullLogger<ExternalSdkProvider>.Instance);

            var phpComposerVersionProvider = new TestPhpComposerVersionProvider(
                supportedPhpComposerVersions: new[] { "7.2.15", PhpVersions.ComposerDefaultVersion });

            phpScriptGeneratorOptions = phpScriptGeneratorOptions ?? new PhpScriptGeneratorOptions();
            commonOptions = commonOptions ?? new BuildScriptGeneratorOptions();    
            return new PhpPlatform(
                Options.Create(phpScriptGeneratorOptions),
                Options.Create(commonOptions),
                phpVersionProvider,
                phpComposerVersionProvider,
                NullLogger<PhpPlatform>.Instance,
                detector: null,
                phpInstaller: null,
                phpComposerInstaller: null,
                externalSdkProvider,
                TelemetryClientHelper.GetTelemetryClient());
        }

        private static BuildScriptGeneratorContext CreateBuildScriptGeneratorContext(ISourceRepo sourceRepo)
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = sourceRepo
            };
        }

        private class TestPhpVersionProvider : IPhpVersionProvider
        {
            private readonly string[] _supportedPhpVersions;

            public TestPhpVersionProvider(string[] supportedPhpVersions)
            {
                _supportedPhpVersions = supportedPhpVersions;
            }

            public PlatformVersionInfo GetVersionInfo()
            {
                return PlatformVersionInfo.CreateOnDiskVersionInfo(_supportedPhpVersions, PhpVersions.Php73Version);
            }
        }

        private class TestPhpComposerVersionProvider : IPhpComposerVersionProvider
        {
            private readonly string[] _supportedPhpComposerVersions;

            public TestPhpComposerVersionProvider(string[] supportedPhpComposerVersions)
            {
                _supportedPhpComposerVersions = supportedPhpComposerVersions;
            }

            public PlatformVersionInfo GetVersionInfo()
            {
                return PlatformVersionInfo.CreateOnDiskVersionInfo(
                    _supportedPhpComposerVersions,
                    PhpVersions.ComposerDefaultVersion);
            }
        }
    }
}