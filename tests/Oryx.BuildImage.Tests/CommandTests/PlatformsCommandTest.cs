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
using Microsoft.Oryx.BuildScriptGenerator.Ruby;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGeneratorCli;
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

        [Fact, Trait("category", "githubactions")]
        public void ListsPlatformsAndVersionsAvailableForDynamicInstall()
        {
            // Arrange
            var script = new ShellScriptBuilder()
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
                    Assert.True(dotNetCorePlatform.Versions.Contains("2.1.811"));
                    Assert.True(dotNetCorePlatform.Versions.Contains("5.0.408"));
                    Assert.True(dotNetCorePlatform.Versions.Contains("6.0.100-preview.7.21379.14"));
                    Assert.True(dotNetCorePlatform.Versions.Contains("7.0.409"));

                    var nodePlatform = actualResults
                        .Where(pr => pr.Name.EqualsIgnoreCase(NodeConstants.PlatformName))
                        .FirstOrDefault();
                    Assert.NotNull(nodePlatform);
                    Assert.NotNull(nodePlatform.Versions);
                    Assert.True(nodePlatform.Versions.Any());
                    Assert.True(nodePlatform.Versions.Contains("17.6.0"));

                    var pythonPlatform = actualResults
                        .Where(pr => pr.Name.EqualsIgnoreCase(PythonConstants.PlatformName))
                        .FirstOrDefault();
                    Assert.NotNull(pythonPlatform);
                    Assert.NotNull(pythonPlatform.Versions);
                    Assert.True(pythonPlatform.Versions.Any());
                    Assert.True(pythonPlatform.Versions.Contains("3.6.12"));
                    Assert.True(pythonPlatform.Versions.Contains("3.7.9"));
                    Assert.True(pythonPlatform.Versions.Contains("3.9.15"));
                    Assert.True(pythonPlatform.Versions.Contains("3.9.0"));
                    Assert.True(pythonPlatform.Versions.Contains("3.10.4"));

                    var phpPlatform = actualResults
                        .Where(pr => pr.Name.EqualsIgnoreCase(PhpConstants.PlatformName))
                        .FirstOrDefault();
                    Assert.NotNull(phpPlatform);
                    Assert.NotNull(phpPlatform.Versions);
                    Assert.True(phpPlatform.Versions.Any());
                    Assert.True(phpPlatform.Versions.Contains("8.3.7"));
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
