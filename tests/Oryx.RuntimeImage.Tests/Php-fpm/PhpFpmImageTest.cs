// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    public class PhpTestBase : TestBase, IClassFixture<TestTempDirTestFixture>
    {
        public readonly string _hostSamplesDir;
        public readonly string _tempRootDir;
        public readonly HttpClient _httpClient = new HttpClient();

        public DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "php", sampleAppName));

        public PhpTestBase(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture) : base(output)
        {
            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
            _tempRootDir = testTempDirTestFixture.RootDirPath;
        }
    }
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

        public static IEnumerable<object[]> BullseyePhpFpmVersionTestData => new[]
        {
            new object[] { "8.1-fpm", PhpVersions.Php81Version },
            new object[] { "8.2-fpm", PhpVersions.Php82Version },
            new object[] { "8.3-fpm", PhpVersions.Php83Version },
            new object[] { "8.4-fpm", PhpVersions.Php84Version },
        };

        [Theory]
        [Trait("category", "runtime-bullseye")]
        [MemberData(nameof(BullseyePhpFpmVersionTestData))]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void VersionMatchesBullseyeImageName(string version, string expectedPhpVersion)
        {
            // Arrange & Act
            var result = _dockerCli.Run(
                _imageHelper.GetRuntimeImage("php", version,  ImageTestHelperConstants.OsTypeDebianBullseye),
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

        public static IEnumerable<object[]> BookwormPhpFpmVersionTestData => new[]
        {
            new object[] { "8.3-fpm", PhpVersions.Php83Version },
            new object[] { "8.4-fpm", PhpVersions.Php84Version },
        };

        [Theory]
        [Trait("category", "runtime-bookworm")]
        [MemberData(nameof(BookwormPhpFpmVersionTestData))]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void VersionMatchesBookwormImageName(string version, string expectedPhpVersion)
        {
            // Arrange & Act
            var result = _dockerCli.Run(
                _imageHelper.GetRuntimeImage("php", version, ImageTestHelperConstants.OsTypeDebianBookworm),
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

        public static IEnumerable<object[]> NoblePhpFpmVersionTestData => new[]
        {
            new object[] { "8.5-fpm", PhpVersions.Php85Version },
        };

        [Theory]
        [Trait("category", "runtime-noble")]
        [MemberData(nameof(NoblePhpFpmVersionTestData))]
        [Trait(TestConstants.Category, TestConstants.Release)]
        public void VersionMatchesNobleImageName(string version, string expectedPhpVersion)
        {
            // Arrange & Act
            var result = _dockerCli.Run(
                _imageHelper.GetRuntimeImage("php", version, ImageTestHelperConstants.OsTypeUbuntuNoble),
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
        [Trait("category", "runtime-bullseye")]
        [InlineData("8.1-fpm")]
        [InlineData("8.2-fpm")]
        [InlineData("8.3-fpm")]
        [InlineData("8.4-fpm")]
        public void GraphicsExtension_Gd_IsInstalled_For_Bullseye(string version)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, ImageTestHelperConstants.OsTypeDebianBullseye),
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
        [Trait("category", "runtime-bookworm")]
        [InlineData("8.3-fpm")]
        [InlineData("8.4-fpm")]
        public void GraphicsExtension_Gd_IsInstalled_For_Bookworm(string version)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, ImageTestHelperConstants.OsTypeDebianBookworm),
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
        [Trait("category", "runtime-noble")]
        [InlineData("8.5-fpm")]
        public void GraphicsExtension_Gd_IsInstalled_For_Noble(string version)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, ImageTestHelperConstants.OsTypeUbuntuNoble),
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

        [SkippableTheory]
        [Trait("category", "runtime-bullseye")]
        [InlineData("8.1-fpm")]
        [InlineData("8.2-fpm")]
        [InlineData("8.3-fpm")]
        [InlineData("8.4-fpm")]
        public void PhpFpmBullseyeRuntimeImage_Contains_VersionAndCommit_Information(string version)
        {
            // we cant always rely on gitcommitid as env variable in case build context is not correctly passed
            // so we should check agent_os environment variable to know if the build is happening in azure devops agent
            // or locally, locally we need to skip this test
            var agentOS = Environment.GetEnvironmentVariable("AGENT_OS");
            Skip.If(string.IsNullOrEmpty(agentOS));

            // Arrange
            var gitCommitID = GitHelper.GetCommitID();
            var buildNumber = Environment.GetEnvironmentVariable("IMAGE_BUILDNUMBER");
            var expectedOryxVersion = string.Concat(Settings.OryxVersion, buildNumber);

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, ImageTestHelperConstants.OsTypeDebianBullseye),
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

        [SkippableTheory]
        [Trait("category", "runtime-bookworm")]
        [InlineData("8.3-fpm")]
        [InlineData("8.4-fpm")]
        public void PhpFpmBookwormRuntimeImage_Contains_VersionAndCommit_Information(string version)
        {
            // we cant always rely on gitcommitid as env variable in case build context is not correctly passed
            // so we should check agent_os environment variable to know if the build is happening in azure devops agent
            // or locally, locally we need to skip this test
            var agentOS = Environment.GetEnvironmentVariable("AGENT_OS");
            Skip.If(string.IsNullOrEmpty(agentOS));

            // Arrange
            var gitCommitID = GitHelper.GetCommitID();
            var buildNumber = Environment.GetEnvironmentVariable("IMAGE_BUILDNUMBER");
            var expectedOryxVersion = string.Concat(Settings.OryxVersion, buildNumber);

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, ImageTestHelperConstants.OsTypeDebianBookworm),
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

        [SkippableTheory]
        [Trait("category", "runtime-noble")]
        [InlineData("8.5-fpm")]
        public void PhpFpmNobleRuntimeImage_Contains_VersionAndCommit_Information(string version)
        {
            // we cant always rely on gitcommitid as env variable in case build context is not correctly passed
            // so we should check agent_os environment variable to know if the build is happening in azure devops agent
            // or locally, locally we need to skip this test
            var agentOS = Environment.GetEnvironmentVariable("AGENT_OS");
            Skip.If(string.IsNullOrEmpty(agentOS));

            // Arrange
            var gitCommitID = GitHelper.GetCommitID();
            var buildNumber = Environment.GetEnvironmentVariable("IMAGE_BUILDNUMBER");
            var expectedOryxVersion = string.Concat(Settings.OryxVersion, buildNumber);

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, ImageTestHelperConstants.OsTypeUbuntuNoble),
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
        [Trait("category", "runtime-bullseye")]
        [InlineData("8.1-fpm")]
        [InlineData("8.2-fpm")]
        [InlineData("8.3-fpm")]
        [InlineData("8.4-fpm")]
        public void Redis_IsInstalled_For_Bullseye(string version)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, ImageTestHelperConstants.OsTypeDebianBullseye),
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
        [Trait("category", "runtime-bookworm")]
        [InlineData("8.3-fpm")]
        [InlineData("8.4-fpm")]
        public void Redis_IsInstalled_For_Bookworm(string version)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, ImageTestHelperConstants.OsTypeDebianBookworm),
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
        [Trait("category", "runtime-noble")]
        [InlineData("8.5-fpm")]
        public void Redis_IsInstalled_For_Noble(string version)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, ImageTestHelperConstants.OsTypeUbuntuNoble),
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
        [Trait("category", "runtime-bullseye")]
        [InlineData("8.1-fpm")]
        [InlineData("8.2-fpm")]
        [InlineData("8.3-fpm")]
        [InlineData("8.4-fpm")]
        public void SqlSrv_IsInstalled_For_Bullseye(string version)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, ImageTestHelperConstants.OsTypeDebianBullseye),
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
        [Trait("category", "runtime-bookworm")]
        [InlineData("8.3-fpm")]
        [InlineData("8.4-fpm")]
        public void SqlSrv_IsInstalled_For_Bookworm(string version)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, ImageTestHelperConstants.OsTypeDebianBookworm),
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
        [Trait("category", "runtime-noble")]
        [InlineData("8.5-fpm")]
        public void SqlSrv_IsInstalled_For_Noble(string version)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, ImageTestHelperConstants.OsTypeUbuntuNoble),
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
        [Trait("category", "runtime-bullseye")]
        [InlineData("8.1-fpm")]
        [InlineData("8.2-fpm")]
        [InlineData("8.3-fpm")]
        [InlineData("8.4-fpm")]
        public void Mongodb_IsInstalled_For_Bullseye(string version)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, ImageTestHelperConstants.OsTypeDebianBullseye),
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

        [Theory]
        [Trait("category", "runtime-bookworm")]
        [InlineData("8.3-fpm")]
        [InlineData("8.4-fpm")]
        public void Mongodb_IsInstalled_For_Bookworm(string version)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, ImageTestHelperConstants.OsTypeDebianBookworm),
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

        [Theory]
        [Trait("category", "runtime-noble")]
        [InlineData("8.5-fpm")]
        public void Mongodb_IsInstalled_For_Noble(string version)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("php", version, ImageTestHelperConstants.OsTypeUbuntuNoble),
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

        [Fact]
        [Trait("category", "runtime-resolute")]
        public void SqlServerDrivers_ReadIniSettings_For_Resolute()
        {
            var result = _dockerCli.Run(
                _imageHelper.GetRuntimeImage("php", "8.6-fpm", ImageTestHelperConstants.OsTypeUbuntuResolute),
                "php",
                new[]
                {
                    "-d", "sqlsrv.WarningsReturnAsErrors=0",
                    "-d", "sqlsrv.LogSeverity=4",
                    "-d", "sqlsrv.LogSubsystems=2",
                    "-d", "sqlsrv.ClientBufferMaxKBSize=2048",
                    "-r",
                    "echo json_encode([" +
                    "'drivers' => PDO::getAvailableDrivers()," +
                    "'warnings' => sqlsrv_get_config('WarningsReturnAsErrors')," +
                    "'severity' => sqlsrv_get_config('LogSeverity')," +
                    "'subsystems' => sqlsrv_get_config('LogSubsystems')," +
                    "'buffer' => sqlsrv_get_config('ClientBufferMaxKBSize')]);",
                });

            RunAsserts(() =>
            {
                Assert.True(result.IsSuccess);
                var settings = JObject.Parse(result.StdOut);
                Assert.Contains("sqlsrv", settings["drivers"].ToObject<string[]>());
                Assert.False((bool)settings["warnings"]);
                Assert.Equal(4, (int)settings["severity"]);
                Assert.Equal(2, (int)settings["subsystems"]);
                Assert.Equal(2048, (int)settings["buffer"]);
            }, result.GetDebugInfo());
        }

        [Fact]
        [Trait("category", "runtime-resolute")]
        public void OptionalImap_ParsesMail_For_Resolute()
        {
            var result = _dockerCli.Run(
                _imageHelper.GetRuntimeImage("php", "8.6-fpm", ImageTestHelperConstants.OsTypeUbuntuResolute),
                "php",
                new[]
                {
                    "-d", "extension=imap",
                    "-r",
                    "echo json_encode([" +
                    "'subject' => imap_utf8('=?UTF-8?B?T3J5eA==?=')," +
                    "'mailbox' => imap_rfc822_parse_adrlist('Oryx <oryx@example.test>', 'example.test')[0]->mailbox]);",
                });

            RunAsserts(() =>
            {
                Assert.True(result.IsSuccess);
                var mail = JObject.Parse(result.StdOut);
                Assert.Equal("Oryx", (string)mail["subject"]);
                Assert.Equal("oryx", (string)mail["mailbox"]);
            }, result.GetDebugInfo());
        }

        [Theory]
        [Trait("category", "runtime-resolute")]
        [InlineData("pear", "version", "PEAR Version:")]
        [InlineData("pecl", "version", "PEAR Version:")]
        [InlineData("phpdbg", "--version", "phpdbg")]
        public void DevelopmentTools_AreAvailable_For_Resolute(string command, string argument, string expectedOutput)
        {
            var result = _dockerCli.Run(
                _imageHelper.GetRuntimeImage("php", "8.6-fpm", ImageTestHelperConstants.OsTypeUbuntuResolute),
                command,
                new[] { argument });

            RunAsserts(() =>
            {
                Assert.True(result.IsSuccess);
                Assert.Contains(expectedOutput, result.StdOut);
            }, result.GetDebugInfo());
        }
    }
}
