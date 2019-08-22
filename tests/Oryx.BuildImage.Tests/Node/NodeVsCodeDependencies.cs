// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests.Node
{
    /// <summary>
    /// Tests that the Node platform can build and package the VS Code dependencies.
    /// </summary>
    public class NodeVsCodeDependencies : NodeJSSampleAppsTestBase
    {
        public NodeVsCodeDependencies(ITestOutputHelper output) : base(output)
        {
        }

        public static IEnumerable<object[]> VSCodeDependencies => new object[][]
        {
            new object[] { "applicationinsights", "1.0.8", "https://github.com/microsoft/ApplicationInsights-node.js.git",
                "bf8dee921aeaf2ab5461d21ca090b01d1fd1d715" },
            new object[] { "graceful-fs", "4.1.11", "https://github.com/isaacs/node-graceful-fs.git",
                "65cf80d1fd3413b823c16c626c1e7c326452bee5" },
        };

        [Theory]
        [MemberData(nameof(VSCodeDependencies))]
        public async Task CanBuildNpmPackages(string pkgName, string pkgVersion, string gitRepoUrl, string commitId)
        {
            // Arrange
            var pkgSrcDir = "/tmp/pkg/src";
            var pkgBuildOutputDir = "/tmp/pkg/out";
            var oryxPackOutput = Path.Combine(pkgBuildOutputDir, $"{pkgName}-{pkgVersion}.tgz");

            const string diffSentinel = "--- Diff: ---";

            var script = new ShellScriptBuilder()
                // Fetch source code
                    .AddCommand($"mkdir -p {pkgSrcDir} && git clone {gitRepoUrl} {pkgSrcDir}")
                    .AddCommand($"cd {pkgSrcDir} && git checkout {commitId}")
                // Build & package
                    .AddBuildCommand($"{pkgSrcDir} --package -o {pkgBuildOutputDir}") // Should create a file <name>-<version>.tgz
                    .AddFileExistsCheck(oryxPackOutput)
                // Compute diff between tar contents
                    .AddCommand($"tar -tf {oryxPackOutput} > /tmp/contents.oryx.txt")
                    // Download public NPM build for comparison
                        .AddCommand($"export NpmTarUrl=$(npm view {pkgName}@{pkgVersion} dist.tarball)")
                        .AddCommand("wget -O /tmp/npm-pkg.tgz $NpmTarUrl")
                    .AddCommand("tar -tf /tmp/npm-pkg.tgz > /tmp/contents.npm.txt")
                    .AddCommand("echo " + diffSentinel)
                    .AddCommand("diff /tmp/contents.oryx.txt /tmp/contents.npm.txt")
                .ToString();

            // Act
            var result = _dockerCli.Run(Settings.BuildImageName, "/bin/bash", new[] { "-c", script });

            // Assert
            var tarDiff = result.StdOut.Split(diffSentinel)[1];
        }
    }
}
