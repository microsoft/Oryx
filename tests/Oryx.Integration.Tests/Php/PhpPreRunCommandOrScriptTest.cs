// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "php")]
    public class PhpPreRunCommandOrScriptTest : PhpEndToEndTestsBase
    {
        public PhpPreRunCommandOrScriptTest(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Fact]
        public async Task TwigExampleCanBuildAndRun_UsingPreRunCommand()
        {
            // Arrange
            var phpVersion = "7.4";
            var appName = "twig-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir"; 
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform php --language-version {phpVersion} -o {appOutputDir}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(FilePaths.PreRunCommandEnvVarName, $"\"touch {appOutputDir}/test_pre_run.txt\"")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -output {RunScriptPath}")
                .AddFileExistsCheck($"{appOutputDir}/test_pre_run.txt")
                .AddCommand($"rm {appOutputDir}/test_pre_run.txt")
                .AddCommand(RunScriptPath)
                .ToString();

            // Act & Assert
            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName, _output, volume,
                "/bin/sh", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("php", phpVersion),
                ContainerPort,
                "/bin/sh", new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("<h1>Hello World!</h1>", data);
                });
        }

        [Fact]
        public async Task TwigExampleCanBuildAndRun_UsingPreRunScript()
        {
            // Arrange
            var phpVersion = "7.4";
            var appName = "twig-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir"; 
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform php --language-version {phpVersion} -o {appOutputDir}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(FilePaths.PreRunCommandEnvVarName, $"{appOutputDir}/prerunscript.sh")
                .AddCommand($"touch {appOutputDir}/prerunscript.sh")
                .AddFileExistsCheck($"{appOutputDir}/prerunscript.sh")
                .AddCommand($"echo \"touch {appOutputDir}/test_pre_run.txt\" > {appOutputDir}/prerunscript.sh")
                .AddStringExistsInFileCheck($"touch {appOutputDir}/test_pre_run.txt", $"{appOutputDir}/prerunscript.sh")
                .AddCommand($"chmod 755 {appOutputDir}/prerunscript.sh")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -output {RunScriptPath}")
                .AddFileExistsCheck($"{appOutputDir}/test_pre_run.txt")
                .AddCommand($"rm {appOutputDir}/test_pre_run.txt")
                .AddCommand(RunScriptPath)
                .ToString();

            // Act & Assert
            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName, _output, volume,
                "/bin/sh", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("php", phpVersion),
                ContainerPort,
                "/bin/sh", new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("<h1>Hello World!</h1>", data);
                });
        }

        [Fact]
        public async Task TwigExampleCanBuildAndRun_UsingPreRunScriptToInstallExtension()
        {
            // Arrange
            var phpVersion = "7.4";
            var appName = "twig-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir"; 
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform php --language-version {phpVersion} -o {appOutputDir}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(FilePaths.PreRunCommandEnvVarName, $"{appOutputDir}/prerunscript.sh")
                .AddCommand($"touch {appOutputDir}/prerunscript.sh")
                .AddFileExistsCheck($"{appOutputDir}/prerunscript.sh")
                .AddCommand($"echo \"apt-get install php-json\" > {appOutputDir}/prerunscript.sh")
                .AddCommand($"chmod 755 {appOutputDir}/prerunscript.sh")
                .AddCommand($"php -m | grep 'json' > {appOutputDir}/_temp.txt")
                .AddStringDoesNotExistInFileCheck("json", $"{appOutputDir}/_temp.txt")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -output {RunScriptPath}")
                .AddCommand($"php -m | grep 'json' > {appOutputDir}/_temp.txt")
                .AddStringExistsInFileCheck("json", $"{appOutputDir}/_temp.txt")
                .AddCommand($"rm {appOutputDir}/_temp.txt")
                .AddCommand(RunScriptPath)
                .ToString();

            // Act & Assert
            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName, _output, volume,
                "/bin/sh", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("php", phpVersion),
                ContainerPort,
                "/bin/sh", new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("<h1>Hello World!</h1>", data);
                });
        }
    }
}
