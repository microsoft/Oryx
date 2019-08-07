// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "python")]
    public class PythonDebuggingTests : PythonEndToEndTestsBase
    {
        public PythonDebuggingTests(ITestOutputHelper output, TestTempDirTestFixture tempDir)
            : base(output, tempDir)
        {
        }

        [Theory]
        [InlineData("3.7", "ptvsd")]
        public async Task CanBuildAndDebugFlaskApp(string pythonVersion, string debugAdapter)
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);

            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {volume.ContainerDir} --platform python --platform-version {pythonVersion} --debug")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx -appPath {volume.ContainerDir} -bindPort {ContainerPort} -debugAdapter {debugAdapter} -debugWait")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/bash", new[] { "-c", buildScript },
                $"oryxdevmcr.azurecr.io/public/oryx/python-{pythonVersion}",
                ContainerPort,
                "/bin/bash", new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.DoesNotContain("Hello World!", data);
                });
        }
    }
}