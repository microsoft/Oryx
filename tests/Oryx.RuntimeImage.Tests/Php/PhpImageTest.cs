// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
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

    public class PhpImageTest : PhpTestBase
    {
        public PhpImageTest(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture) : base(output, testTempDirTestFixture)
        {
        }

        [Theory]
        [InlineData("7.3", PhpVersions.Php73Version)]
        [InlineData("7.2", PhpVersions.Php72Version)]
        [InlineData("7.0", PhpVersions.Php70Version)]
        [InlineData("5.6", PhpVersions.Php56Version)]
        public void VersionMatchesImageName(string imageTag, string expectedPhpVersion)
        {
            // Arrange & Act
            var result = _dockerCli.Run(
                $"oryxdevmcr.azurecr.io/public/oryx/php-{imageTag}:latest",
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
        [InlineData("7.3")]
        [InlineData("7.2")]
        [InlineData("7.0")]
        [InlineData("5.6")]
        public void GraphicsExtension_Gd_IsInstalled(string imageTag)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = $"oryxdevmcr.azurecr.io/public/oryx/php-{imageTag}:latest",
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
        [InlineData("7.3")]
        [InlineData("7.2")]
        [InlineData("7.0")]
        [InlineData("5.6")]
        public async Task Check_If_Apache_IsConfigured_For_PHP(string imageTag)
        {
            // Arrange
            var appName = "imagick-example";
            var hostDir = Path.Combine(_hostSamplesDir, "php", appName);
            var volume = CreateSampleAppVolume(hostDir);
            var appDir = volume.ContainerDir;

            var testSiteConfigApache2 =
                @"<VirtualHost *:80>
                    \nServerAdmin php-x@localhost
                    \nDocumentRoot /var/www/php-x/
                    \nServerName localhost
                    \nServerAlias www.php-x.com
                    
                    \n<Directory />
                    \n    Options FollowSymLinks
                    \n    AllowOverride None
                    \n</Directory>
                    \n<Directory /var/www/php-x/>
                        Require all granted
                    \n</Directory>

                    \nErrorLog /var/www/php-x/error.log
                    \nCustomLog /var/www/php-x/access.log combined
                  </VirtualHost>";

            int containerPort = 80;
            var customSiteConfig = @"echo '" + testSiteConfigApache2 + "' > /etc/apache2/sites-available/php-x.conf";
            var portConfig = @"sed -i -e 's!\${APACHE_PORT}!" + containerPort + "!g' /etc/apache2/ports.conf /etc/apache2/sites-available/*.conf";
            var documentRootConfig = @"sed -i -e 's!\${APACHE_DOCUMENT_ROOT}!/var/www/php-x/!g' /etc/apache2/apache2.conf /etc/apache2/conf-available/*.conf /etc/apache2/sites-available/*.conf";
            var script = new ShellScriptBuilder()
                .AddCommand("mkdir -p /var/www/php-x")
                .AddCommand("echo -e '' > /var/www/php-x/error.log")
                .AddCommand("echo -e '' > /var/www/php-x/access.log")
                .AddCommand("echo -e '<?php\n phpinfo();\n ?>' > /var/www/php-x/inDex.PhP")
                .AddCommand("chmod -R +x /var/www/php-x")
                .AddCommand(documentRootConfig)
                .AddCommand(portConfig)
                .AddCommand("echo 'ServerName localhost' >> /etc/apache2/apache2.conf")
                .AddCommand(customSiteConfig)
                .AddCommand("a2ensite php-x.conf")
                .AddCommand("service apache2 restart")
                .ToString();

            // Assert
            await EndToEndTestHelper.RunAndAssertAppAsync(
                imageName: $"oryxdevmcr.azurecr.io/public/oryx/php-{imageTag}",
                output: _output,
                volumes: new List<DockerVolume> { volume },
                environmentVariables: null,
                port: containerPort,
                link: null,
                runCmd: "/bin/sh",
                runArgs: new[] { "-c", script },
                assertAction: async (hostPort) =>
                {
                    var data = await _httpClient.GetStringAsync($"http://localhost:{hostPort}/php-x/inDex.PhP");
                    Assert.DoesNotContain("<?", data);
                },
                dockerCli: _dockerCli);
        }

        [Theory]
        [InlineData("7.0")]
        [InlineData("5.6")]
        // mcrypt only exists in 5.6 and 7.0, it's deprecated from php 7.2  and newer
        public void Mcrypt_IsInstalled(string imageTag)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = $"oryxdevmcr.azurecr.io/public/oryx/php-{imageTag}:latest",
                CommandToExecuteOnRun = "php",
                CommandArguments = new[] { "-m", " | grep mcrypt);" }
            });

            // Assert
            var output = result.StdOut.ToString();
            RunAsserts(() =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("mcrypt", output);
                },
                result.GetDebugInfo());
            
        }

        [SkippableTheory]
        [InlineData("7.3")]
        [InlineData("7.2")]
        [InlineData("7.0")]
        [InlineData("5.6")]
        public void PhpRuntimeImage_Contains_VersionAndCommit_Information(string version)
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
                ImageId = "oryxdevmcr.azurecr.io/public/oryx/php-" + version + ":latest",
                CommandToExecuteOnRun = "oryx",
                CommandArguments = new[] { " " }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.NotNull(result.StdErr);
                    Assert.DoesNotContain(".unspecified, Commit: unspecified", result.StdErr);
                    Assert.Contains(gitCommitID, result.StdErr);
                    Assert.Contains(expectedOryxVersion, result.StdErr);
                },
                result.GetDebugInfo());
        }
    }
}
