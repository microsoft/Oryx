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
    public class DotnetCoreSampleAppsTest
    {
        private readonly ITestOutputHelper _output;
        private readonly DockerCli _dockerCli;
        private readonly string _hostSamplesDir;

        public DotnetCoreSampleAppsTest(ITestOutputHelper output)
        {
            _output = output;
            // Specify wait time as 'dotnet build' could take lot of time as it needs
            // to do a restore and then build.
            _dockerCli = new DockerCli(waitTimeInSeconds: (int)TimeSpan.FromMinutes(2).TotalSeconds);

            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
        }

        [Fact]
        public void CanBuild_NetCoreApp10WebApp()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                volume,
                commandToExecuteOnRun: "dotnet",
                commandArguments:
                new[]
                {
                    "build",
                    $"{volume.ContainerDir}/DotNetCore/NetCoreApp10WebApp/NetCoreApp10WebApp.csproj"
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void CanBuild_NetCoreApp21WebApp()
        {
            // Arrange
            var volume = DockerVolume.Create(_hostSamplesDir);

            // Arrange & Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                volume,
                commandToExecuteOnRun: "dotnet",
                commandArguments:
                new[]
                {
                    "build",
                    $"{volume.ContainerDir}/DotNetCore/NetCoreApp21WebApp/NetCoreApp21WebApp.csproj"
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
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
