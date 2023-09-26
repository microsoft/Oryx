// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Collection("Php integration")]
    [Trait("category", "php-7.4")]
    public class PhpPreRunCommandOrScriptTest : PhpEndToEndTestsBase
    {
        public PhpPreRunCommandOrScriptTest(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Fact]
        [Trait("build-image", "debian-stretch")]
        public async Task TwigExampleCanBuildAndRun_UsingPreRunCommandAsync()
        {
            // Arrange
            var phpVersion = "7.4";
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "twig-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            // pre-run command which writes out a file to the output/app directory
            var preRunCmdGeneratedFileName = Guid.NewGuid().ToString("N");
            // Note that we are using MountedHostDir rather than the directory in the container. This allows us to
            // write an asset from this test to check the host directory itself even after the container is killed.
            var expectedFileInOutput = Path.Join(appOutputDirVolume.MountedHostDir, preRunCmdGeneratedFileName);

            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
               $"--platform php --platform-version {phpVersion}")
               .ToString();

            // split run script to test pre-run command or script and then run the app
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(FilePaths.PreRunCommandEnvVarName, $"'echo > {preRunCmdGeneratedFileName}'", true)
                .AddCommand($"oryx create-script -appPath {appOutputDir} -output {RunScriptPath}")
                .AddCommand($"cat {RunScriptPath}")
                .AddCommand(RunScriptPath)
                .ToString();

            // Act & Assert
            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName, _output, new[] { volume, appOutputDirVolume },
                "/bin/sh", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("php", phpVersion, osType),
                ContainerPort,
                "/bin/sh", new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("<h1>Hello World!</h1>", data);

                    Assert.True(File.Exists(expectedFileInOutput));
                });
        }

        [Fact]
        [Trait("build-image", "debian-stretch")]
        public async Task TwigExampleCanBuildAndRun_UsingPreRunScriptAsync()
        {
            // Arrange
            var phpVersion = "7.4";
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "twig-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            // pre-run script which writes out a file to the output/app directory
            var preRunScriptGeneratedFileName = Guid.NewGuid().ToString("N");
            File.WriteAllText(Path.Join(volume.MountedHostDir, "prerunscript.sh"),
                "#!/bin/bash\n" +
                "set -ex\n" +
                $"echo > {preRunScriptGeneratedFileName}\n");
            // Note that we are using MountedHostDir rather than the directory in the container. This allows us to
            // write an asset from this test to check the host directory itself even after the container is killed.
            var expectedFileInOutput = Path.Join(appOutputDirVolume.MountedHostDir, preRunScriptGeneratedFileName);

            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int " +
               $"--platform php --platform-version {phpVersion} -o {appOutputDir}")
               .ToString();

            // split run script to test pre-run command or script and then run the app
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(FilePaths.PreRunCommandEnvVarName, "./prerunscript.sh")
                .AddCommand($"chmod +x {appOutputDir}/prerunscript.sh")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -output {RunScriptPath}")
                .AddCommand(RunScriptPath)
                .ToString();

            // Act & Assert
            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName, _output, new[] { volume, appOutputDirVolume },
                "/bin/sh", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("php", phpVersion, osType),
                ContainerPort,
                "/bin/sh", new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("<h1>Hello World!</h1>", data);

                    Assert.True(File.Exists(expectedFileInOutput));
                });
        }

        [Fact]
        [Trait("build-image", "debian-stretch")]
        public async Task TwigExampleCanBuildAndRun_UsingPreRunScriptToInstallExtensionAsync()
        {
            // Arrange
            var phpVersion = "7.4";
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "twig-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            // pre-run script which writes out a file to the output/app directory
            var preRunScriptGeneratedFileName = Guid.NewGuid().ToString("N");
            File.WriteAllText(Path.Join(volume.MountedHostDir, "prerunscript.sh"),
                "#!/bin/bash\n" +
                "set -ex\n" +
                "apt-get update\n" +
                "apt-get install -y htop\n" +
                $"apt list --installed > {preRunScriptGeneratedFileName}\n");
            // Note that we are using MountedHostDir rather than the directory in the container. This allows us to
            // write an asset from this test to check the host directory itself even after the container is killed.
            var expectedFileInOutput = Path.Join(appOutputDirVolume.MountedHostDir, preRunScriptGeneratedFileName);

            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int " +
               $"--platform php --platform-version {phpVersion} -o {appOutputDir}")
               .ToString();

            // split run script to test pre-run command or script and then run the app
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(FilePaths.PreRunCommandEnvVarName, "./prerunscript.sh")
                .AddCommand($"chmod +x {appOutputDir}/prerunscript.sh")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -output {RunScriptPath}")
                .AddCommand(RunScriptPath)
                .ToString();

            // Act & Assert
            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName, _output, new[] { volume, appOutputDirVolume },
                "/bin/sh", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("php", phpVersion, osType),
                ContainerPort,
                "/bin/sh", new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("<h1>Hello World!</h1>", data);

                    Assert.True(File.Exists(expectedFileInOutput));
                    var installedPackages = File.ReadAllText(expectedFileInOutput);
                    Assert.Contains("htop", installedPackages);
                });
        }

        [Fact]
        [Trait("build-image", "debian-stretch")]
        public async Task CanRunApp_UsingPreRunCommand_FromBuildEnvFileAsync()
        {
            // Arrange
            var phpVersion = "7.4";
            var osType = ImageTestHelperConstants.OsTypeDebianBullseye;
            var appName = "twig-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var expectedFileInOutputDir = Guid.NewGuid().ToString("N");
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PhpConstants.PlatformName} --platform-version {phpVersion}")
                // Create a 'build.env' file
                .AddCommand(
                $"echo '{FilePaths.PreRunCommandEnvVarName}=\"echo > {expectedFileInOutputDir}\"' > " +
                $"{appOutputDir}/{BuildScriptGeneratorCli.Constants.BuildEnvironmentFileName}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -output {RunScriptPath}")
                .AddCommand(RunScriptPath)
                .ToString();

            // Act & Assert
            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/sh", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("php", phpVersion, osType),
                ContainerPort,
                "/bin/sh", new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("<h1>Hello World!</h1>", data);

                    // Verify that the file created using the pre-run command is 
                    // in fact present in the output directory.
                    Assert.True(File.Exists(Path.Combine(appOutputDirVolume.MountedHostDir, expectedFileInOutputDir)));
                });
        }
    }
}
