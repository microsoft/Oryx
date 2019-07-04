// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Xunit;
using Xunit.Abstractions;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.BuildScriptGenerator.Php;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class ExecCommandTest : SampleAppsTestBase
    {
        public ExecCommandTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void CanExec_SingleCommand()
        {
            // Arrange
            var appPath = "/tmp";
            var cmd = "node --version";
            var expectedBashPath = FilePaths.Bash;
            var script = new ShellScriptBuilder()
                .CreateFile($"{appPath}/{NodeConstants.PackageJsonFileName}", "{}")
                .AddCommand($"oryx exec --debug --src {appPath} '{cmd}'") // '--debug' prints the benv command
                .ToString();

            // Act
            var result = _dockerCli.Run(Settings.BuildImageName, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    var nodeArg = $"node={NodeConstants.NodeLtsVersion}";
                    Assert.Contains("#!" + expectedBashPath, result.StdOut);
                    Assert.Contains(nodeArg, result.StdOut);
                    Assert.True(result.IsSuccess);
                    // Actual output from `node --version` starts with a 'v'
                    Assert.Contains($"v{NodeConstants.NodeLtsVersion}", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void CanExec_CommandInSourceDir()
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
                    Assert.Contains($"v{NodeConstants.NodeLtsVersion}", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void CanExec_MultipleCommands_WithOlderToolVersions()
        {
            // Arrange
            var appPath = "/tmp";
            var cmd = "node --version && php --version";

            var expectedNodeVersion = NodeVersions.Node8Version;
            var expectedPhpVersion  = PhpVersions.Php72Version;

            var script = new ShellScriptBuilder()
                .CreateFile($"{appPath}/{NodeConstants.PackageJsonFileName}",
                    "'{\"engines\": {\"node\": \"" + expectedNodeVersion + "\"}}'")
                .CreateFile($"{appPath}/{PhpConstants.ComposerFileName}",
                    "'{\"require\": {\"php\": \"" + expectedPhpVersion + "\"}}'")
                .AddCommand($"oryx exec --debug --src {appPath} '{cmd}'") // '--debug' prints the benv command
                .ToString();

            // Act
            var result = _dockerCli.Run(Settings.BuildImageName, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.Contains($"node={expectedNodeVersion} php={expectedPhpVersion}", result.StdOut);
                    Assert.True(result.IsSuccess);
                    // Actual output from `node --version` starts with a 'v'
                    Assert.Contains($"v{expectedNodeVersion}", result.StdOut);
                    // Actual output from `php --version`
                    Assert.Contains($"PHP {expectedPhpVersion}", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Exec_PropagatesFailures()
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
    }
}
