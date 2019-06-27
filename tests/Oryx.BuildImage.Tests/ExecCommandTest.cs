// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.BuildScriptGeneratorCli;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class ExecCommandTest : SampleAppsTestBase
    {
        public ExecCommandTest(ITestOutputHelper output) : base(output) { }

        [Theory]
        [InlineData(null)]
        [InlineData("/bin/dash")]
        public void Exec_NodeVersion(string altBashPath)
        {
            // Arrange
            var appPath = "/tmp/app";
            var cmd = "node --version && npm --version";

            var scriptBuilder = new ShellScriptBuilder()
                .AddCommand($"mkdir {appPath}")
                .CreateFile($"{appPath}/package.json", "{}");

            var expectedBashPath = FilePaths.Bash;
            if (!string.IsNullOrEmpty(altBashPath))
            {
                scriptBuilder.SetEnvironmentVariable("BASH", altBashPath);
                expectedBashPath = altBashPath;
            }

            var script = scriptBuilder
                .AddCommand($"oryx exec {appPath} '{cmd}' --debug") // '--debug' makes sure the benv command is printed
                .ToString();

            // Act
            var result = _dockerCli.Run(Settings.BuildImageName, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.Contains($"{expectedBashPath} -c '{FilePaths.Benv} node={NodeConstants.NodeLtsVersion} {cmd}'", result.StdOut);
                    Assert.True(result.IsSuccess);
                    Assert.Contains($"v{NodeConstants.NodeLtsVersion}", result.StdOut); // Actual output from 'node --version'
                },
                result.GetDebugInfo());
        }
    }
}
