// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    public class PhpFpmTestBase : TestBase, IClassFixture<TestTempDirTestFixture>
    {
        public readonly string _hostSamplesDir;
        public readonly string _tempRootDir;
        public readonly HttpClient _httpClient = new HttpClient();

        public DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "php", sampleAppName));

        public PhpFpmTestBase(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture) : base(output)
        {
            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
            _tempRootDir = testTempDirTestFixture.RootDirPath;
        }
    }

    public class PhpFpmImageTest : PhpTestBase
    {
        public PhpFpmImageTest(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture) : base(output, testTempDirTestFixture)
        {
        }

        [Theory]
        [InlineData("7.4-fpm", ImageTestHelperConstants.OsTypeDebianBuster, PhpVersions.Php74Version)]
        [InlineData("7.4-fpm", ImageTestHelperConstants.OsTypeDebianBullseye, PhpVersions.Php74Version)]
        [InlineData("8.0-fpm", ImageTestHelperConstants.OsTypeDebianBuster, PhpVersions.Php80Version)]
        [InlineData("8.0-fpm", ImageTestHelperConstants.OsTypeDebianBullseye, PhpVersions.Php80Version)]
        [InlineData("8.1-fpm", ImageTestHelperConstants.OsTypeDebianBuster, PhpVersions.Php81Version)]
        [InlineData("8.1-fpm", ImageTestHelperConstants.OsTypeDebianBullseye, PhpVersions.Php81Version)]
        [InlineData("8.2-fpm", ImageTestHelperConstants.OsTypeDebianBuster, PhpVersions.Php82Version)]
        [InlineData("8.2-fpm", ImageTestHelperConstants.OsTypeDebianBullseye, PhpVersions.Php82Version)]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void VersionMatchesImageName(string version, string osType, string expectedPhpVersion)
        {
            // Arrange & Act
            var result = _dockerCli.Run(
                _imageHelper.GetRuntimeImage("php", version, osType),
                "php",
                new[] { "--version" }
            );

            // Assert
            RunAsserts(() =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("PHP " + expectedPhpVersion, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("7.4-fpm", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("7.4-fpm", ImageTestHelperConstants.OsTypeDebianBullseye)]
        [InlineData("8.0-fpm", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("8.0-fpm", ImageTestHelperConstants.OsTypeDebianBullseye)]
        [InlineData("8.1-fpm", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("8.1-fpm", ImageTestHelperConstants.OsTypeDebianBullseye)]
        [InlineData("8.2-fpm", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("8.2-fpm", ImageTestHelperConstants.OsTypeDebianBullseye)]
        public void GraphicsExtension_Gd_IsInstalled(string version, string osType)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, osType),
                CommandToExecuteOnRun = "php",
                CommandArguments = new[] { "-r", "echo json_encode(gd_info());" }
            });

            // Assert
            JObject gdInfo = JsonConvert.DeserializeObject<JObject>(result.StdOut);
            Assert.True((bool)((JValue)gdInfo.GetValue("GIF Read Support")).Value);
            Assert.True((bool)((JValue)gdInfo.GetValue("GIF Create Support")).Value);
            Assert.True((bool)((JValue)gdInfo.GetValue("JPEG Support")).Value);
            Assert.True((bool)((JValue)gdInfo.GetValue("PNG Support")).Value);
        }

        [Theory]
        [InlineData("7.4-fpm", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("7.4-fpm", ImageTestHelperConstants.OsTypeDebianBullseye)]
        public void MySqlnd_Azure_IsInstalled(string version, string osType)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, osType),
                CommandToExecuteOnRun = "php",
                CommandArguments = new[] { "-m", " | grep mysqlnd_azure);" }
            });

            // Assert
            var output = result.StdOut.ToString();
            RunAsserts(() =>
            {
                Assert.True(result.IsSuccess);
                Assert.Contains("mysqlnd_azure", output);
            },
                result.GetDebugInfo());

        }

        [SkippableTheory]
        [InlineData("7.4-fpm", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("7.4-fpm", ImageTestHelperConstants.OsTypeDebianBullseye)]
        [InlineData("8.0-fpm", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("8.0-fpm", ImageTestHelperConstants.OsTypeDebianBullseye)]
        [InlineData("8.1-fpm", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("8.1-fpm", ImageTestHelperConstants.OsTypeDebianBullseye)]
        [InlineData("8.2-fpm", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("8.2-fpm", ImageTestHelperConstants.OsTypeDebianBullseye)]
        public void PhpFpmRuntimeImage_Contains_VersionAndCommit_Information(string version, string osType)
        {
            // we cant always rely on gitcommitid as env variable in case build context is not correctly passed
            // so we should check agent_os environment variable to know if the build is happening in azure devops agent
            // or locally, locally we need to skip this test
            var agentOS = Environment.GetEnvironmentVariable("AGENT_OS");
            Skip.If(string.IsNullOrEmpty(agentOS));

            // Arrange
            var gitCommitID = GitHelper.GetCommitID();
            var buildNumber = Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");
            var expectedOryxVersion = string.Concat(Settings.OryxVersion, buildNumber);

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, osType),
                CommandToExecuteOnRun = "oryx",
                CommandArguments = new[] { "version" }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.NotNull(result.StdErr);
                    Assert.DoesNotContain(".unspecified, Commit: unspecified", result.StdOut);
                    Assert.Contains(gitCommitID, result.StdOut);
                    Assert.Contains(expectedOryxVersion, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("7.4-fpm", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("7.4-fpm", ImageTestHelperConstants.OsTypeDebianBullseye)]
        [InlineData("8.0-fpm", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("8.0-fpm", ImageTestHelperConstants.OsTypeDebianBullseye)]
        [InlineData("8.1-fpm", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("8.1-fpm", ImageTestHelperConstants.OsTypeDebianBullseye)]
        [InlineData("8.2-fpm", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("8.2-fpm", ImageTestHelperConstants.OsTypeDebianBullseye)]
        public void Redis_IsInstalled(string version, string osType)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, osType),
                CommandToExecuteOnRun = "php",
                CommandArguments = new[] { "-m", " | grep redis);" }
            });

            // Assert
            var output = result.StdOut.ToString();
            RunAsserts(() =>
            {
                Assert.True(result.IsSuccess);
                Assert.Contains("redis", output);
            },
                result.GetDebugInfo());

        }

        [Theory]
        [InlineData("8.0-fpm", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("8.0-fpm", ImageTestHelperConstants.OsTypeDebianBullseye)]
        [InlineData("8.1-fpm", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("8.1-fpm", ImageTestHelperConstants.OsTypeDebianBullseye)]
        [InlineData("8.2-fpm", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("8.2-fpm", ImageTestHelperConstants.OsTypeDebianBullseye)]
        public void SqlSrv_IsInstalled(string version, string osType)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, osType),
                CommandToExecuteOnRun = "php",
                CommandArguments = new[] { "-m", " | grep pdo_sqlsrv);" }
            });

            // Assert
            var output = result.StdOut.ToString();
            RunAsserts(() =>
            {
                Assert.True(result.IsSuccess);
                Assert.Contains("pdo_sqlsrv", output);
            },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("8.1-fpm", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("8.1-fpm", ImageTestHelperConstants.OsTypeDebianBullseye)]
        [InlineData("8.2-fpm", ImageTestHelperConstants.OsTypeDebianBuster)]
        [InlineData("8.2-fpm", ImageTestHelperConstants.OsTypeDebianBullseye)]
        public void Mongodb_IsInstalled(string version, string osType)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, osType),
                CommandToExecuteOnRun = "php",
                CommandArguments = new[] { "-m", " | grep mongodb);" }
            });

            // Assert
            var output = result.StdOut.ToString();
            RunAsserts(() =>
            {
                Assert.True(result.IsSuccess);
                Assert.Contains("mongodb", output);
            },
            result.GetDebugInfo());
        }
    }
}
