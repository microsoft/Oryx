// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Integration.Tests.VSCodeDebugProtocol;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public class PythonDebuggingTests : PythonEndToEndTestsBase
    {
        private const int DefaultDebuggerPort = 5678;

        public PythonDebuggingTests(ITestOutputHelper output, TestTempDirTestFixture tempDir)
            : base(output, tempDir)
        {
        }

        [Theory]
        [Trait("category", "python-3.7")]
        [Trait("build-image", "debian-stretch")]
        [InlineData("3.7", ImageTestHelperConstants.OsTypeDebianBuster, 5637)] // Test with a non-default port as well
        [InlineData("3.7", ImageTestHelperConstants.OsTypeDebianBullseye, 5637)] // Test with a non-default port as well
        public async Task CanBuildAndDebugFlaskAppAsync(string pythonVersion, string osType, int? debugPort = null)
        {
            // Arrange
            var appName = "flask-app";
            var appVolume = CreateAppVolume(appName);
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var scriptGenDebugPortArg = debugPort.HasValue ? $"-debugPort {debugPort.Value}" : string.Empty;

            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appVolume.ContainerDir} -i /tmp/int -o {appOutputDir} " +
               $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion} --debug")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}" +
                            $" -debugAdapter ptvsd {scriptGenDebugPortArg} -debugWait")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { appVolume, appOutputDirVolume },
                "/bin/bash", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("python", pythonVersion, osType),
                debugPort.GetValueOrDefault(DefaultDebuggerPort),
                "/bin/bash", new[] { "-c", runScript },
                async (ptvsdHostPort) =>
                {
                    // Send an Initialize request to make sure the debugger is running
                    using (var debugClient = new SimpleDAPClient("127.0.0.1", ptvsdHostPort, "oryxtests"))
                    {
                        string initResponse = await debugClient.InitializeAsync();
                        // Deliberately weak assertion; don't care what's in the response, only that there IS a response
                        Assert.False(string.IsNullOrEmpty(initResponse));
                    }
                });
        }

        [Theory]
        [Trait("category", "python-3.7")]
        [Trait("build-image", "debian-stretch")]
        [InlineData("3.7", ImageTestHelperConstants.OsTypeDebianBuster, 5637)] // Test with a non-default port as well
        [InlineData("3.7", ImageTestHelperConstants.OsTypeDebianBullseye, 5637)] // Test with a non-default port as well
        public async Task CanBuildAndDebugFlaskAppWithDebugPyAsync(string pythonVersion, string osType, int? debugPort = null)
        {
            // Arrange
            var appName = "flask-app";
            var appVolume = CreateAppVolume(appName);
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var scriptGenDebugPortArg = debugPort.HasValue ? $"-debugPort {debugPort.Value}" : string.Empty;

            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appVolume.ContainerDir} -i /tmp/int -o {appOutputDir} " +
               $"--platform {PythonConstants.PlatformName} --platform-version {pythonVersion} --debug")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}" +
                            $" -debugAdapter debugpy {scriptGenDebugPortArg} -debugWait")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { appVolume, appOutputDirVolume },
                "/bin/bash", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("python", pythonVersion, osType),
                debugPort.GetValueOrDefault(DefaultDebuggerPort),
                "/bin/bash", new[] { "-c", runScript },
                async (debugPyHostPort) =>
                {
                    // Send an Initialize request to make sure the debugger is running
                    using (var debugClient = new SimpleDAPClient("127.0.0.1", debugPyHostPort, "oryxtests"))
                    {
                        string initResponse = await debugClient.InitializeAsync();
                        // Deliberately weak assertion; don't care what's in the response, only that there IS a response
                        Assert.False(string.IsNullOrEmpty(initResponse));
                    }
                });
        }
    }
}