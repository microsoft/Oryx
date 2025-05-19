// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class BenvTest
    {
        private ITestOutputHelper _output;
        private DockerCli _dockerCli;
        private ImageTestHelper _imageHelper;

        public BenvTest(ITestOutputHelper output)
        {
            _output = output;
            _dockerCli = new DockerCli();
            _imageHelper = new ImageTestHelper(output);
        }

        [Fact, Trait("category", "latest")]
        public void PipelineTestInvocationLatest()
        {
            InstalledNodeModulesExecutablesAreOnPath(ImageTestHelperConstants.LatestStretchTag);
            InstalledPythonExecutablesAreOnPath(ImageTestHelperConstants.LatestStretchTag);
            var imageTestHelper = new ImageTestHelper();
            BuildImagesHaveOryxPathsEnvironmentVariableAvailable(
                imageTestHelper.GetBuildImage());
        }

        [Fact, Trait("category", "jamstack")]
        public void PipelineTestInvocationJamstack()
        {
            var imageTestHelper = new ImageTestHelper();
            BuildImagesHaveOryxPathsEnvironmentVariableAvailable(
                imageTestHelper.GetAzureFunctionsJamStackBuildImage());
        }

        [Fact, Trait("category", "githubaction")]
        public void PipelineTestInvocationGithubActions()
        {
            var imageTestHelper = new ImageTestHelper();
            BuildImagesHaveOryxPathsEnvironmentVariableAvailable(
                imageTestHelper.GetGitHubActionsBuildImage());
        }

        [Theory, Trait("category", "latest")]
        // DotNet
        [InlineData("dotnet", "/opt/dotnet/")]
        // Node
        [InlineData("node", "/opt/nodejs/")]
        [InlineData("npm", "/opt/nodejs/")]
        [InlineData("npx", "/opt/nodejs/")]
        [InlineData("yarn", "/opt/yarn/")]
        [InlineData("yarnpkg", "/opt/yarn/")]
        // Python
        [InlineData("python", "/opt/python/")]
        [InlineData("pip", "/opt/python/")]
        [InlineData("pip3", "/opt/python/")]
        [InlineData("pydoc3", "/opt/python/")]
        [InlineData("wheel", "/opt/python/")]
        [InlineData("python3-config", "/opt/python/")]
        // Php
        [InlineData("php", "/opt/php/")]
        public void OutOfTheBox_PlatformToolsSupportedByOryx_ShouldBeChosen_InFullBuildImage(
            string executableName,
            string expectedPathPrefix)
        {
            // Arrange
            var script = new ShellScriptBuilder()
                .AddCommand($"which {executableName}")
                .ToString();

            // Act
            var image = _imageHelper.GetBuildImage();
            var result = _dockerCli.Run(image, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedPathPrefix, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory, Trait("category", "ltsversions")]
        // DotNet
        [InlineData("dotnet", "/opt/dotnet/")]
        // Node
        [InlineData("node", "/opt/nodejs/")]
        [InlineData("npm", "/opt/nodejs/")]
        [InlineData("npx", "/opt/nodejs/")]
        [InlineData("yarn", "/opt/yarn/")]
        [InlineData("yarnpkg", "/opt/yarn/")]
        // Python
        [InlineData("python", "/opt/python/")]
        [InlineData("pip", "/opt/python/")]
        [InlineData("pip3", "/opt/python/")]
        [InlineData("wheel", "/opt/python/")]
        [InlineData("pydoc3", "/opt/python/")]
        [InlineData("python3-config", "/opt/python/")]
        // Php
        [InlineData("php", "/opt/php/")]
        [InlineData("composer.phar", "/opt/php-composer/")]
        public void OutOfTheBox_PlatformToolsSupportedByOryx_ShouldBeChosen_InLtsVersionsBuildImage(
            string executableName,
            string expectedPathPrefix)
        {
            // Arrange
            var script = new ShellScriptBuilder()
                .AddCommand($"which {executableName}")
                .ToString();

            // Act
            var image = _imageHelper.GetLtsVersionsBuildImage();
            var result = _dockerCli.Run(image, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedPathPrefix, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory, Trait("category", "vso-focal")]
        [InlineData("dotnet")]
        [InlineData("node")]
        [InlineData("npm")]
        [InlineData("npx")]
        [InlineData("yarn")]
        [InlineData("python")]
        [InlineData("php")]
        [InlineData("ruby")]
        [InlineData("java")]
        [InlineData("mvn")]
        public void UserInstalledExecutable_IsChosenOverOryxExecutable_InVsoBuildImage(string executableName)
        {
            // Arrange
            var userInstalledExecutable = $"/usr/local/bin/{executableName}";
            var script = new ShellScriptBuilder()
                .AddLinkDoesNotExistCheck(userInstalledExecutable)
                .AddFileDoesNotExistCheck(userInstalledExecutable)
                .AddCommand($"echo > {userInstalledExecutable}")
                .AddCommand($"chmod +x {userInstalledExecutable}")
                .AddCommand($"which {executableName}")
                .ToString();

            // Act
            var image = _imageHelper.GetVsoBuildImage(ImageTestHelperConstants.VsoFocal);
            var result = _dockerCli.Run(image, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(userInstalledExecutable, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory, Trait("category", "vso-focal")]
        [InlineData("python3", "/usr/bin/python3")]
        [InlineData("pip", "/usr/local/bin/pip")]
        [InlineData("pip3", "/usr/local/bin/pip3")]
        public void DefaultVersionsOfPythonExecutablesAreUsedInVSOImage(string executableName, string expectedPath)
        {
            // Arrange
            var script = new ShellScriptBuilder()
                .AddCommand($"which {executableName}")
                .ToString();

            // Act
            var image = _imageHelper.GetVsoBuildImage(ImageTestHelperConstants.VsoFocal);
            var result = _dockerCli.Run(image, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedPath, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "vso-focal")]
        public void ExecutableLookUp_FallsBackTo_OryxInstalledVersions_IfNotFoundInEarlierPaths_InVsoImage()
        {
            // Arrange
            var userInstalledDotNet = "/usr/local/bin/dotnet";
            var oryxInstalledNode = "/opt/nodejs/";
            var script = new ShellScriptBuilder()
                .AddCommand($"echo > {userInstalledDotNet}")
                .AddCommand($"chmod +x {userInstalledDotNet}")
                .AddCommand("which dotnet")
                // The following should be picked up from Oryx install
                .AddCommand("which node")
                .ToString();

            // Act
            var image = _imageHelper.GetVsoBuildImage(ImageTestHelperConstants.VsoFocal);
            var result = _dockerCli.Run(image, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(userInstalledDotNet, result.StdOut);
                    Assert.Contains(oryxInstalledNode, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "vso-focal")]
        public void UserInstalledExecutable_TakesPrecedence_OverEnvironmentSetupByBenv_InVsoBuildImage()
        {
            // Arrange
            var userInstalledDotNet = "/usr/local/bin/dotnet";
            var nodeSetupByBenv = "/opt/nodejs/";
            var script = new ShellScriptBuilder()
                .AddCommand($"echo > {userInstalledDotNet}")
                .AddCommand($"chmod +x {userInstalledDotNet}")
                // The following should add a path in such a way that user installed dotnet does not get affected,
                // However the specific node version setup by benv should be picked up.
                .AddCommand($"source benv dotnet={DotNetCoreSdkVersions.DotNetCore31SdkVersion} node=16")
                .AddCommand("which dotnet")
                .AddCommand("which node")
                .ToString();

            // Act
            var image = _imageHelper.GetVsoBuildImage(ImageTestHelperConstants.VsoFocal);
            var result = _dockerCli.Run(image, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(userInstalledDotNet, result.StdOut);
                    Assert.Contains(nodeSetupByBenv, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData(ImageTestHelperConstants.LatestStretchTag)]
        [InlineData(ImageTestHelperConstants.LtsVersionsStretch)]
        public void InstalledNodeModulesExecutablesAreOnPath(string tag)
        {
            // Arrange
            var serveNodeModulePathPrefix = "/opt/nodejs/";
            var script = new ShellScriptBuilder()
                .AddCommand("npm install -g serve > /dev/null 2>&1")
                .AddCommand("which serve")
                .ToString();

            // Act
            var image = _imageHelper.GetBuildImage(tag);
            var result = _dockerCli.Run(image, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(serveNodeModulePathPrefix, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData(ImageTestHelperConstants.LatestStretchTag)]
        [InlineData(ImageTestHelperConstants.LtsVersionsStretch)]
        public void InstalledPythonExecutablesAreOnPath(string tag)
        {
            // Arrange
            var pythonPacakageExecutablePathPrefix = "/opt/python/";
            var script = new ShellScriptBuilder()
                .AddCommand("pip install pylint > /dev/null 2>&1")
                .AddCommand("which pylint")
                .ToString();

            // Act
            var image = _imageHelper.GetBuildImage(tag);
            var result = _dockerCli.Run(image, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(pythonPacakageExecutablePathPrefix, result.StdOut);
                },
                result.GetDebugInfo());
        }

        private void BuildImagesHaveOryxPathsEnvironmentVariableAvailable(string iamgeName)
        {
            // Please note:
            // This test method has at least 1 wrapper function that pases the imageName parameter.

            // Arrange
            var expected = "/opt/oryx:";
            var script = new ShellScriptBuilder()
                .AddCommand("echo \"$ORYX_PATHS\"")
                .ToString();

            // Act
            var result = _dockerCli.Run(iamgeName, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.StartsWith(expected, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory, Trait("category", "vso-focal")]
        // DotNet
        [InlineData("dotnet", "/opt/dotnet/")]
        // Node
        [InlineData("node", "/opt/nodejs/")]
        [InlineData("npm", "/opt/nodejs/")]
        [InlineData("npx", "/opt/nodejs/")]
        [InlineData("yarn", "/opt/yarn/")]
        [InlineData("yarnpkg", "/opt/yarn/")]
        // Python: Note that by default system installed python is available in the path
        // Php
        [InlineData("php", "/opt/php/")]
        [InlineData("composer.phar", "/opt/php-composer/")]
        // Java
        [InlineData("java", "/opt/java/")]
        [InlineData("mvn", "/opt/maven/")]
        public void OutOfTheBox_SomeToolsAreAvailableInPathOfVSOImage(
            string executableName,
            string expectedPathPrefix)
        {
            // Arrange
            var script = new ShellScriptBuilder()
                .AddCommand($"which {executableName}")
                .ToString();

            // Act
            var image = _imageHelper.GetVsoBuildImage(ImageTestHelperConstants.VsoFocal);
            var result = _dockerCli.Run(image, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedPathPrefix, result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "latest")]
        public void BenvDotNet22UsesDotNetCore22Version()
        {
            // Arrange
            var expectedPath = $"/opt/dotnet/2.2/dotnet";
            var script = new ShellScriptBuilder()
                .AddCommand($"source benv dotnet=2.2")
                .AddCommand("which dotnet")
                .ToString();

            // Act
            var image = _imageHelper.GetBuildImage();
            var result = _dockerCli.Run(image, "/bin/bash", "-c", script);

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(expectedPath, result.StdOut);
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