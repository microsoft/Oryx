// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("category", "python")]
    public class PythonDjangoAppTests : PythonEndToEndTestsBase
    {
        public PythonDjangoAppTests(ITestOutputHelper output, TestTempDirTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Fact(Skip = "Legacy python versions are out of support")]
        public async Task CanBuildAndRun_DjangoPython36App_UsingVirtualEnv()
        {
            // Arrange
            var appName = "django-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            const string virtualEnvName = "antenv3.6";
            var buildScript = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version 3.6 -p virtualenv_name={virtualEnvName}")
                .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
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
                    var data = await GetResponseDataAsync($"http://localhost:{hostPort}/staticfiles/css/boards.css");
                    Assert.Contains("CSS file from Boards app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}/staticfiles/css/uservoice.css");
                    Assert.Contains("CSS file from UserVoice app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}/boards/");
                    Assert.Contains("Hello, World! from Boards app", data);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}/uservoice/");
                    Assert.Contains("Hello, World! from Uservoice app", data);
                });
        }

        [Theory(Skip = "Legacy python versions are out of support")]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CanBuildAndRun_DjangoApp_UsingPython36(bool compressDestinationDir)
        {
            // Arrange
            var appName = "django-app";
            var volume = CreateAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var compressDestination = compressDestinationDir ? "--compress-destination-dir" : string.Empty;
            var buildScript = new ShellScriptBuilder()
               .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir} " +
               $"--platform {PythonConstants.PlatformName} --platform-version 3.6 {compressDestination}")
               .ToString();
            var runScript = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
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
                    var data = await GetResponseDataAsync($"http://localhost:{hostPort}/staticfiles/css/boards.css");
                    Assert.Contains("CSS file from Boards app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}/staticfiles/css/uservoice.css");
                    Assert.Contains("CSS file from UserVoice app module", data);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}/boards/");
                    Assert.Contains("Hello, World! from Boards app", data);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}/uservoice/");
                    Assert.Contains("Hello, World! from Uservoice app", data);
                });
        }

        [Fact]
        public async Task CanBuildAndRun_MultiPlatformApp_HavingReactAndDjango()
        {
            // Arrange
            var appName = "reactdjango";
            var hostDir = Path.Combine(_hostSamplesDir, "multilanguage", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var appOutputDirVolume = CreateAppOutputDirVolume();
            var appOutputDir = appOutputDirVolume.ContainerDir;
            var buildScript = new ShellScriptBuilder()
                .AddCommand($"export {BuildScriptGeneratorCli.SettingsKeys.EnableMultiPlatformBuild}=true")
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"--platform {PythonConstants.PlatformName} --platform-version {PythonVersions.Python37Version}")
                .ToString();

            var runAppScript = new ShellScriptBuilder()
                // User would do this through app settings
                .AddCommand("export DJANGO_SETTINGS_MODULE=\"reactdjango.settings.local_base\"")
                .AddCommand($"oryx create-script -appPath {appOutputDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new[] { volume, appOutputDirVolume },
                "/bin/bash",
                new[]
                {
                    "-c",
                    buildScript
                },
                _imageHelper.GetRuntimeImage("python", "3.7"),
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    runAppScript
                },
                async (hostPort) =>
                {
                    var data = await GetResponseDataAsync($"http://localhost:{hostPort}/");
                    Assert.Contains("<h1>it works! (Django Template)</h1>", data);

                    // Looks for the link to the webpack-generated file
                    var linkStartIdx = data.IndexOf("src=\"/static/webpack_bundles/main-");
                    Assert.NotEqual(-1, linkStartIdx);

                    var linkdEndIx = data.IndexOf(".js", linkStartIdx);
                    Assert.NotEqual(-1, linkdEndIx);

                    // We remove 5 chars for `src="` and add 2 since we get the first char of ".js"
                    // but we want to include ".js in the string
                    int length = linkdEndIx - linkStartIdx - 2;
                    var link = data.Substring(linkStartIdx + 5, length);

                    data = await GetResponseDataAsync($"http://localhost:{hostPort}{link}");
                    Assert.Contains("!function(e){var t={};function n(r){if(t[r])return t[r].exports", data);
                });
        }
    }
}