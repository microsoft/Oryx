// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using JetBrains.Annotations;
using Microsoft.Oryx.BuildScriptGenerator.Common;
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

        public static IEnumerable<object[]> SomeVSCodeDependencies => new object[][]
        {
            new object[] { "applicationinsights", "1.0.8",
                "git://github.com/microsoft/ApplicationInsights-node.js.git" },
            new object[] { "iconv-lite", "0.5.0",
                "git://github.com/ashtuchkin/iconv-lite.git" },
            new object[] { "keytar", "4.11.0",
                "git://github.com/atom/node-keytar.git", new[] { "libsecret-1-dev" } },
            new object[] { "native-keymap", "2.0.0",
                "git://github.com/Microsoft/node-native-keymap.git", new[] { "libx11-dev", "libxkbfile-dev" } },
        };

        private readonly string[] IgnoredTarEntries = new[] { "package/.npmignore", "package", "package/yarn.lock" };

        [Theory(Skip = "Bug# 1361701: In agent this test is failing"), Trait("category", "latest")]
        [MemberData(nameof(SomeVSCodeDependencies))]
        public void CanBuildNpmPackages(
            string pkgName,
            string pkgVersion,
            string gitRepoUrl,
            string[] requiredOsPackages = null)
        {
            const string tarListCmd = "tar -tvf";
            const string npmTarPath = "/tmp/npm-pkg.tgz";
            const string tarListMarker = "---TAR---";

            // Arrange
            var pkgSrcDir = "/tmp/pkg/src";
            var pkgBuildOutputDir = "/tmp/pkg/out";
            var oryxPackOutput = $"{pkgBuildOutputDir}/{pkgName}-{pkgVersion}.tgz";

            string commitId = GetGitHeadFromNpmRegistry(pkgName, pkgVersion);
            Assert.NotNull(commitId);

            var osReqsParam = string.Empty;
            if (requiredOsPackages != null)
            {
                osReqsParam = $"--os-requirements {string.Join(',', requiredOsPackages)}";
            }

            var script = new ShellScriptBuilder()
            // Fetch source code
                .AddCommand($"mkdir -p {pkgSrcDir} && git clone {gitRepoUrl} {pkgSrcDir}")
                .AddCommand($"cd {pkgSrcDir} && git checkout {commitId}")
                // Make sure python2 is on the path as node-gyp install of iconv fails otherwise
                .AddCommand("source benv python=2")
            // Build & package
                .AddBuildCommand($"{pkgSrcDir} --package --manifest-dir /tmp/temp -o {pkgBuildOutputDir} {osReqsParam}") // Should create a file <name>-<version>.tgz
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
            // Not using Settings.BuildImageName on purpose - so that apt-get can run as root
            var image = _imageHelper.GetBuildImage();
            var result = _dockerCli.Run(image, "/bin/bash", new[] { "-c", script });

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
