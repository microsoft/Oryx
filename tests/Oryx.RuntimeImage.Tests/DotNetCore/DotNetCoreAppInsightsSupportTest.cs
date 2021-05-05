// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Tests.Common;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    [Trait("category", "dotnetcore")]
    public class DotNetCoreAppInsightsSupportTest : DotNetCoreRuntimeImageTestBase
    {
        public DotNetCoreAppInsightsSupportTest(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData("6.0", "~2", "Net5MvcApp")]
        [InlineData("5.0", "~3", "Net5MvcApp")]
        [InlineData("6.0", "disabled", "Net5MvcApp")]
        public async Task GeneratesScript_Doesnot_Add_Oryx_AppInsights_Codeless_Configuration(
            string dotNetVersion,
            string agentExtensionVersionEnvValue,
            string appName)

        {
            // This test is for the following scenario:
            // When we find the user has set env variable "ApplicationInsightsAgent_EXTENSION_VERSION" to '~3' 
            // Oryx should not attach appinsight codeless config to runscript

            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "dotnetcore", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var imageName = _imageHelper.GetRuntimeImage("node", dotNetVersion);
            //agentextension version will be set to '~3' or '' or 'disabled'
            var agentExtensionVersionEnv = ExtVarNames.UserAppInsightsAgentExtensionVersion;
            var connectionStringEnv = ExtVarNames.UserAppInsightsConnectionStringEnv;
            int containerDebugPort = 8080;
            var OryxAppInsightsAttachString1 = "export ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=";
            var OryxAppInsightsAttachString2 = "export DOTNET_STARTUP_HOOKS=";
            var testTomlFile =
                @"DotNetCoreRuntimeVersion=""5.0.5""
                  DotNetCoreSdkVersion = ""5.0.202""
                  StartupDllFileName = Net5MvcApp.dll";

            var script = new ShellScriptBuilder()
                .AddCommand($"echo {testTomlFile} > {appDir}")
                .AddCommand($"export {agentExtensionVersionEnv}={agentExtensionVersionEnvValue}")
                .AddCommand($"export {connectionStringEnv}=alkajsldkajd")
                .AddCommand($"oryx create-script -appPath {appDir}")
                .AddCommand($"./run.sh > {appDir}/log.log")
                .AddStringDoesNotExistInFileCheck(OryxAppInsightsAttachString1, $"{appDir}/run.sh")
                .AddStringDoesNotExistInFileCheck(OryxAppInsightsAttachString2, $"{appDir}/run.sh")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("dotnetcore", dotNetVersion),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                port: containerDebugPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", script },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Welcome to ASP.NET Core MVC!", data);
                },
                dockerCli: _dockerCli);
        }

        [Theory]
        [InlineData("6.0", "~3", "NetCore6PreviewMvcApp")]
        public async Task GeneratesScript_Adds_Oryx_AppInsights_Codeless_Configuration(
            string dotNetVersion,
            string agentExtensionVersionEnvValue,
            string appName)

        {
            // This test is for the following scenario:
            // When we find the user has set env variable "ApplicationInsightsAgent_EXTENSION_VERSION" to '~3' 
            // Oryx should not attach appinsight codeless config to runscript

            // Arrange
            var hostDir = Path.Combine(_hostSamplesDir, "dotnetcore", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var imageName = _imageHelper.GetRuntimeImage("node", dotNetVersion);
            //agentextension version will be set to '~3' or '' or 'disabled'
            var agentExtensionVersionEnv = ExtVarNames.UserAppInsightsAgentExtensionVersion;
            var connectionStringEnv = ExtVarNames.UserAppInsightsConnectionStringEnv;
            int containerDebugPort = 8080;
            var OryxAppInsightsAttachString1 = "export ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=";
            var OryxAppInsightsAttachString2 = "export DOTNET_STARTUP_HOOKS=";
            var testTomlFile =
                @"DotNetCoreRuntimeVersion=""6.0.0-preview.3.21201.4""
                  DotNetCoreSdkVersion = ""6.0.100-preview.3.21202.5""
                  StartupDllFileName = NetCore6PreviewMvcApp.dll";

            var script = new ShellScriptBuilder()
                .AddCommand($"echo {testTomlFile} > {appDir}")
                .AddCommand($"export {agentExtensionVersionEnv}={agentExtensionVersionEnvValue}")
                .AddCommand($"export {connectionStringEnv}=alkajsldkajd")
                .AddCommand($"oryx create-script -appPath {appDir}")
                .AddCommand($"./run.sh > {appDir}/log.log")
                .AddStringExistsInFileCheck(OryxAppInsightsAttachString1, $"{appDir}/run.sh")
                .AddStringExistsInFileCheck(OryxAppInsightsAttachString2, $"{appDir}/run.sh")
                .ToString();

            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: _imageHelper.GetRuntimeImage("dotnetcore", dotNetVersion),
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                port: containerDebugPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", script },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Welcome to ASP.NET Core MVC!", data);
                },
                dockerCli: _dockerCli);
        }


        [Theory]
        [InlineData("3.1")]
        [InlineData("5.0")]
        public void DotnetMonitorTool_IsPresentInTheImage(string version)
        {
            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("dotnetcore", version),
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", $"ls opt/dotnetcore-tools/" },
            });

            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("dotnet-monitor", actualOutput);
                },
                result.GetDebugInfo());
        }
    }
}
