﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Integration.Tests.Fixtures;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.Integration.Tests
{
    [Trait("db", "sqlserver")]
    public class NodeSqlServerIntegrationTests : PlatformEndToEndTestsBase
    {
        private const int ContainerPort = 3000;
        private const string DefaultStartupFilePath = "./run.sh";

        public NodeSqlServerIntegrationTests(ITestOutputHelper output) : base(output, null)
        {
        }

        [Fact]
        [Trait("category", "node-14-gh-buster")]
        [Trait("build-image", "github-actions-debian-buster")]
        public async Task NodeApp_MicrosoftSqlServerDBAsync_With_GitHubActionsBusterTag()
        {
            await Run_NodeApp_MicrosoftSqlServerDBAsync(ImageTestHelperConstants.GitHubActionsBuster);
        }

        [Fact]
        [Trait("category", "node-14-stretch-3")]
        [Trait("build-image", "debian-stretch")]
        public async Task NodeApp_MicrosoftSqlServerDBAsync_With_LatestStretchTag()
        {
            await Run_NodeApp_MicrosoftSqlServerDBAsync(ImageTestHelperConstants.LatestStretchTag);
        }

        private async Task Run_NodeApp_MicrosoftSqlServerDBAsync(string imageTag)
        {
            // Arrange
            var appName = "node-mssql";
            var hostDir = Path.Combine(_hostSamplesDir, "nodejs", appName);
            var volume = DockerVolume.CreateMirror(hostDir);
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddCommand($"oryx create-script -appPath {appDir} -bindPort {ContainerPort}")
                .AddCommand(DefaultStartupFilePath)
                .ToString();
            List<EnvironmentVariable> buildEnvVariableList = GetEnvironmentVariableList();

            await EndToEndTestHelper.BuildRunAndAssertAppAsync(
                appName,
                _output,
                new List<DockerVolume> { volume },
                _imageHelper.GetBuildImage(imageTag),
                "oryx",
                new[] { "build", appDir, "--platform", "nodejs", "--platform-version", "14" },
                _imageHelper.GetRuntimeImage("node", "14"),
                buildEnvVariableList,
                ContainerPort,
                "/bin/bash",
                new[]
                {
                    "-c",
                    script
                },
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

        protected string GetKeyVaultSecretValue(string keyVaultUri, string secretName)
        {
            var client = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
            var sasToken = client.GetSecret(secretName).Value.Value;
            return sasToken;
        }
        
        private List<EnvironmentVariable> GetEnvironmentVariableList()
        {
            List<EnvironmentVariable> envVariableList = SqlServerDbTestHelper.GetEnvironmentVariables();
            var testStorageAccountUrl = Environment.GetEnvironmentVariable(SdkStorageConstants.TestingSdkStorageUrlKeyName);
            var sdkStorageUrl = string.IsNullOrEmpty(testStorageAccountUrl) ? SdkStorageConstants.PrivateStagingSdkStorageBaseUrl : testStorageAccountUrl;

            envVariableList.Add(new EnvironmentVariable(SdkStorageConstants.SdkStorageBaseUrlKeyName, sdkStorageUrl));

            if (sdkStorageUrl == SdkStorageConstants.PrivateStagingSdkStorageBaseUrl)
            {
                string stagingStorageSasToken = Environment.GetEnvironmentVariable(SdkStorageConstants.PrivateStagingStorageSasTokenKey) ?? 
                    this.GetKeyVaultSecretValue(SdkStorageConstants.OryxKeyvaultUri, SdkStorageConstants.StagingStorageSasTokenKeyvaultSecretName);
                envVariableList.Add(new EnvironmentVariable(SdkStorageConstants.PrivateStagingStorageSasTokenKey, stagingStorageSasToken));
            }

            return envVariableList;
        }

    }
}