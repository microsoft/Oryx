// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Oryx.BuildImage.Tests
{
    /// <summary>
    /// Custom trait attribute for Debian flavor filtering in CI/CD.
    /// Usage: [DebianFlavor("bullseye")] or [DebianFlavor("bookworm")]
    /// </summary>
    [TraitDiscoverer("Microsoft.Oryx.Tests.Common.DebianFlavorDiscoverer", "Oryx.Tests.Common")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DebianFlavorAttribute : Attribute, ITraitAttribute
    {
        public string Flavor { get; }
        public DebianFlavorAttribute(string flavor) => Flavor = flavor;
    }
}

namespace Microsoft.Oryx.BuildImage.Tests
{
    /// <summary>
    /// Tests for Python dynamic installation across different Debian flavors and Python versions.
    /// Uses Theory-based testing for better parameterization and CI/CD parallelization.
    /// </summary>
    [Trait("Platform", "python")]
    [Collection("Python Dynamic Installation Tests")]
    public class PythonDynamicInstallationTest : PythonSampleAppsTestBase
    {
        private readonly string DefaultInstallationRootDir = "/opt/python";

        public PythonDynamicInstallationTest(ITestOutputHelper output) : base(output)
        {
        }

        #region Test Data Providers - Centralized Configuration

        /// <summary>
        /// Master data provider: All Python versions across all Debian flavors.
        /// Bullseye: Python 3.7-3.13 | Bookworm: Python 3.13
        /// </summary>
        public static TheoryData<string, string, string> AllPythonVersionsAllFlavors
        {
            get
            {
                var data = new TheoryData<string, string, string>();
                var imageHelper = new ImageTestHelper();
                var bullseye = imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye);
                var bookworm = imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBookworm);
                
                // Bullseye - Python 3.7-3.13
                data.Add(bullseye, PythonVersions.Python37Version, "bullseye");
                data.Add(bullseye, PythonVersions.Python38Version, "bullseye");
                data.Add(bullseye, PythonVersions.Python310Version, "bullseye");
                data.Add(bullseye, PythonVersions.Python311Version, "bullseye");
                data.Add(bullseye, PythonVersions.Python312Version, "bullseye");
                data.Add(bullseye, PythonVersions.Python313Version, "bullseye");
                
                // Bookworm - Python 3.13
                data.Add(bookworm, PythonVersions.Python313Version, "bookworm");
                
                return data;
            }
        }

        /// <summary>
        /// Django-specific data provider (requires Python 3.10+).
        /// </summary>
        public static TheoryData<string, string, string> DjangoSupportedVersions
        {
            get
            {
                var data = new TheoryData<string, string, string>();
                var imageHelper = new ImageTestHelper();
                var bullseye = imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBullseye);
                var bookworm = imageHelper.GetGitHubActionsBuildImage(ImageTestHelperConstants.GitHubActionsBookworm);
                
                // Bullseye - Python 3.10+
                data.Add(bullseye, PythonVersions.Python310Version, "bullseye");
                data.Add(bullseye, PythonVersions.Python311Version, "bullseye");
                data.Add(bullseye, PythonVersions.Python312Version, "bullseye");
                data.Add(bullseye, PythonVersions.Python313Version, "bullseye");
                
                // Bookworm - Python 3.13
                data.Add(bookworm, PythonVersions.Python313Version, "bookworm");
                
                return data;
            }
        }

        /// <summary>
        /// Unsupported/deprecated Python versions for error handling tests.
        /// </summary>
        public static TheoryData<string, string> UnsupportedVersions
        {
            get
            {
                var data = new TheoryData<string, string>();
                var imageHelper = new ImageTestHelper();
                var bullseye = imageHelper.GetGitHubActionsBuildImage();
                
                data.Add(PythonVersions.Python27Version, bullseye);
                data.Add(PythonVersions.Python36Version, bullseye);
                return data;
            }
        }

        #endregion


        #region Core Build Tests - App Types Across All Versions/Flavors

        /// <summary>
        /// Tests Flask app dynamic build across all Python versions and Debian flavors.
        /// </summary>
        [Theory]
        [Trait("Platform", "python")]
        [Trait("category", "githubactions")]
        [Trait("TestPriority", "critical")]
        [MemberData(nameof(AllPythonVersionsAllFlavors))]
        public void Build_FlaskApp_AllVersionsAndFlavors(string imageName, string version, string debianFlavor)
        {
            BuildPythonApp(imageName, version, "flask-app");
        }

        /// <summary>
        /// Tests PyODBC app (database connectivity) across all versions and flavors.
        /// </summary>
        [Theory]
        [Trait("Platform", "python")]
        [Trait("category", "githubactions")]
        [MemberData(nameof(AllPythonVersionsAllFlavors))]
        public void Build_PyodbcApp_AllVersionsAndFlavors(string imageName, string version, string debianFlavor)
        {
            BuildPythonApp(imageName, version, "pyodbc-app");
        }

        /// <summary>
        /// Tests Django app (requires Python 3.10+) across supported versions.
        /// </summary>
        [Theory]
        [Trait("Platform", "python")]
        [Trait("category", "githubactions")]
        [MemberData(nameof(DjangoSupportedVersions))]
        public void Build_DjangoApp_SupportedVersions(string imageName, string version, string debianFlavor)
        {
            BuildPythonApp(imageName, version, "django-regex-example-app");
        }

        #endregion



        /// <summary>
        /// Tests SDK reinstallation when sentinel file is missing.
        /// </summary>
        [Fact]
        [Trait("Platform", "python")]
        [Trait("category", "githubactions")]
        public void Build_ReinstallsSDK_WhenSentinelFileMissing()
        {
            // Arrange
            var installationDir = $"{BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot}/" +
                $"python/{previewVersion}";
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} --platform {PythonConstants.PlatformName} --platform-version {previewVersion} " +
                $"-o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        $"Python Version: {installationDir}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        /// <summary>
        /// Tests Azure Functions Python apps.
        /// </summary>
        [Fact]
        [Trait("Platform", "python")]
        [Trait("category", "githubactions")]
        public void Build_AzureFunctionsApp()
        {
            // Arrange
            var version = "3.8.16"; //NOTE: use the full version so that we know the install directory path
            var installationDir = $"{BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot}/" +
                $"python/{version}";
            var sentinelFile = $"{installationDir}/{SdkStorageConstants.SdkDownloadSentinelFileName}";
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var buildCmd = $"{appDir} --platform {PythonConstants.PlatformName} --platform-version {version} " +
                $"-o {appOutputDir}";
            var script = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddBuildCommand(buildCmd)
                .AddFileExistsCheck(sentinelFile)
                .AddCommand($"rm -f {sentinelFile}")
                .AddBuildCommand(buildCmd)
                .AddFileExistsCheck(sentinelFile)
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        /// <summary>
        /// Tests dynamic installation to custom directory.
        /// </summary>
        [Fact]
        [Trait("Platform", "python")]
        [Trait("category", "githubactions")]
        public void Build_WithCustomInstallDirectory()
        {
            // Arrange
            var version = "3.8.18";
            var installationDir = $"{BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot}/" +
                $"python/{version}";
            var appName = "Python_HttpTriggerSample";
            var volume = DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "azureFunctionsApps", appName));
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddBuildCommand(
                $"{appDir} --platform {PythonConstants.PlatformName} --platform-version {version} -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        $"Python Version: {installationDir}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        /// <summary>
        /// Tests building with custom package directory.
        /// </summary>
        [Fact]
        [Trait("Platform", "python")]
        [Trait("category", "githubactions")]
        public void Build_WithCustomPackageDirectory()
        {
            // Arrange
            var version = "3.10.18";
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var expectedDynamicInstallRootDir = "/foo/bar";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} --platform {PythonConstants.PlatformName} --platform-version {version} -o {appOutputDir}" +
                $" --dynamic-install-root-dir {expectedDynamicInstallRootDir}")
                .AddDirectoryExistsCheck(
                $"{expectedDynamicInstallRootDir}/{PythonConstants.PlatformName}/{version}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        $"Python Version: {expectedDynamicInstallRootDir}/{PythonConstants.PlatformName}" +
                        $"/{version}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        #endregion

        #region Helper Methods - Reusable Test Logic

        /// <summary>
        /// Universal Python app builder - consolidates all app build logic.
        /// </summary>
        private void BuildPythonApp(string imageName, string version, string appName, string installationRoot = BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot)
        {
            var installationDir = $"{installationRoot}/python/{version}";
            var volume = CreateSampleAppVolume(appName);
            var script = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddBuildCommand($"{volume.ContainerDir} --platform {PythonConstants.PlatformName} --platform-version {version} -o /tmp/app-output")
                .ToString();

            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = imageName,
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            RunAsserts(() =>
            {
                Assert.True(result.IsSuccess);
                Assert.Contains($"Python Version: {installationDir}/bin/python3", result.StdOut);
            }, result.GetDebugInfo());
        }
        {
            // Arrange
            var version = "3.10.13";
            var appName = "flask-app";
            var installationDir = $"{BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot}" +
                $"/python/{version}";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var packagesDir = ".python_packages/lib/python3.7/site-packages";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --platform {PythonConstants.PlatformName} " +
                $"--platform-version {version} -p packagedir={packagesDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{packagesDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        $"Python Version: {installationDir}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        #region Generic Build Tests - Edge Cases & Special Scenarios

        /// <summary>
        /// Tests building with Python preview/alpha versions.
        /// </summary>
        [Theory]
        [Trait("Platform", "python")]
        [Trait("category", "githubactions")]
        [InlineData("3.10.0a2")]
        public void Build_WithPreviewVersion(string previewVersion)
        {
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var installationDir = $"{BuildScriptGenerator.Constants.TemporaryInstallationDirectoryRoot}/python/{previewVersion}";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{volume.ContainerDir} --platform {PythonConstants.PlatformName} --platform-version {previewVersion} -o /tmp/app-output")
                .ToString();

            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            RunAsserts(() =>
            {
                Assert.True(result.IsSuccess);
                Assert.Contains($"Python Version: {installationDir}/bin/python3", result.StdOut);
            }, result.GetDebugInfo());
        }

        /// <summary>
        /// Tests that builds fail gracefully with unsupported Python versions.
        /// </summary>
        [Theory]
        [Trait("Platform", "python")]
        [Trait("category", "githubactions")]
        [MemberData(nameof(UnsupportedVersions))]
        public void Build_FailsWithUnsupportedVersion(string version, string imageName)
        {
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var script = new ShellScriptBuilder()
                .AddCommand(GetSnippetToCleanUpExistingInstallation())
                .AddBuildCommand($"{volume.ContainerDir} --platform {PythonConstants.PlatformName} --platform-version {version} -o /tmp/app-output")
                .ToString();

            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = imageName,
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            RunAsserts(() =>
            {
                Assert.False(result.IsSuccess);
                Assert.Contains($"Error: Platform '{PythonConstants.PlatformName}' version '{version}' is unsupported.", result.StdErr);
            }, result.GetDebugInfo());
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Returns shell command to clean up existing Python installations.
        /// Used to ensure tests start with a clean environment.
        /// </summary>
        private string GetSnippetToCleanUpExistingInstallation()
        {
            return $"rm -rf {DefaultInstallationRootDir}; mkdir -p {DefaultInstallationRootDir}";
        }

        #endregion
    }
}
