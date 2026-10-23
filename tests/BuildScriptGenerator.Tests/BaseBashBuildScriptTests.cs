// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class BaseBashBuildScriptTests
    {
        [Theory]
        [InlineData(ManifestFilePropertyKeys.DotNetCoreSdkVersion, true)]
        [InlineData("NodeVersion", false)]
        [InlineData("PythonVersion", false)]
        [InlineData(null, false)]
        public void DotNetRuntimeDirectory_IsConfiguredOnlyForDotNetBuilds(string propertyName, bool expected)
        {
            var scriptProps = new BaseBashBuildScriptProperties
            {
                BuildProperties = propertyName == null
                    ? null
                    : new Dictionary<string, string> { { propertyName, "test-version" } },
            };

            var script = TemplateHelper.Render(TemplateHelper.TemplateResource.BaseBashScript, scriptProps);

            Assert.Equal(expected, scriptProps.IsDotNetCoreBuild);
            if (expected)
            {
                Assert.Contains("oryxDotnetRuntimeDir=\"${XDG_RUNTIME_DIR:-/tmp/oryx-dotnet-runtime-$oryxDotnetUserId}\"", script);
                Assert.Contains("mkdir -m 0700 \"$oryxDotnetRuntimeDir\"", script);
                Assert.Contains("export XDG_RUNTIME_DIR=\"$oryxDotnetRuntimeDir\"", script);
                Assert.Contains("Invalid .NET runtime directory", script);
                Assert.Contains("if ! configure_oryx_dotnet_runtime_directory; then", script);
                Assert.Contains("Warning: continuing build without changing XDG_RUNTIME_DIR", script);

                var setupStart = script.IndexOf("configure_oryx_dotnet_runtime_directory() {", StringComparison.Ordinal);
                var setupEnd = script.IndexOf("unset -f configure_oryx_dotnet_runtime_directory", StringComparison.Ordinal);
                Assert.DoesNotContain("exit ", script.Substring(setupStart, setupEnd - setupStart));
            }
            else
            {
                Assert.DoesNotContain("XDG_RUNTIME_DIR", script);
            }
        }

        [Fact]
        public void DotNetRuntimeDirectory_IsConfiguredBeforePreBuildAndBuildCommands()
        {
            const string installationScript = "install-selected-sdk";
            const string preBuildCommand = "dotnet pre-build";
            const string buildCommand = "dotnet restore";
            var scriptProps = new BaseBashBuildScriptProperties
            {
                BuildProperties = new Dictionary<string, string>
                {
                    { ManifestFilePropertyKeys.DotNetCoreSdkVersion, "9.0.319" },
                },
                PlatformInstallationScript = installationScript,
                PreBuildCommand = preBuildCommand,
                BuildScriptSnippets = new[] { buildCommand },
            };

            var script = TemplateHelper.Render(TemplateHelper.TemplateResource.BaseBashScript, scriptProps);

            var runtimeSetupIndex = script.IndexOf("export XDG_RUNTIME_DIR", StringComparison.Ordinal);
            Assert.True(script.IndexOf(installationScript, StringComparison.Ordinal) < runtimeSetupIndex);
            Assert.True(runtimeSetupIndex < script.IndexOf(preBuildCommand, StringComparison.Ordinal));
            Assert.True(runtimeSetupIndex < script.IndexOf(buildCommand, StringComparison.Ordinal));
        }

        [Fact]
        public void BuildSnippets_ShouldBeIncluded_InOrder()
        {
            // Arrange
            const string script1 = "abcdefg";
            const string script2 = "123456";
            var scriptProps = new BaseBashBuildScriptProperties()
            {
                BuildScriptSnippets = new List<string>() { script1, script2 }
            };

            // Act
            var script = TemplateHelper.Render(TemplateHelper.TemplateResource.BaseBashScript, scriptProps);

            // Assert
            var indexOfScript1 = script.IndexOf(script1);
            var indexOfScript2 = script.IndexOf(script2);
            Assert.True(indexOfScript1 < indexOfScript2);
            Assert.DoesNotContain("Executing pre-build script", script);
            Assert.DoesNotContain("Executing post-build script", script);
        }

        [Fact]
        public void PrePostBuildScripts_ShouldBeIncluded_IfSupplied()
        {
            // Arrange
            const string script1 = "abcdefg";
            const string script2 = "hijklmn";
            var scriptProps = new BaseBashBuildScriptProperties
            {
                PreBuildCommand = script1,
                PostBuildCommand = script2,
            };

            // Act
            var script = TemplateHelper.Render(TemplateHelper.TemplateResource.BaseBashScript, scriptProps);

            // Assert
            Assert.Contains("Executing pre-build command", script);
            Assert.Contains(script1, script);
            Assert.Contains("Executing post-build command", script);
            Assert.Contains(script2, script);
        }
    }
}