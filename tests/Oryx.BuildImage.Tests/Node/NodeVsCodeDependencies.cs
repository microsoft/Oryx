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
            new object[] { "http-proxy-agent", "2.1.0", "https://github.com/TooTallNate/node-http-proxy-agent.git",
                "65307ac8fe4e6ce1a2685d21ec4affa4c2a0a30d" },
            new object[] { "https-proxy-agent", "2.2.1", "https://github.com/TooTallNate/node-https-proxy-agent.git",
                "8c3a75baddecae7e2fe2921d1adde7edd0203156" },
            new object[] { "iconv-lite", "0.5.0", "https://github.com/ashtuchkin/iconv-lite.git",
                "2b4125d11a733a40e45a755648389b2512a97a62" },
            new object[] { "jschardet", "1.6.0", "https://github.com/aadsm/jschardet.git",
                "28152dd8db5904dc2cf9aa12ef4f8783f713e79a" },
            new object[] { "keytar", "4.11.0", "https://github.com/atom/node-keytar.git",
                "8467f748fcd8115f8814f59a13990e7d9ac0502c" },
            new object[] { "native-is-elevated", "0.3.0", "https://github.com/arkon/native-is-elevated.git",
                "10f66a30a5b62522e2220982af56fddde098b110" },
            new object[] { "native-keymap", "2.0.0", "https://github.com/Microsoft/node-native-keymap.git",
                "09c4398591f720809aec60c089d52a80ad600e75" },
            new object[] { "native-watchdog", "1.0.0", "https://github.com/Microsoft/node-native-watchdog.git",
                "277683b01e7c0933f51c322a76749f91f34143a8" },
            new object[] { "node-pty", "0.9.0-beta19", "https://github.com/microsoft/node-pty.git",
                "32ea3e47791794a82c58c76bf83dc4d441a93108" },
            new object[] { "nsfw", "1.2.5", "https://github.com/Axosoft/nsfw.git",
                "636dc8c9424a81310e609eaecbdd113640bd822e" },
            new object[] { "onigasm-umd", "2.2.2", "https://github.com/alexandrudima/onigasm-umd.git",
                "e1d4f7142c4bfe9336924428a218fde2e665fdd6" },
            new object[] { "semver-umd", "5.5.3", "https://github.com/Microsoft/semver-umd.git",
                "7b204c2a62206cfbd916d911f3cf876f6a9e437f" },
            /*
            "onigasm-umd": "^2.2.2",
            "semver-umd": "^5.5.3",
            "spdlog": "^0.9.0",
            "sudo-prompt": "9.0.0",
            "v8-inspect-profiler": "^0.0.20",
            "vscode-chokidar": "2.1.7",
            "vscode-minimist": "^1.2.1",
            "vscode-proxy-agent": "0.4.0",
            "vscode-ripgrep": "^1.5.6",
            "vscode-sqlite3": "4.0.8",
            "vscode-textmate": "^4.2.2",
            "xterm": "3.15.0-beta101",
            "xterm-addon-search": "0.2.0-beta5",
            "xterm-addon-web-links": "0.1.0-beta10",
            "yauzl": "^2.9.2",
            "yazl": "^2.4.3"
            */
        };

        [Theory]
        [MemberData(nameof(VSCodeDependencies))]
        public void CanBuildNpmPackages(string pkgName, string pkgVersion, string gitRepoUrl, string commitId)
        {
            // Arrange
            var pkgSrcDir = "/tmp/pkg/src";
            var pkgBuildOutputDir = "/tmp/pkg/out";
            var oryxPackOutput = $"{pkgBuildOutputDir}/{pkgName}-{pkgVersion}.tgz";

            // HACK: node-pty has a different version string in its `package.json`
            if (pkgName == "node-pty")
            {
                oryxPackOutput = $"{pkgBuildOutputDir}/{pkgName}-0.8.1.tgz";
            }

            const string npmTarPath = "/tmp/npm-pkg.tgz";
            const string tarListMarker = "---TAR---";
            const string sizeMarker = "---SIZE---";

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
                    .AddCommand($"wget -O {npmTarPath} $NpmTarUrl")
                // Print tar content lists
                    .AddCommand("echo " + tarListMarker)
                    .AddCommand($"tar -tf {oryxPackOutput}")
                    .AddCommand("echo " + tarListMarker)
                    .AddCommand($"tar -tf {npmTarPath}")
                    .AddCommand("echo " + tarListMarker)
                // Print tar sizes
                    .AddCommand("echo " + sizeMarker)
                    .AddCommand($"stat --format %s {oryxPackOutput}")
                    .AddCommand("echo " + sizeMarker)
                    .AddCommand($"stat --format %s {npmTarPath}")
                .ToString();

            // Act
            var result = _dockerCli.Run(Settings.BuildImageName, "/bin/bash", new[] { "-c", script });

            // Assert contained file names
            var tarLists = result.StdOut.Split(tarListMarker);

            var oryxTarList = NormalizeTarList(tarLists[1]);
            var npmTarList  = NormalizeTarList(tarLists[2]);
            Assert.Equal(oryxTarList, npmTarList);

            // Assert tar file sizes
            var tarSizes = result.StdOut.Split(sizeMarker);

            var oryxTarSize = int.Parse(tarSizes[1].Trim());
            var npmTarSize = int.Parse(tarSizes[2].Trim());

            var tarSizeDiff = Math.Abs(oryxTarSize - npmTarSize);
            Assert.True(tarSizeDiff < npmTarSize * 0.1); // Less than 10% of the official build size
        }

        private IEnumerable<string> NormalizeTarList(string rawTarList)
        {
            return rawTarList.Trim().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Where(fname => fname != "package/.npmignore")
                .OrderBy(s => s);
        }
    }
}
