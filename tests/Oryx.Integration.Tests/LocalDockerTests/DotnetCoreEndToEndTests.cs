// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.DotnetCore;
using Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.Integration.Tests.LocalDockerTests
{
    public class DotnetCoreEndToEndTests
    {
        private const int HostPort = 8081;
        private const string startupFilePath = "/tmp/startup.sh";
        private const string NetCoreApp11WebApp = "NetCoreApp11WebApp";
        private const string NetCoreApp21WebApp = "NetCoreApp21WebApp";
        private const string NetCoreApp22WebApp = "NetCoreApp22WebApp";

        private readonly ITestOutputHelper _output;
        private readonly string _hostSamplesDir;
        private readonly HttpClient _httpClient;

        public DotnetCoreEndToEndTests(ITestOutputHelper output)
        {
            _output = output;
            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
            _httpClient = new HttpClient();
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore11WebApp()
        {
            await BuildRunAndAssertAsync(NetCoreApp11WebApp, languageVersion: "1.1");
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore11WebApp_HavingExplicitAssemblyName()
        {
            await BuildRunAndAssertAsync("NetCoreApp11WithExplicitAssemblyName", languageVersion: "1.1");
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore21WebApp()
        {
            await BuildRunAndAssertAsync(NetCoreApp21WebApp, languageVersion: "2.1");
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore21WebApp_HavingExplicitAssemblyName()
        {
            await BuildRunAndAssertAsync("NetCoreApp21WithExplicitAssemblyName", languageVersion: "2.1");
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore22WebApp()
        {
            await BuildRunAndAssertAsync(NetCoreApp22WebApp, languageVersion: "2.2");
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore22WebApp_HavingExplicitAssemblyName()
        {
            await BuildRunAndAssertAsync("NetCoreApp22WithExplicitAssemblyName", languageVersion: "2.2");
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore11WebApp_UsingSdkInGlobalJson()
        {
            await BuildRunAndAssertAsync(
                NetCoreApp11WebApp,
                languageVersion: "1.1",
                DotnetCoreConstants.DotnetCoreSdkVersion11);
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore21WebApp_UsingSdkInGlobalJson()
        {
            await BuildRunAndAssertAsync(
                NetCoreApp21WebApp,
                languageVersion: "2.1",
                DotnetCoreConstants.DotnetCoreSdkVersion21);
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore22WebApp_UsingSdkInGlobalJson()
        {
            await BuildRunAndAssertAsync(
                NetCoreApp22WebApp,
                languageVersion: "2.2",
                DotnetCoreConstants.DotnetCoreSdkVersion22);
        }

        [Fact]
        public async Task CanBuildAndRun_NetCore21WebApp_UsingExplicitStartupCommand()
        {
            await BuildRunAndAssertAsync(
                NetCoreApp21WebApp,
                languageVersion: "2.1",
                globalJsonSdkVersion: null,
                startupCommand: $"dotnet {NetCoreApp21WebApp}.dll",
                publishToDifferentOutputDirectory: false);
        }

        private Task BuildRunAndAssertAsync(string sampleName, string languageVersion)
        {
            return BuildRunAndAssertAsync(
                sampleName,
                languageVersion,
                globalJsonSdkVersion: null,
                startupCommand: null,
                publishToDifferentOutputDirectory: false);
        }

        private Task BuildRunAndAssertAsync(
            string sampleName,
            string languageVersion,
            string globalJsonSdkVersion)
        {
            return BuildRunAndAssertAsync(
                sampleName,
                languageVersion,
                globalJsonSdkVersion,
                startupCommand: null,
                publishToDifferentOutputDirectory: false);
        }

        private async Task BuildRunAndAssertAsync(
            string sampleName,
            string languageVersion,
            string globalJsonSdkVersion,
            string startupCommand,
            bool publishToDifferentOutputDirectory)
        {
            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "DotNetCore", sampleName);
            var volume = DockerVolume.Create(hostDir);

            if (!string.IsNullOrEmpty(globalJsonSdkVersion))
            {
                var globalJsonFile = Path.Combine(volume.MountedHostDir, "global.json");
                if (File.Exists(globalJsonFile))
                {
                    throw new InvalidOperationException($"Global json file already exists at '{globalJsonFile}'.");
                }

                File.WriteAllText(globalJsonFile, "{\"sdk\":{\"version\":\"" + globalJsonSdkVersion + "\"}}");
            }

            var appDir = volume.ContainerDir;
            string appOutputDir = null;
            if (publishToDifferentOutputDirectory)
            {
                appOutputDir = $"/tmp/{appDir}-output";
            }

            var command = $"oryx -sourcePath {appDir} -output {startupFilePath}";
            if (!string.IsNullOrEmpty(startupCommand))
            {
                command += $" -userStartupCommand \"{startupCommand}\"";
            }

            var portMapping = $"{HostPort}:9095";
            var script = new ShellScriptBuilder()
                .AddCommand(command)
                .AddCommand("export ASPNETCORE_URLS=http://*:9095")
                .AddCommand(startupFilePath)
                .ToString();

            var buildArgs = new List<string>();
            buildArgs.Add("build");
            buildArgs.Add(appDir);
            if (!string.IsNullOrEmpty(appOutputDir))
            {
                buildArgs.Add("-o");
                buildArgs.Add(appOutputDir);
            }
            buildArgs.Add("-l");
            buildArgs.Add("dotnet");

            if (!string.IsNullOrEmpty(languageVersion))
            {
                buildArgs.Add("--language-version");
                buildArgs.Add(languageVersion);
            }

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
            _output,
            volume,
            "oryx",
            buildArgs.ToArray(),
            $"oryxdevms/dotnetcore-{languageVersion}",
            portMapping,
            "/bin/sh",
            new[]
            {
                "-c",
                script
            },
            async () =>
            {
                var data = await _httpClient.GetStringAsync($"http://localhost:{HostPort}/");
                Assert.Contains("Hello World!", data);
            });
        }
    }
}