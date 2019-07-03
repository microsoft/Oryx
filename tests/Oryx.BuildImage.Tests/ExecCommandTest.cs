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

        [Theory]
        [InlineData(null)]
        [InlineData("/bin/dash")]
        public void CanExec_SingleCommand(string altBashPath)
        {
            // Arrange
            var appPath = "/tmp";
            var cmd = "node --version";

            var scriptBuilder = new ShellScriptBuilder()
                .CreateFile($"{appPath}/{NodeConstants.PackageJsonFileName}", "{}");

            var expectedBashPath = FilePaths.Bash;
            if (!string.IsNullOrEmpty(altBashPath))
            {
                scriptBuilder.SetEnvironmentVariable("BASH", altBashPath);
                expectedBashPath = altBashPath;
            }

            var script = scriptBuilder
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
        public void CanExec_MultipleCommands()
        {
            // Arrange
            var appPath = "/tmp";
            var cmd = "node --version && php --version";

            var script = new ShellScriptBuilder()
                .CreateFile($"{appPath}/{NodeConstants.PackageJsonFileName}", "{}")
                .CreateFile($"{appPath}/{PhpConstants.ComposerFileName}", "{}")
                .AddCommand($"oryx exec --debug --src {appPath} '{cmd}'") // '--debug' prints the benv command
                .ToString();

            // Act
            var result = _dockerCli.Run(Settings.BuildImageName, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    var benvArgs = $"node={NodeConstants.NodeLtsVersion} php={PhpConstants.DefaultPhpRuntimeVersion}";
                    Assert.Contains(benvArgs, result.StdOut);
                    Assert.True(result.IsSuccess);
                    // Actual output from `node --version` starts with a 'v'
                    Assert.Contains($"v{NodeConstants.NodeLtsVersion}", result.StdOut);
                    // Actual output from `php --version`
                    Assert.Contains($"PHP {PhpConstants.DefaultPhpRuntimeVersion}", result.StdOut);
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
