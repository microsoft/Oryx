// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Hugo;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Common.Extensions;
using Microsoft.Oryx.Tests.Common;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class PlatformsCommandTest : SampleAppsTestBase
    {
        public PlatformsCommandTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void ListsPlatformsAndVersionsAvailableForDynamicInstall()
        {
            // Arrange
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                // get in json format so that it can be deserialized and verified
                .AddCommand("oryx platforms --json")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            var actualResults = JsonConvert.DeserializeObject<List<PlatformResult>>(result.StdOut);
            RunAsserts(
                () =>
                {
                    Assert.NotNull(actualResults);
                    var dotNetCorePlatform = actualResults
                        .Where(pr => pr.Name.EqualsIgnoreCase(DotNetCoreConstants.PlatformName))
                        .FirstOrDefault();
                    Assert.NotNull(dotNetCorePlatform);
                    Assert.NotNull(dotNetCorePlatform.Versions);
                    Assert.True(dotNetCorePlatform.Versions.Any());
                    Assert.True(dotNetCorePlatform.Versions.Contains("1.1.13"));

                    var nodePlatform = actualResults
                        .Where(pr => pr.Name.EqualsIgnoreCase(NodeConstants.PlatformName))
                        .FirstOrDefault();
                    Assert.NotNull(nodePlatform);
                    Assert.NotNull(nodePlatform.Versions);
                    Assert.True(nodePlatform.Versions.Any());
                    Assert.True(nodePlatform.Versions.Contains("4.4.7"));

                    var pythonPlatform = actualResults
                        .Where(pr => pr.Name.EqualsIgnoreCase(PythonConstants.PlatformName))
                        .FirstOrDefault();
                    Assert.NotNull(pythonPlatform);
                    Assert.NotNull(pythonPlatform.Versions);
                    Assert.True(pythonPlatform.Versions.Any());
                    Assert.True(pythonPlatform.Versions.Contains("2.7.17"));

                    var phpPlatform = actualResults
                        .Where(pr => pr.Name.EqualsIgnoreCase(PhpConstants.PlatformName))
                        .FirstOrDefault();
                    Assert.NotNull(phpPlatform);
                    Assert.NotNull(phpPlatform.Versions);
                    Assert.True(phpPlatform.Versions.Any());
                    Assert.True(phpPlatform.Versions.Contains("5.6.40"));

                    var hugoPlatform = actualResults
                        .Where(pr => pr.Name.EqualsIgnoreCase(HugoConstants.PlatformName))
                        .FirstOrDefault();
                    Assert.NotNull(hugoPlatform);
                    Assert.NotNull(hugoPlatform.Versions);
                    Assert.True(hugoPlatform.Versions.Any());
                    Assert.True(hugoPlatform.Versions.Contains(HugoConstants.Version));
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void ListsPlatformsAndVersionsAvailable_OutputToFileOption()
        {
            // Arrange
            var outputFile = "/tmp/outputfile.txt";
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    SdkStorageConstants.SdkStorageBaseUrlKeyName,
                    SdkStorageConstants.DevSdkStorageBaseUrl)
                // get in json format so that it can be deserialized and verified
                .AddCommand($"oryx platforms --output {outputFile}")
                .AddFileExistsCheck($"{outputFile}")
                .AddCommand($"cat {outputFile}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            var OutputFileResults = result.StdOut;
            RunAsserts(
                () =>
                {
                    Assert.NotNull(OutputFileResults);
                    Assert.Contains("python", OutputFileResults);
                    Assert.Contains("php", OutputFileResults);
                    Assert.Contains("nodejs", OutputFileResults);
                    Assert.Contains("dotnet", OutputFileResults);
                    Assert.Contains("hugo", OutputFileResults);
                },
                result.GetDebugInfo());
        }

        private class PlatformResult
        {
            public string Name { get; set; }

            public IList<string> Versions { get; set; }

            public IDictionary<string, string> Properties { get; set; }
        }
    }
}
