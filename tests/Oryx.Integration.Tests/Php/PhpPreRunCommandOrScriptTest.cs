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
        private readonly string RunScriptTempPath = "/tmp/startup_temp.sh";
        private readonly string RunScriptPreRunPath = "/tmp/startup_prerun.sh";

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
            
            // split run script to test pre-run command or script and then run the app
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(FilePaths.PreRunCommandEnvVarName,
                    $"\"touch {appOutputDir}/_test_file.txt\ntouch {appOutputDir}/_test_file_2.txt\"")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -output {RunScriptPath}")
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
            var preRunScriptPath = $"{appOutputDir}/prerunscript.sh";
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform php --language-version {phpVersion} -o {appOutputDir}")
               .ToString();
            
            // split run script to test pre-run command or script and then run the app
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(FilePaths.PreRunCommandEnvVarName, preRunScriptPath)
                .AddCommand($"touch {preRunScriptPath}")
                .AddFileExistsCheck(preRunScriptPath)
                .AddCommand($"echo \"touch {appOutputDir}/_test_file.txt\" > {preRunScriptPath}")
                .AddStringExistsInFileCheck($"touch {appOutputDir}/_test_file.txt", $"{preRunScriptPath}")
                .AddCommand($"chmod 755 {preRunScriptPath}")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -output {RunScriptPath}")
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
            var preRunScriptPath = $"{appOutputDir}/prerunscript.sh";
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} --platform php --language-version {phpVersion} -o {appOutputDir}")
               .ToString();

            // split run script to test pre-run command or script and then run the app
            var runScript = new ShellScriptBuilder()
                .SetEnvironmentVariable(FilePaths.PreRunCommandEnvVarName, preRunScriptPath)
                .AddCommand($"touch {preRunScriptPath}")
                .AddFileExistsCheck(preRunScriptPath)
                .AddCommand($"echo \"apt-get install --assume-yes htop\" > {preRunScriptPath}")
                .AddCommand($"chmod 755 {preRunScriptPath}")
                .AddCommand($"apt-get remove --assume-yes htop")
                .AddCommand($"apt-get remove --assume-yes htop | grep '0 to remove' > {appOutputDir}/_cli_output.txt")
                .AddStringExistsInFileCheck("0 to remove", $"{appOutputDir}/_cli_output.txt")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -output {RunScriptPath}")
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
                .AddCommand($"apt-get install --assume-yes htop | grep '0 newly installed' > {appOutputDir}/_cli_output.txt")
                .AddStringExistsInFileCheck("0 newly installed", $"{appOutputDir}/_cli_output.txt")
                .AddCommand($"rm {appOutputDir}/_cli_output.txt")
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
