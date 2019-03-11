// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.RuntimeImage.Tests
{
    public class PythonImagesTest : TestBase
    {
        public PythonImagesTest(ITestOutputHelper output) : base(output)
        {
        }

        [SkippableTheory]
        [InlineData("2.7")]
        [InlineData("3.6")]
        [InlineData("3.7")]
        public void PythonRuntimeImage_Contains_VersionAndCommit_Information(string version)
        {
            var agentOS = Environment.GetEnvironmentVariable("AGENT_OS");
            var gitCommitID = Environment.GetEnvironmentVariable("BUILD_SOURCEVERSION");
            var buildNumber = Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");
            var expectedOryxVersion = string.Concat(Settings.OryxVersion, buildNumber);

            // we cant always rely on gitcommitid as env variable in case build context is not correctly passed
            // so we should check agent_os environment variable to know if the build is happening in azure devops agent 
            // or locally, locally we need to skip this test
            Skip.If(string.IsNullOrEmpty(agentOS));
            // Act
            var result = _dockerCli.Run(
                "oryxdevms/python-" + version + ":latest",
                commandToExecuteOnRun: "oryx",
                commandArguments: new[] { "--version" });
            // Assert
            RunAsserts(
                () =>
                {
                    Assert.False(result.IsSuccess);
                    Assert.NotNull(result.Error);
                    Assert.DoesNotContain(".unspecified, Commit: unspecified", result.Error);
                    Assert.Contains(gitCommitID, result.Error);
                    Assert.Contains(expectedOryxVersion, result.Error);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("3.6", "Python " + Settings.Python36Version)]
        [InlineData("3.7", "Python " + Common.PythonVersions.Python37Version)]
        public void PythonVersionMatchesImageName(string pythonVersion, string expectedOutput)
        {
            // Arrange & Act
            var result = _dockerCli.Run(
                "oryxdevms/python-" + pythonVersion + ":latest",
                commandToExecuteOnRun: "python",
                commandArguments: new[] { "--version" });

            // Assert
            var actualOutput = result.Output.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Python2MatchesImageName()
        {
            string pythonVersion = "2.7";
            string expectedOutput = "Python " + Settings.Python27Version;

            // Arrange & Act
            var result = _dockerCli.Run(
                "oryxdevms/python-" + pythonVersion + ":latest",
                commandToExecuteOnRun: "python",
                commandArguments: new[] { "--version" });

            // Assert
            var actualOutput = result.Error.ReplaceNewLine();
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    //bugs.python.org >> issue18338 weird but true, earlier than python 3.4 
                    // sends python --version output to STDERR
                    Assert.Equal(expectedOutput, actualOutput);
                },
                result.GetDebugInfo());
        }
    }
}
