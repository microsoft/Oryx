// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Integration.Tests.Fixtures;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    public abstract class DatabaseTestsBase
    {
        protected readonly ITestOutputHelper _output;
        protected readonly ImageTestHelper _imageHelper;
        protected readonly Fixtures.DbContainerFixtureBase _dbFixture;
        protected readonly HttpClient _httpClient = new HttpClient();

        protected DatabaseTestsBase(ITestOutputHelper outputHelper, Fixtures.DbContainerFixtureBase dbFixture)
        {
            _output = outputHelper;
            _imageHelper = new ImageTestHelper(_output);
            _dbFixture = dbFixture;
            HostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
        }

        protected string HostSamplesDir { get; }

        protected async Task RunTestAsync(
            string language,
            string languageVersion,
            string samplePath,
            int containerPort = 8000,
            bool specifyBindPortFlag = true,
            string buildImageName = Settings.BuildImageName)
        {
            var volume = DockerVolume.CreateMirror(samplePath);
            var appDir = volume.ContainerDir;
            var entrypointScript = "./run.sh";
            var bindPortFlag = specifyBindPortFlag ? $"-bindPort {containerPort}" : string.Empty;
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"oryx -appPath {appDir} {bindPortFlag}")
                .AddCommand(entrypointScript)
                .ToString();

            var runtimeImageName = _imageHelper.GetRuntimeImage(language, languageVersion);
            if (string.Equals(language, "nodejs", StringComparison.OrdinalIgnoreCase))
            {
                runtimeImageName = _imageHelper.GetRuntimeImage("node", languageVersion);
            }

            string link = $"{_dbFixture.DbServerContainerName}:{Constants.InternalDbLinkName}";

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                _output,
                new List<DockerVolume> { volume },
                buildImageName,
                "oryx", new[] { "build", appDir, "-l", language, "--language-version", languageVersion },
                runtimeImageName,
                _dbFixture.GetCredentialsAsEnvVars(),
                containerPort,
                link,
                "/bin/sh", new[] { "-c", script },
                async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/");
                    Assert.Equal(
                        DbContainerFixtureBase.GetSampleDataAsJson(),
                        data.Trim(),
                        ignoreLineEndingDifferences: true,
                        ignoreWhiteSpaceDifferences: true);
                });
        }

        protected void RunAsserts(Action action, string message)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                _output.WriteLine(message);
                throw;
            }
        }
    }
}
