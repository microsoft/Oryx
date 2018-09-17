// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Oryx.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.BuildImage.Tests
{
    public class NodeJSSampleAppsTest
    {
        private readonly ITestOutputHelper _output;
        private readonly DockerCli _dockerCli;
        private readonly string _hostSamplesDir;

        public NodeJSSampleAppsTest(ITestOutputHelper output)
        {
            _output = output;

            _dockerCli = new DockerCli(waitTimeInSeconds: (int)TimeSpan.FromMinutes(2).TotalSeconds);

            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
        }

        [Fact]
        public void CanBuild_NodeJSSampleApp()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);

            // Act
            var result = _dockerCli.Run(
                "oryxdevms/build:latest",
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    "\"/opt/buildscriptgen/GenerateBuildScript " + $"{volume.ContainerDir}/nodejs/webfrontend " 
                    + $"{volume.ContainerDir}/nodejs/build.sh && " + $"{volume.ContainerDir}/nodejs/build.sh && ls " 
                    + $"{volume.ContainerDir}/nodejs/webfrontend\""
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("node_modules", result.Output); // to see if the build actually happened
                },
                result.GetDebugInfo());
        }

        private void RunAsserts(Action action, string message)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                _output.WriteLine(message);
                throw;
            }
        }
    }
}
