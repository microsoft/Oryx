// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
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
        public void Exec_NodeLtsVersion(string altBashPath)
        {
            // Arrange
            var appPath = "/tmp/app";
            var cmd = "node --version";

            var scriptBuilder = new ShellScriptBuilder()
                .AddCommand($"mkdir {appPath}")
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
                    Assert.Contains($"{expectedBashPath} -c '{FilePaths.Benv} {nodeArg} {cmd}'", result.StdOut);
                    Assert.True(result.IsSuccess);
                    // Actual output from 'node --version' starts with a 'v'
                    Assert.Contains($"v{NodeConstants.NodeLtsVersion}", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Exec_NodeVersionAndPhpVersion()
        {
            // Arrange
            var appPath = "/tmp/app";
            var cmd = "node --version && php --version";

            var scriptBuilder = new ShellScriptBuilder()
                .AddCommand($"mkdir {appPath}")
                .CreateFile($"{appPath}/{NodeConstants.PackageJsonFileName}", "{}")
                .CreateFile($"{appPath}/{PhpConstants.ComposerFileName}", "{}");

            var script = scriptBuilder
                .AddCommand($"oryx exec --debug --src {appPath} '{cmd}'") // '--debug' prints the benv command
                .ToString();

            // Act
            var result = _dockerCli.Run(Settings.BuildImageName, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    var benvArgs = $"node={NodeConstants.NodeLtsVersion} php={PhpConstants.DefaultPhpRuntimeVersion}";
                    Assert.Contains($"{FilePaths.Bash} -c '{FilePaths.Benv} {benvArgs} {cmd}'", result.StdOut);
                    Assert.True(result.IsSuccess);
                    // Actual output from 'node --version' starts with a 'v'
                    Assert.Contains($"v{NodeConstants.NodeLtsVersion}", result.StdOut); // Actual output from 'node --version'
                    Assert.Contains($"php{NodeConstants.NodeLtsVersion}", result.StdOut); // Actual output from '`php --version'
                },
                result.GetDebugInfo());
        }
    }
}
