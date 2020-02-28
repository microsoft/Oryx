// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class PythonDynamicInstallationTest : PythonSampleAppsTestBase
    {
        private readonly string DefaultInstallationRootDir = "/opt/python";

        public PythonDynamicInstallationTest(ITestOutputHelper output) : base(output)
        {
        }

        public static TheoryData<string> ImageNameData
        {
            get
            {
                var imageTestHelper = new ImageTestHelper();
                var data = new TheoryData<string>();
                data.Add(imageTestHelper.GetTestSlimBuildImage());
                data.Add(imageTestHelper.GetGitHubActionsBuildImage());
                return data;
            }
        }

        //[Theory]
        //[MemberData(nameof(ImageNameData))]
        //public void GeneratesScript_AndBuilds(string imageName)
        //{
        //    // Arrange
        //    var version = "3.8.1";
        //    var appName = "flask-app";
        //    var volume = CreateSampleAppVolume(appName);
        //    var appDir = volume.ContainerDir;
        //    var appOutputDir = "/tmp/app-output";
        //    var script = new ShellScriptBuilder()
        //        .AddCommand(GetSnippetToCleanUpExistingInstallation())
        //        .SetEnvironmentVariable(
        //            SdkStorageConstants.SdkStorageBaseUrlKeyName,
        //            SdkStorageConstants.DevSdkStorageBaseUrl)
        //        .AddBuildCommand(
        //        $"{appDir} --platform python --platform-version {version} -o {appOutputDir} --enable-dynamic-install")
        //        .ToString();

        //    // Act
        //    var result = _dockerCli.Run(new DockerRunArguments
        //    {
        //        ImageId = imageName,
        //        EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
        //        Volumes = new List<DockerVolume> { volume },
        //        CommandToExecuteOnRun = "/bin/bash",
        //        CommandArguments = new[] { "-c", script }
        //    });

        //    // Assert
        //    RunAsserts(
        //        () =>
        //        {
        //            Assert.True(result.IsSuccess);
        //            Assert.Contains(
        //                $"Python Version: {Constants.TemporaryInstallationDirectoryRoot}/python/{version}/bin/python3",
        //                result.StdOut);
        //        },
        //        result.GetDebugInfo());
        //}

        private string GetSnippetToCleanUpExistingInstallation()
        {
            return $"rm -rf {DefaultInstallationRootDir}; mkdir -p {DefaultInstallationRootDir}";
        }
    }
}
