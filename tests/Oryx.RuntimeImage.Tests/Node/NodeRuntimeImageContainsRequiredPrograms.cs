﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    public class NodeRuntimeImageContainsRequiredPrograms : NodeRuntimeImageTestBase
    {
        public NodeRuntimeImageContainsRequiredPrograms(
            ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
        }

        // [Theory]
        // [Trait("category", "runtime-buster")]
        // [MemberData(nameof(TestValueGenerator.GetBusterNodeVersions), MemberType = typeof(TestValueGenerator))]
        // public void NodeBusterImage_Contains_RequiredPrograms(string version, string osType)
        // {
        //     // Arrange & Act
        //     var result = _dockerCli.Run(new DockerRunArguments
        //     {
        //         ImageId = _imageHelper.GetRuntimeImage("node", version, osType),
        //         CommandToExecuteOnRun = "/bin/sh",
        //         CommandArguments = new[]
        //         {
        //             "-c",
        //             "which tar && which unzip && which pm2 && cd /opt/node-wrapper && node --version"
        //         }
        //     });

        //     // Assert
        //     RunAsserts(() => Assert.True(result.IsSuccess), result.GetDebugInfo());
        // }

        [Theory]
        [Trait("category", "runtime-bullseye")]
        [MemberData(nameof(TestValueGenerator.GetBullseyeNodeVersions), MemberType = typeof(TestValueGenerator))]
        public void NodeBullseyeImage_Contains_RequiredPrograms(string version, string osType)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", version, osType),
                CommandToExecuteOnRun = "/bin/sh",
                CommandArguments = new[]
                {
                    "-c",
                    "which tar && which unzip && which pm2 && cd /opt/node-wrapper && node --version"
                }
            });

            // Assert
            RunAsserts(() => Assert.True(result.IsSuccess), result.GetDebugInfo());
        }

        [Theory]
        [Trait("category", "runtime-bookworm")]
        [MemberData(nameof(TestValueGenerator.GetBookwormNodeVersions), MemberType = typeof(TestValueGenerator))]
        public void NodeBookwormImage_Contains_RequiredPrograms(string version, string osType)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", version, osType),
                CommandToExecuteOnRun = "/bin/sh",
                CommandArguments = new[]
                {
                    "-c",
                    "which tar && which unzip && which pm2 && cd /opt/node-wrapper && node --version"
                }
            });

            // Assert
            RunAsserts(() => Assert.True(result.IsSuccess), result.GetDebugInfo());
        }

        // [Theory]
        // [Trait("category", "runtime-buster")]
        // [InlineData("14")]
        // public void Node14BusterImage_Contains_PM2(string version)
        // {
        //     // Arrange & Act
        //     var result = _dockerCli.Run(new DockerRunArguments
        //     {
        //         ImageId = _imageHelper.GetRuntimeImage("node", version, ImageTestHelperConstants.OsTypeDebianBuster),
        //         CommandToExecuteOnRun = "/bin/sh",
        //         CommandArguments = new[]
        //         {
        //             "-c",
        //             "which pm2"
        //         }
        //     });

        //     // Assert
        //     RunAsserts(() => Assert.True(result.IsSuccess), result.GetDebugInfo());
        // }

        // [Theory]
        // [Trait("category", "runtime-bullseye")]
        // [InlineData("14")]
        // public void Node14BullseyeImage_Contains_PM2(string version)
        // {
        //     // Arrange & Act
        //     var result = _dockerCli.Run(new DockerRunArguments
        //     {
        //         ImageId = _imageHelper.GetRuntimeImage("node", version, ImageTestHelperConstants.OsTypeDebianBullseye),
        //         CommandToExecuteOnRun = "/bin/sh",
        //         CommandArguments = new[]
        //         {
        //             "-c",
        //             "which pm2"
        //         }
        //     });

        //     // Assert
        //     RunAsserts(() => Assert.True(result.IsSuccess), result.GetDebugInfo());
        // }

        // [Theory]
        // [Trait("category", "runtime-buster")]
        // [MemberData(nameof(TestValueGenerator.GetBusterNodeVersions), MemberType = typeof(TestValueGenerator))]
        // public void NodeBusterImage_Contains_ApplicationInsights(string version, string osType)
        // {
        //     // Arrange & Act
        //     var expectedAppInsightsVersion = string.Concat("applicationinsights@", NodeVersions.NodeAppInsightsSdkVersion);
        //     var result = _dockerCli.Run(new DockerRunArguments
        //     {
        //         ImageId = _imageHelper.GetRuntimeImage("node", version, osType),
        //         CommandToExecuteOnRun = "/bin/sh",
        //         CommandArguments = new[]
        //         {
        //             "-c",
        //             "npm list -g applicationinsights"
        //         }
        //     });

        //     var actualOutput = result.StdOut.ReplaceNewLine();

        //     // Assert
        //     RunAsserts(
        //         () =>
        //         {
        //             Assert.True(result.IsSuccess);
        //             Assert.Contains(expectedAppInsightsVersion, actualOutput);
        //             Assert.Contains("/usr/local/lib", actualOutput);
        //         },
        //         result.GetDebugInfo());
        // }

        [Theory]
        [Trait("category", "runtime-bullseye")]
        [MemberData(nameof(TestValueGenerator.GetBullseyeNodeVersions), MemberType = typeof(TestValueGenerator))]
        public void NodeBullseyeImage_Contains_ApplicationInsights(string version, string osType)
        {
            // Arrange & Act
            var expectedAppInsightsVersion = string.Concat("applicationinsights@", NodeVersions.NodeAppInsightsSdkVersion);
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", version, osType),
                CommandToExecuteOnRun = "/bin/sh",
                CommandArguments = new[]
                {
                    "-c",
                    "npm list -g applicationinsights"
                }
            });

            var actualOutput = result.StdOut.ReplaceNewLine();

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedAppInsightsVersion, actualOutput);
                    Assert.Contains("/usr/local/lib", actualOutput);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [Trait("category", "runtime-bookworm")]
        [MemberData(nameof(TestValueGenerator.GetBookwormNodeVersions), MemberType = typeof(TestValueGenerator))]
        public void NodeBookwormImage_Contains_ApplicationInsights(string version, string osType)
        {
            // Arrange & Act
            var expectedAppInsightsVersion = string.Concat("applicationinsights@", NodeVersions.NodeAppInsightsSdkVersion);
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", version, osType),
                CommandToExecuteOnRun = "/bin/sh",
                CommandArguments = new[]
                {
                    "-c",
                    "npm list -g applicationinsights"
                }
            });

            var actualOutput = result.StdOut.ReplaceNewLine();

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedAppInsightsVersion, actualOutput);
                    Assert.Contains("/usr/local/lib", actualOutput);
                },
                result.GetDebugInfo());
        }

        // [Theory]
        // [Trait("category", "runtime-buster")]
        // [MemberData(nameof(TestValueGenerator.GetBusterNodeVersions), MemberType = typeof(TestValueGenerator))]
        // public void NodeBusterImages_Contains_Correct_NPM_Version(string version, string osType)
        // {
        //     // Arrange & Act
        //     var result = _dockerCli.Run(new DockerRunArguments
        //     {
        //         ImageId = _imageHelper.GetRuntimeImage("node", version, osType),
        //         CommandToExecuteOnRun = "/bin/sh",
        //         CommandArguments = new[]
        //         {
        //             "-c",
        //             "npm -v"
        //         }
        //     });

        //     // Assert
        //     RunAsserts(
        //         () =>
        //         {
        //             Assert.True(result.IsSuccess);
        //             Assert.Contains(NodeVersions.NpmVersion, result.StdOut.ReplaceNewLine());
        //         },
        //         result.GetDebugInfo());
        // }

        [Theory]
        [Trait("category", "runtime-bullseye")]
        [MemberData(nameof(TestValueGenerator.GetBullseyeNodeVersions), MemberType = typeof(TestValueGenerator))]
        public void NodeBullseyeImages_Contains_Correct_NPM_Version(string version, string osType)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", version, osType),
                CommandToExecuteOnRun = "/bin/sh",
                CommandArguments = new[]
                {
                    "-c",
                    "npm -v"
                }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    if (version == "18" || version == "20" || version == "22")
                    {
                        Assert.Contains("10.7.0", result.StdOut.ReplaceNewLine());
                    }
                    else
                    {
                        Assert.Contains(NodeVersions.NpmVersion, result.StdOut.ReplaceNewLine());
                    }
                },
                result.GetDebugInfo());
        }

        [Theory]
        [Trait("category", "runtime-bookworm")]
        [MemberData(nameof(TestValueGenerator.GetBookwormNodeVersions), MemberType = typeof(TestValueGenerator))]
        public void NodeBookwormImages_Contains_Correct_NPM_Version(string version, string osType)
        {
            // Arrange & Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("node", version, osType),
                CommandToExecuteOnRun = "/bin/sh",
                CommandArguments = new[]
                {
                    "-c",
                    "npm -v"
                }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    if (version == "18" || version == "20" || version == "22")
                    {
                        Assert.Contains("10.7.0", result.StdOut.ReplaceNewLine());
                    }
                    else
                    {
                        Assert.Contains(NodeVersions.NpmVersion, result.StdOut.ReplaceNewLine());
                    }
                },
                result.GetDebugInfo());
        }
    }
}
