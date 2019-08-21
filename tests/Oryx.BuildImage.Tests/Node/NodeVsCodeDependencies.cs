// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Oryx.BuildImage.Tests.Node
{
    /// <summary>
    /// Tests that the Node platform can build and package the VS Code dependencies.
    /// </summary>
    public class NodeVsCodeDependencies : NodeJSSampleAppsTestBase
    {

        public static IEnumerable<object[]> VSCodeDependencies => new object[][]
        {
            new object[] { "applicationinsights", "1.0.8", "https://github.com/Microsoft/ApplicationInsights-node.js",
                "bf8dee921aeaf2ab5461d21ca090b01d1fd1d715" },
            new object[] { "graceful-fs", "4.1.11", "https://github.com/isaacs/node-graceful-fs",
                "65cf80d1fd3413b823c16c626c1e7c326452bee5" },
        };

        [Theory]
        [MemberData(nameof(VSCodeDependencies))]
        public async Task CanBuildAndPackage(string pkgName, string pkgVersion, string gitRepoUrl, string commitId)
        {
            // Arrange
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });
        }
}
