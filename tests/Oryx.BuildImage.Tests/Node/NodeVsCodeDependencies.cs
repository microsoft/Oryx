// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public void CanBuildNpmPackages(string pkgName, string pkgVersion, string gitRepoUrl, string commitId)
        {
            // Arrange
            var pkgSrcDir = "/tmp/pkg/src";
            var pkgBuildOutputDir = "/tmp/pkg/out";
            var oryxPackOutput = $"{pkgBuildOutputDir}/{pkgName}-{pkgVersion}.tgz";

            const string tarListMarker = "------";

            var script = new ShellScriptBuilder()
                // Fetch source code
                    .AddCommand($"mkdir -p {pkgSrcDir} && git clone {gitRepoUrl} {pkgSrcDir}")
                    .AddCommand($"cd {pkgSrcDir} && git checkout {commitId}")
                // Build & package
                    .AddBuildCommand($"{pkgSrcDir} --package -o {pkgBuildOutputDir}") // Should create a file <name>-<version>.tgz
                    .AddFileExistsCheck(oryxPackOutput)
                // Compute diff between tar contents
                    // Download public NPM build for comparison
                        .AddCommand($"export NpmTarUrl=$(npm view {pkgName}@{pkgVersion} dist.tarball)")
                        .AddCommand("wget -O /tmp/npm-pkg.tgz $NpmTarUrl")
                    .AddCommand("echo " + tarListMarker)
                    .AddCommand($"tar -tf {oryxPackOutput}")
                    .AddCommand("echo " + tarListMarker)
                    .AddCommand("tar -tf /tmp/npm-pkg.tgz")
                .ToString();

            // Act
            var result = _dockerCli.Run(Settings.BuildImageName, "/bin/bash", new[] { "-c", script });

            // Assert
            var tarLists = result.StdOut.Split(tarListMarker);

            var oryxTarList = SplitLines(tarLists[1]).OrderBy(s => s);
            var npmTarList  = SplitLines(tarLists[2]).OrderBy(s => s);
            Assert.Equal(oryxTarList, npmTarList);
        }

        private string[] SplitLines(string str)
        {
            return str.Trim().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }
    }
}
