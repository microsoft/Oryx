﻿// --------------------------------------------------------------------------------------------
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
                .AddDefaultTestEnvironmentVariables()
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
                    Assert.True(dotNetCorePlatform.Versions.Contains("5.0.0-preview.3.20214.6"));
                    Assert.True(dotNetCorePlatform.Versions.Contains("6.0.0-rc.1.21451.13"));
                    Assert.True(dotNetCorePlatform.Versions.Contains("7.0.0-preview.1.22076.8"));

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
                    Assert.True(pythonPlatform.Versions.Contains("3.8.4rc1"));
                    Assert.True(pythonPlatform.Versions.Contains("3.9.0b1"));
                    Assert.True(pythonPlatform.Versions.Contains("3.9.0"));

                    var phpPlatform = actualResults
                        .Where(pr => pr.Name.EqualsIgnoreCase(PhpConstants.PlatformName))
                        .FirstOrDefault();
                    Assert.NotNull(phpPlatform);
                    Assert.NotNull(phpPlatform.Versions);
                    Assert.True(phpPlatform.Versions.Any());
                    Assert.True(phpPlatform.Versions.Contains("8.1.6"));

                    var hugoPlatform = actualResults
                        .Where(pr => pr.Name.EqualsIgnoreCase(HugoConstants.PlatformName))
                        .FirstOrDefault();
                    Assert.NotNull(hugoPlatform);
                    Assert.NotNull(hugoPlatform.Versions);
                    Assert.True(hugoPlatform.Versions.Any());
                    Assert.True(hugoPlatform.Versions.Contains(HugoConstants.Version));

                    var rubyPlatform = actualResults
                        .Where(pr => pr.Name.EqualsIgnoreCase(RubyConstants.PlatformName))
                        .FirstOrDefault();
                    Assert.NotNull(rubyPlatform);
                    Assert.NotNull(rubyPlatform.Versions);
                    Assert.True(rubyPlatform.Versions.Any());
                    Assert.True(rubyPlatform.Versions.Contains("2.6.6"));
                    Assert.True(rubyPlatform.Versions.Contains("2.7.1"));
                    Assert.True(rubyPlatform.Versions.Contains("3.0.3"));
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
