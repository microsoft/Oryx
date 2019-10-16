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
        protected const string _imageBaseEnvironmentVariable = "ORYX_TEST_IMAGE_BASE";
        protected const string _defaultImageBase = "oryxdevmcr.azurecr.io";
        protected const string _oryxImageSuffix = "/public/oryx";

        protected readonly ITestOutputHelper _output;
        protected readonly Fixtures.DbContainerFixtureBase _dbFixture;
        protected readonly HttpClient _httpClient = new HttpClient();
        protected readonly string _imageBase;

        protected DatabaseTestsBase(ITestOutputHelper outputHelper, Fixtures.DbContainerFixtureBase dbFixture)
        {
            _output = outputHelper;
            _dbFixture = dbFixture;
            HostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
            _imageBase = Environment.GetEnvironmentVariable(_imageBaseEnvironmentVariable);
            if (string.IsNullOrEmpty(_imageBase))
            {
                _output.WriteLine($"Could not find a value for environment variable " +
                                  $"'{_imageBaseEnvironmentVariable}', using default image base '{_defaultImageBase}'.");
                _imageBase = _defaultImageBase;
            }

            _imageBase += _oryxImageSuffix;
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

            var runtimeImageName = $"{_imageBase}/{language}:{languageVersion}";
            if (string.Equals(language, "nodejs", StringComparison.OrdinalIgnoreCase))
            {
                runtimeImageName = $"{_imageBase}/node:{languageVersion}";
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
