// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Ruby;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "ruby")]
    public class RubyPreRunCommandOrScriptTest : RubyEndToEndTestsBase
    {
        private readonly string RunScriptPath = "/tmp/startup.sh";
        private readonly string RunScriptTempPath = "/tmp/startup_temp.sh";
        private readonly string RunScriptPreRunPath = "/tmp/startup_prerun.sh";

        public RubyPreRunCommandOrScriptTest(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanBuildAndRunRubyApp_UsingPreRunCommand_WithDynamicInstall()
        {
            // Arrange
            var rubyVersion = "2.7";
            var appName = "sinatra-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddDefaultTestEnvironmentVariables()
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int " +
                $"--platform ruby --platform-version {rubyVersion} -o {appOutputDir}")
               .ToString();

            // split run script to test pre-run command or script and then run the app
            var runScript = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .SetEnvironmentVariable(SettingsKeys.EnableDynamicInstall, true.ToString())
                .SetEnvironmentVariable(FilePaths.PreRunCommandEnvVarName,
                    $"\"touch {appOutputDir}/_test_file.txt\ntouch {appOutputDir}/_test_file_2.txt\"")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -output {RunScriptPath} -bindPort {ContainerPort}")
                .AddCommand($"LINENUMBER=\"$(grep -n '# End of pre-run' {RunScriptPath} | cut -f1 -d:)\"")
                .AddCommand($"eval \"head -n +${{LINENUMBER}} {RunScriptPath} > {RunScriptPreRunPath}\"")
                .AddCommand($"chmod 755 {RunScriptPreRunPath}")
                .AddCommand($"LINENUMBERPLUSONE=\"$(expr ${{LINENUMBER}} + 1)\"")
                .AddCommand($"eval \"tail -n +${{LINENUMBERPLUSONE}} {RunScriptPath} > {RunScriptTempPath}\"")
                .AddCommand($"mv {RunScriptTempPath} {RunScriptPath}")
                .AddCommand($"head -n +1 {RunScriptPreRunPath} | cat - {RunScriptPath} > {RunScriptTempPath}")
                .AddCommand($"mv {RunScriptTempPath} {RunScriptPath}")
                .AddCommand($"chmod 755 {RunScriptPath}")
                .AddCommand($"unset LINENUMBER")
                .AddCommand($"unset LINENUMBERPLUSONE")
                .AddCommand(RunScriptPreRunPath)
                .AddFileExistsCheck($"{appOutputDir}/_test_file.txt")
                .AddFileExistsCheck($"{appOutputDir}/_test_file_2.txt")
                .AddCommand(RunScriptPath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(),
                "/bin/bash", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("ruby", "dynamic"),
                ContainerPort,
                "/bin/bash",
                new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRunRubyApp_UsingPreRunScript_WithDynamicInstall()
        {
            // Arrange
            var rubyVersion = "2.7";
            var appName = "sinatra-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var preRunScriptPath = $"{appOutputDir}/prerunscript.sh";
            var buildScript = new ShellScriptBuilder()
               .AddDefaultTestEnvironmentVariables()
               .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform ruby --platform-version {rubyVersion}")
               .ToString();

            // split run script to test pre-run command and then run the app
            var runScript = new ShellScriptBuilder()
                .AddDefaultTestEnvironmentVariables()
                .SetEnvironmentVariable(SettingsKeys.EnableDynamicInstall, true.ToString())
                .SetEnvironmentVariable(FilePaths.PreRunCommandEnvVarName, $"\"touch '{appOutputDir}/_test_file_2.txt' && {preRunScriptPath}\"")
                .AddCommand($"touch {preRunScriptPath}")
                .AddFileExistsCheck(preRunScriptPath)
                .AddCommand($"echo \"touch {appOutputDir}/_test_file.txt\" > {preRunScriptPath}")
                .AddStringExistsInFileCheck($"touch {appOutputDir}/_test_file.txt", $"{preRunScriptPath}")
                .AddCommand($"chmod 755 {preRunScriptPath}")

                .AddCommand($"oryx create-script -appPath {appOutputDir} -output {RunScriptPath} -bindPort {ContainerPort}")

                .AddCommand($"LINENUMBER=\"$(grep -n '# End of pre-run' {RunScriptPath} | cut -f1 -d:)\"")
                .AddCommand($"eval \"head -n +${{LINENUMBER}} {RunScriptPath} > {RunScriptPreRunPath}\"")
                .AddCommand($"chmod 755 {RunScriptPreRunPath}")
                .AddCommand($"LINENUMBERPLUSONE=\"$(expr ${{LINENUMBER}} + 1)\"")
                .AddCommand($"eval \"tail -n +${{LINENUMBERPLUSONE}} {RunScriptPath} > {RunScriptTempPath}\"")
                .AddCommand($"mv {RunScriptTempPath} {RunScriptPath}")
                .AddCommand($"head -n +1 {RunScriptPreRunPath} | cat - {RunScriptPath} > {RunScriptTempPath}")
                .AddCommand($"mv {RunScriptTempPath} {RunScriptPath}")
                .AddCommand($"chmod 755 {RunScriptPath}")
                .AddCommand($"unset LINENUMBER")
                .AddCommand($"unset LINENUMBERPLUSONE")
                .AddCommand(RunScriptPreRunPath)
                .AddFileExistsCheck($"{appOutputDir}/_test_file.txt")
                .AddFileExistsCheck($"{appOutputDir}/_test_file_2.txt")
                .AddCommand(RunScriptPath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                _imageHelper.GetGitHubActionsBuildImage(),
                "/bin/bash", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("ruby", "dynamic"),
                ContainerPort,
                "/bin/bash",
                new[] { "-c", runScript },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }

        [Fact]
        public async Task CanRunApp_UsingPreRunCommand_FromBuildEnvFile()
        {
            // Arrange
            var version = "2.7";
            var appName = "sinatra-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var expectedFileInOutputDir = Guid.NewGuid().ToString("N");
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {RubyConstants.PlatformName} --platform-version {version}")
                // Create a 'build.env' file
                .AddCommand(
                $"echo '{FilePaths.PreRunCommandEnvVarName}=\"echo > {expectedFileInOutputDir}\"' > " +
                $"{appOutputDir}/{BuildScriptGeneratorCli.Constants.BuildEnvironmentFileName}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new DockerVolume[] { volume, appOutputDirVolume },
                _imageHelper.GetLtsVersionsBuildImage(),
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("ruby", version),
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!", data);

                    // Verify that the file created using the pre-run command is 
                    // in fact present in the output directory.
                    Assert.True(File.Exists(Path.Combine(appOutputDirVolume.MountedHostDir, expectedFileInOutputDir)));
                });
        }
    }
}
