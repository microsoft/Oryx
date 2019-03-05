using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class PhpSampleAppsTest : SampleAppsTestBase
    {
        private readonly ITestOutputHelper _output;
        private readonly DockerCli _dockerCli = new DockerCli();

        public PhpSampleAppsTest(ITestOutputHelper output) : base(output)
        {
        }

        private DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.Create(Path.Combine(_hostSamplesDir, "php", sampleAppName));

        public override void Builds_AndCopiesContentToOutputDirectory_Recursively()
        {
            throw new NotImplementedException();
        }

        public override void Build_CopiesOutput_ToNestedOutputDirectory()
        {
            throw new NotImplementedException();
        }

        public override void Build_ReplacesContentInDestinationDir_WhenDestinationDirIsNotEmpty()
        {
            throw new NotImplementedException();
        }

        public override void CanBuild_UsingScriptGeneratedBy_ScriptOnlyOption()
        {
            throw new NotImplementedException();
        }

        public override void ErrorDuringBuild_ResultsIn_NonSuccessfulExitCode()
        {
            throw new NotImplementedException();
        }

        public override void GeneratesScriptAndBuilds_WhenDestination_IsSubDirectoryOfSource()
        {
            throw new NotImplementedException();
        }

        public override void GeneratesScriptAndBuilds_WhenSourceAndDestinationFolders_AreSame()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public override void GeneratesScript_AndBuilds()
        {
            // Arrange
            var volume = CreateSampleAppVolume("templating");
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("templating"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments: new[] { "-c", script });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        public override void GeneratesScript_AndBuilds_UsingSuppliedIntermediateDir()
        {
            throw new NotImplementedException();
        }

        public override void GeneratesScript_AndBuilds_WhenExplicitLanguageAndVersion_AreProvided()
        {
            throw new NotImplementedException();
        }
    }
}
