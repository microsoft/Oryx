// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Oryx.Common;
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

        [Theory]
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
        public void OutOfTheBox_PlatformToolsSupportedByOryx_ShouldBeChosen(
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

        [Theory]
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
        public void OutOfTheBox_PlatformToolsSupportedByOryx_ShouldBeChosen_InSlimBuildImage(
            string executableName,
            string expectedPathPrefix)
        {
            // Arrange
            var script = new ShellScriptBuilder()
                .AddCommand($"which {executableName}")
                .ToString();

            // Act
            var image = _imageHelper.GetSlimBuildImage();
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

        [Theory]
        [InlineData("dotnet")]
        [InlineData("node")]
        [InlineData("npm")]
        [InlineData("npx")]
        [InlineData("yarn")]
        [InlineData("python")]
        [InlineData("php")]
        public void UserInstalledExecutable_IsChosenOverOryxExecutable(string executableName)
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
            var image = _imageHelper.GetBuildImage();
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

        [Theory]
        [InlineData("dotnet")]
        [InlineData("node")]
        [InlineData("npm")]
        [InlineData("npx")]
        [InlineData("yarn")]
        [InlineData("python")]
        [InlineData("php")]
        public void UserInstalledExecutable_IsChosenOverOryxExecutable_InSlimBuildImage(string executableName)
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
            var image = _imageHelper.GetSlimBuildImage();
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

        [Theory]
        [InlineData("latest")]
        [InlineData("slim")]
        public void ExecutableLookUp_FallsBackTo_OryxInstalledVersions_IfNotFoundInEarlierPaths(string tag)
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
            var image = _imageHelper.GetBuildImage(tag);
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

        [Theory]
        [InlineData("latest")]
        [InlineData("slim")]
        public void UserInstalledExecutable_TakesPrecedence_OverEnvironmentSetupByBenv(string tag)
        {
            // Arrange
            var userInstalledDotNet = "/usr/local/bin/dotnet";
            var nodeSetupByBenv = "/opt/nodejs/";
            var script = new ShellScriptBuilder()
                .AddCommand($"echo > {userInstalledDotNet}")
                .AddCommand($"chmod +x {userInstalledDotNet}")
                // The following should add a path in such a way that user installed dotnet does not get affected,
                // However the specific node version setup by benv should be picked up.
                .AddCommand("source benv dotnet=2 node=8")
                .AddCommand("which dotnet")
                .AddCommand("which node")
                .ToString();

            // Act
            var image = _imageHelper.GetBuildImage(tag);
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
        [InlineData("latest")]
        [InlineData("slim")]
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
        [InlineData("latest")]
        [InlineData("slim")]
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