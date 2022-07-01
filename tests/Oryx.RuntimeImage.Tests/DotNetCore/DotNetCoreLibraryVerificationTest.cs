// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    [Trait("category", "dotnetcore-runtime")]
    public class DotNetCoreLibraryVerificationTest : TestBase
    {
        public DotNetCoreLibraryVerificationTest(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData("3.1")]
        [InlineData("5.0")]
        public void GDIPlusLibrary_IsPresentInTheImage(string version)
        {
            // Arrange
            var expectedLibrary = "libgdiplus";

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("dotnetcore", version),
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", $"ldconfig -p | grep {expectedLibrary}" },
            });

            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedLibrary, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("3.1")]
        [InlineData("5.0")]
        public void DotnetMonitorTool_IsPresentInTheImage(string version)
        {
            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetRuntimeImage("dotnetcore", version),
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", $"ls opt/dotnetcore-tools/" },
            });

            // Assert
            var actualOutput = result.StdOut.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("dotnet-monitor", actualOutput);
                },
                result.GetDebugInfo());
        }
    }
}
