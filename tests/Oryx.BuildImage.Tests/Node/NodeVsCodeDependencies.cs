// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using JetBrains.Annotations;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests.Node
{
    /// <summary>
    /// Tests that the Node platform can build and package the VS Code dependencies.
    /// </summary>
    public class NodeVsCodeDependencies : NodeJSSampleAppsTestBase
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public NodeVsCodeDependencies(ITestOutputHelper output) : base(output)
        {
        }

        public static IEnumerable<object[]> VSCodeDependencies => new object[][]
        {
            new object[] { "applicationinsights", "1.0.8", "git://github.com/microsoft/ApplicationInsights-node.js.git",
                "bf8dee921aeaf2ab5461d21ca090b01d1fd1d715" },
            new object[] { "graceful-fs", "4.1.11", "git://github.com/isaacs/node-graceful-fs.git",
                "65cf80d1fd3413b823c16c626c1e7c326452bee5" },
            new object[] { "http-proxy-agent", "2.1.0", "git://github.com/TooTallNate/node-http-proxy-agent.git",
                "65307ac8fe4e6ce1a2685d21ec4affa4c2a0a30d" },
            new object[] { "https-proxy-agent", "2.2.1", "git://github.com/TooTallNate/node-https-proxy-agent.git",
                "8c3a75baddecae7e2fe2921d1adde7edd0203156" },
            new object[] { "iconv-lite", "0.5.0", "git://github.com/ashtuchkin/iconv-lite.git",
                "2b4125d11a733a40e45a755648389b2512a97a62" },
            new object[] { "jschardet", "1.6.0", "git://github.com/aadsm/jschardet.git",
                "28152dd8db5904dc2cf9aa12ef4f8783f713e79a" },
            new object[] { "keytar", "4.11.0", "git://github.com/atom/node-keytar.git",
                "8467f748fcd8115f8814f59a13990e7d9ac0502c" },
            new object[] { "native-is-elevated", "0.3.0", "git://github.com/arkon/native-is-elevated.git",
                "10f66a30a5b62522e2220982af56fddde098b110" },
            new object[] { "native-keymap", "2.0.0", "git://github.com/Microsoft/node-native-keymap.git",
                "09c4398591f720809aec60c089d52a80ad600e75" },
            new object[] { "native-watchdog", "1.0.0", "git://github.com/Microsoft/node-native-watchdog.git",
                "277683b01e7c0933f51c322a76749f91f34143a8" },
            new object[] { "node-pty", "0.9.0-beta19", "git://github.com/microsoft/node-pty.git",
                "32ea3e47791794a82c58c76bf83dc4d441a93108" },
            new object[] { "nsfw", "1.2.5", "git://github.com/Axosoft/nsfw.git",
                "636dc8c9424a81310e609eaecbdd113640bd822e" },
            new object[] { "onigasm-umd", "2.2.2", "git://github.com/alexandrudima/onigasm-umd.git",
                "e1d4f7142c4bfe9336924428a218fde2e665fdd6" },
            new object[] { "semver-umd", "5.5.3", "git://github.com/Microsoft/semver-umd.git",
                "7b204c2a62206cfbd916d911f3cf876f6a9e437f" },
            new object[] { "spdlog", "0.9.0", "git://github.com/Microsoft/node-spdlog.git",
                "52a0510087667a18cc3524450ae946183f6a4c7d" },
            new object[] { "sudo-prompt", "9.0.0", "git://github.com/jorangreef/sudo-prompt.git",
                "3bfa62163b59e45111436c695ee8e7e2befbe310" },
            new object[] { "v8-inspect-profiler", "0.0.20", "git://github.com/jrieken/v8-inspect-profiler.git",
                "71c714d4ee24af26828ba7c75928cf4493c7a255" },
            new object[] { "vscode-chokidar", "2.1.7", "git://github.com/paulmillr/chokidar.git",
                "f53aaac0472c0f464fdb8031cb27e91c759c2ac5" },
            new object[] { "vscode-minimist", "1.2.1", "git://github.com/substack/minimist.git",
                "39bc39518f81b69a5deab422e0aac656ef653318" },
            new object[] { "vscode-proxy-agent", "0.4.0", "git://github.com/Microsoft/vscode-proxy-agent.git",
                "2b3ed03ae8621271008f49d32f23cba84a1cf89b" },
            new object[] { "vscode-ripgrep", "1.5.6", "git://github.com/microsoft/vscode-ripgrep.git",
                "50354992f3c2c13436e5fbe600661fb739a4c43c" },
            new object[] { "vscode-sqlite3", "4.0.8", "git://github.com/mapbox/node-sqlite3.git" },
            new object[] { "vscode-textmate", "4.2.2", "git://github.com/microsoft/vscode-textmate.git",
                "afd20a74ab3e6edc8b7a9f031960667e06bc6d8c" },
            new object[] { "xterm", "3.15.0-beta101", "git://github.com/xtermjs/xterm.js.git",
                "211700518fcf9a3b767b34f9e57d0c101f9051bb" },
            new object[] { "yauzl", "2.9.2", "git://github.com/thejoshwolfe/yauzl.git" },
            new object[] { "yazl", "2.4.3", "git://github.com/thejoshwolfe/yazl.git" },
            /*
            "xterm-addon-search": "0.2.0-beta5",
            "xterm-addon-web-links": "0.1.0-beta10",
            */
        };

        private readonly string[] IgnoredTarEntries = new[] { "package/.npmignore", "package", "package/yarn.lock" };

        [Theory]
        [MemberData(nameof(VSCodeDependencies))]
        public void CanBuildNpmPackages(string pkgName, string pkgVersion, string gitRepoUrl, string altCommitId = null)
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

            const string tarListCmd = "tar -tvf";
            const string npmTarPath = "/tmp/npm-pkg.tgz";
            const string tarListMarker = "---TAR---";

            string commitId = GetGitHeadFromNpmRegistry(pkgName, pkgVersion) ?? altCommitId;

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
                    .AddCommand($"{tarListCmd} {oryxPackOutput}")
                    .AddCommand("echo " + tarListMarker)
                    .AddCommand($"{tarListCmd} {npmTarPath}")
                .ToString();

            // Act
            var result = _dockerCli.Run(Settings.BuildImageName, "/bin/bash", new[] { "-c", script });

            // Assert contained file names
            var tarLists = result.StdOut.Split(tarListMarker);

            var (oryxTarList, oryxTarSize) = ParseTarList(tarLists[1]);
            var (npmTarList,  npmTarSize)  = ParseTarList(tarLists[2]);
            Assert.Equal(npmTarList, oryxTarList);

            // Assert tar file sizes
            var tarSizeDiff = Math.Abs(npmTarSize - oryxTarSize);
            Assert.True(tarSizeDiff < npmTarSize * 0.1, // Accepting differences of less than 10% of the official artifact size
                $"Size difference is too big. Oryx build: {oryxTarSize}, NPM build: {npmTarSize}");
        }

        private (IEnumerable<string>, int) ParseTarList(string rawTarList)
        {
            var fileEntries = rawTarList.Trim().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Where(line => !line.StartsWith('d')) // Filter out directories
                .Select(line => line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries))
                .Select(cols => (Size: int.Parse(cols[2]), Name: cols.Last())) // Keep only the size and the name
                .Where(entry => !IgnoredTarEntries.Contains(entry.Name))
                .OrderBy(entry => entry.Name);

            return (fileEntries.Select(entry => entry.Name), fileEntries.Sum(entry => entry.Size));
        }

        [CanBeNull]
        private string GetGitHeadFromNpmRegistry(string name, string version)
        {
            var packageJson = JsonConvert.DeserializeObject<dynamic>(
                _httpClient.GetStringAsync($"http://registry.npmjs.org/{name}/{version}").Result);
            var gitHeadNode = packageJson?.gitHead as Newtonsoft.Json.Linq.JValue;
            return gitHeadNode?.ToString();
        }
    }
}
