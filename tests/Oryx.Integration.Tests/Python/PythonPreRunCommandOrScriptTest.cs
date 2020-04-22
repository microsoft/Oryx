// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "python")]
    public class PythonPreRunCommandOrScriptTest : PythonEndToEndTestsBase
    {
        private readonly string DefaultSdksRootDir = "/opt/python";

        private readonly string RunScriptPath = "/tmp/startup.sh";
        private readonly string RunScriptTempPath = "/tmp/startup_temp.sh";
        private readonly string RunScriptPreRunPath = "/tmp/startup_prerun.sh";

        public PythonPreRunCommandOrScriptTest(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        [Fact]
        public async Task CanBuildAndRunPythonApp_UsingPreRunCommand_WithDynamicInstall()
        {
            // Arrange
            var pythonVersion = "3.7";
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var buildScript = new ShellScriptBuilder()
               .SetEnvironmentVariable(SettingsKeys.EnableDynamicInstall, true.ToString())
               .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
               .AddCommand(
                $"oryx build {appDir} --platform python --platform-version {pythonVersion} -o {appOutputDir}")
               .ToString();

            // split run script to test pre-run command or script and then run the app
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(SettingsKeys.EnableDynamicInstall, true.ToString())
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)

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
                new[] { volume },
                _imageHelper.GetGitHubActionsBuildImage(),
                "/bin/bash", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("python", "dynamic"),
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
        public async Task CanBuildAndRunPythonApp_UsingPreRunScript_WithDynamicInstall()
        {
            // Arrange
            var pythonVersion = "3.7";
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/myoutputdir";
            var preRunScriptPath = $"{appOutputDir}/prerunscript.sh";
            var buildScript = new ShellScriptBuilder()
               .SetEnvironmentVariable(SettingsKeys.EnableDynamicInstall, true.ToString())
               .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
               .AddCommand(
                $"oryx build {appDir} --platform python --platform-version {pythonVersion} -o {appOutputDir}")
               .ToString();

            // split run script to test pre-run command and then run the app
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(SettingsKeys.EnableDynamicInstall, true.ToString())
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
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
                new[] { volume },
                _imageHelper.GetGitHubActionsBuildImage(),
                "/bin/bash", new[] { "-c", buildScript },
                _imageHelper.GetRuntimeImage("python", "dynamic"),
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
            var appName = "flask-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform {PythonConstants.PlatformName} --platform-version 3.6")
               .ToString();

            // Create a 'build.env' file
            var fileName = Guid.NewGuid().ToString("N");
            File.WriteAllText(
                $"{FilePaths.PreRunCommandEnvVarName}=\"echo > {fileName}\"",
                Path.Combine(appDir, BuildScriptGeneratorCli.Constants.BuildEnvironmentFileName));
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                volume,
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("python", "3.6"),
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runScript
                },
                async (hostPort) =>
                {
                    // Verify that the file created using the pre-run command is 
                    // in fact present in the output directory.
                    Assert.True(File.Exists(Path.Combine(appDir, fileName)));

                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("Hello World!", data);
                });
        }
    }
}
