// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class OryxCommandTest : SampleAppsTestBase
    {
        private const int UnsupportedPlatform = 2;
        private const int UnsupportedPlatformVersion = 3;

        public OryxCommandTest(ITestOutputHelper output) : base(output) { }

        [Fact, Trait("category", "latest")]
        public void Build_ReturnsExpectedErrorCode_ForUnsupportedPlatformException()
        {
            // Arrange
            var script = new ShellScriptBuilder()
               .AddCommand("mkdir /tmp/app")
               .AddCommand("oryx build /tmp/app -o /tmp/out")
               .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.Equal(UnsupportedPlatform, result.ExitCode);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "latest")]
        public void Build_ReturnsExpectedErrorCode_ForUnsupportedVersionException()
        {
            // Arrange
            var script = new ShellScriptBuilder()
               .AddCommand("mkdir /tmp/app")
               .AddCommand($"oryx build /tmp/app -o /tmp/out --platform {DotNetCoreConstants.PlatformName} --platform-version 0.0")
               .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.Equal(UnsupportedPlatformVersion, result.ExitCode);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "latest")]
        public void BuildImage_Build_UsesCwd_WhenNoSourceDirGiven()
        {
            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                CommandToExecuteOnRun = "oryx",
                CommandArguments = new[] { "build" },
                WorkingDirectory = "/tmp"
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.Contains("Error: Could not detect", result.StdErr);
                    Assert.DoesNotContain("does not exist", result.StdErr);
                },
                result.GetDebugInfo());
        }

        [Fact(Skip = "Temporarily skipping test"), Trait("category", "latest")]
        public void BuildImage_CanExec_WithNoUsableToolsDetected()
        {
            // Arrange
            var appPath = "/tmp";
            var cmd = "node --version";
            var script = new ShellScriptBuilder()
                .AddCommand($"oryx exec --debug --src {appPath} '{cmd}'") // '--debug' prints the resulting script
                .ToString();

            // Act
            var result = _dockerCli.Run(Settings.BuildImageName, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    // Actual output from `node --version` starts with a 'v'
                    Assert.Contains($"v{NodeConstants.NodeLtsVersion}", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact(Skip = "Temporarily skipping test as it is failing to retrieve the sas-token for the staging environment"), Trait("category", "latest")]
        public void BuildImage_CanExec_SingleCommand()
        {
            // Arrange
            var appPath = "/tmp";
            var cmd = "node --version";
            var expectedBashPath = FilePaths.Bash;
            var script = new ShellScriptBuilder()
                .CreateFile($"{appPath}/{NodeConstants.PackageJsonFileName}", "{}")
                .AddCommand($"oryx exec --debug --src {appPath} '{cmd}'") // '--debug' prints the resulting script
                .ToString();

            // Act
            var result = _dockerCli.Run(Settings.BuildImageName, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.Contains("#!" + expectedBashPath, result.StdOut);
                    Assert.Contains($"{NodeConstants.PlatformName}={FinalStretchVersions.FinalStretchNode16Version}", result.StdOut);
                    Assert.True(result.IsSuccess);
                    // Actual output from `node --version` starts with a 'v'
                    Assert.Contains($"v{FinalStretchVersions.FinalStretchNode16Version}", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact(Skip="Temporarily skipping the test"), Trait("category", "latest")]
        public void BuildImage_CanExec_CommandInSourceDir()
        {
            // Arrange
            var appPath = "/tmp";
            var repoScriptPath = "userScript.sh";
            var absScriptPath = $"{appPath}/{repoScriptPath}";

            var script = new ShellScriptBuilder()
                .CreateFile($"{appPath}/{NodeConstants.PackageJsonFileName}", "{}")
                .CreateFile(absScriptPath, "node --version")
                .SetExecutePermissionOnFile(absScriptPath)
                .AddCommand($"oryx exec --debug --src {appPath} ./{repoScriptPath}")
                .ToString();

            // Act
            var result = _dockerCli.Run(Settings.BuildImageName, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    // Actual output from `node --version` starts with a 'v'
                    Assert.Contains($"v{FinalStretchVersions.FinalStretchNode14Version}", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact(Skip = "Temporarily skipping test as it is failing to retrieve the sas-token for the staging environment"), Trait("category", "latest")]
        public void BuildImage_CanExec_MultipleCommands_WithOlderToolVersions()
        {
            // Arrange
            var appPath = "/tmp";
            var cmd = "node --version && php --version";

            var expectedNodeVersion = NodeVersions.Node8Version;
            var expectedPhpVersion = PhpVersions.Php72Version;

            var script = new ShellScriptBuilder()
                .CreateFile($"{appPath}/{NodeConstants.PackageJsonFileName}",
                    "'{\"engines\": {\"node\": \"" + expectedNodeVersion + "\"}}'")
                .CreateFile($"{appPath}/{PhpConstants.ComposerFileName}",
                    "'{\"require\": {\"php\": \"" + expectedPhpVersion + "\"}}'")
                .AddCommand($"oryx exec --debug --src {appPath} '{cmd}'") // '--debug' prints the resulting script
                .ToString();

            // Act
            var result = _dockerCli.Run(Settings.BuildImageName, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.Contains(
                        $"{NodeConstants.PlatformName}={expectedNodeVersion} " +
                        $"{PhpConstants.PlatformName}={expectedPhpVersion}",
                        result.StdOut);
                    Assert.True(result.IsSuccess);
                    // Actual output from `node --version` starts with a 'v'
                    Assert.Contains($"v{expectedNodeVersion}", result.StdOut);
                    // Actual output from `php --version`
                    Assert.Contains($"PHP {expectedPhpVersion}", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact(Skip = "Temporarily skipping test as it is failing to retrieve the sas-token for the staging environment"), Trait("category", "latest")]
        public void BuildImage_Exec_PropagatesFailures()
        {
            // Arrange
            var appPath = "/tmp";
            var expectedExitCode = 123;
            var cmd = $"exit {expectedExitCode}";

            var script = new ShellScriptBuilder()
                .CreateFile($"{appPath}/{NodeConstants.PackageJsonFileName}", "{}")
                .AddCommand($"oryx exec --debug --src {appPath} '{cmd}'")
                .ToString();

            // Act
            var result = _dockerCli.Run(Settings.BuildImageName, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.False(result.IsSuccess);
                    Assert.Equal(expectedExitCode, result.ExitCode);
                },
                result.GetDebugInfo());
        }

        [Fact(Skip = "Temorarily skipping the test")]
        public void CliImage_Dockerfile_SucceedsWithBasicNodeApp()
        {
            // Arrange
            var appPath = "/tmp";
            var platformName = "nodejs";
            var runtimeName = ConvertToRuntimeName(platformName);
            var platformVersion = "10.17";
            var runtimeTag = "10";
            var repositoryName = "build";
            var tagName = ImageTestHelperConstants.LtsVersionsStretch;
            var script = new ShellScriptBuilder()
                .CreateFile($"{appPath}/{NodeConstants.PackageJsonFileName}", "{}")
                .AddCommand($"oryx dockerfile {appPath} --platform {platformName} --platform-version {platformVersion}")
                .ToString();

            // Act
            var result = _dockerCli.Run(_imageHelper.GetCliImage(), "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains($"{runtimeName}:{runtimeTag}", result.StdOut);
                    Assert.Contains($"{repositoryName}:{tagName}", result.StdOut);
                },
                result.GetDebugInfo());
        }

        private string ConvertToRuntimeName(string platformName)
        {
            if (string.Equals(platformName, DotNetCoreConstants.PlatformName, StringComparison.OrdinalIgnoreCase))
            {
                platformName = "dotnetcore";
            }

            if (string.Equals(platformName, NodeConstants.PlatformName, StringComparison.OrdinalIgnoreCase))
            {
                platformName = "node";
            }

            return platformName;
        }

    }
}
