// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests.LocalDockerTests
{
    public abstract class DatabaseTestsBase
    {
        protected readonly Fixtures.DbContainerFixtureBase _dbFixture;
        protected readonly ITestOutputHelper _output;
        private readonly Random _rand = new Random();
        protected readonly int _appPort;

        protected DatabaseTestsBase(ITestOutputHelper outputHelper, Fixtures.DbContainerFixtureBase dbFixture)
        {
            _dbFixture = dbFixture;
            _output = outputHelper;
            _appPort = 8080 + _rand.Next(100);
            HostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
            HttpClient = new HttpClient();
        }

        protected string HostSamplesDir { get; }

        protected HttpClient HttpClient { get; }

        protected async Task RunTestAsync(string language, string languageVersion, string samplePath)
        {
            var volume = DockerVolume.Create(samplePath);
            var appDir = volume.ContainerDir;
            var containerPort = 8000;
            var portMapping = $"{_appPort}:{containerPort}";
            var entrypointScript = "./run.sh";
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"oryx -appPath {appDir} -bindPort {containerPort}")
                .AddCommand(entrypointScript)
                .ToString();

            var runtimeImageName = $"oryxdevms/{language}-{languageVersion}";
            if (string.Equals(language, "nodejs", StringComparison.OrdinalIgnoreCase))
            {
                runtimeImageName = $"oryxdevms/node-{languageVersion}";
            }
            
            string link = $"{_dbFixture.DbServerContainerName}:{Constants.InternalDbLinkName}";

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                _output,
                new List<DockerVolume> { volume },
                "oryx", new[] { "build", appDir, "-l", language, "--language-version", languageVersion },
                runtimeImageName,
                _dbFixture.GetCredentialsAsEnvVars(),
                portMapping, link,
                "/bin/sh", new[] { "-c", script },
                async () =>
                {
                    var data = await HttpClient.GetStringAsync($"http://localhost:{_appPort}/");
                    Assert.Equal(_dbFixture.GetSampleDataAsJson(), data.Trim(), ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
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
