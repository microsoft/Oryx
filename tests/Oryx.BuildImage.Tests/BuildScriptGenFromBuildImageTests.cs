// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using Oryx.Tests.Infrastructure;
using Xunit;

namespace Oryx.BuildImage.Tests
{
    public class BuildScriptGenFromBuildImageTests
    {
        [Fact]
        public void BuildScriptGenIsIncludedInBuildImage()
        {
            // Arrange & Act
            var docker = new DockerCli();
            var result = docker.Run(
                imageId: BuildImageTestSettings.BuildImageName,
                commandToExecuteOnRun: "/opt/buildscriptgen/GenerateBuildScript");

            // Assert
            Assert.Contains("Usage:", result.Output);
        }
    }
}