// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildImage.Tests;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.BuildImage.Tests
{
    public class ExtensibilityModelBuildScriptTest : SampleAppsTestBase
    {
        private DockerVolume CreateWebFrontEndVolume() => DockerVolume.CreateMirror(
            Path.Combine(_hostSamplesDir, "nodejs", "webfrontend"));

        private readonly string TempDirectoryPath = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString());

        private void CreateTempDirectory() => Directory.CreateDirectory(TempDirectoryPath);

        private void CreateExtensibilityConfigFile(string content) =>
            File.WriteAllText(Path.Join(TempDirectoryPath, FilePaths.ExtensibleConfigurationFileName), content);

        private DockerVolume CreateTempDirectoryVolume() => DockerVolume.CreateMirror(TempDirectoryPath);

        private void DeleteTempDirectory() => Directory.Delete(TempDirectoryPath, true);

        public ExtensibilityModelBuildScriptTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact, Trait("category", "githubactions")]
        public void BuildScriptGenerated_WithExtensibilityModel_WithEnv_Succeeds()
        {
            // Arrange
            var configContent = @"
base-os: debian
env: 
  - name: FOO
    value: BAR
";

            var volume = CreateWebFrontEndVolume();
            CreateTempDirectory();
            try
            {
                CreateExtensibilityConfigFile(configContent);
                var tempDirVolume = CreateTempDirectoryVolume();
                var appDir = volume.ContainerDir;
                var tempDir = tempDirVolume.ContainerDir;
                var script = new ShellScriptBuilder()
                    .AddCommand($"cp {tempDir}/{FilePaths.ExtensibleConfigurationFileName} {appDir}")
                    .AddCommand($"oryx build-script {appDir}")
                    .ToString();

                // Act
                var result = _dockerCli.Run(new DockerRunArguments
                {
                    ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                    Volumes = new List<DockerVolume> { volume, tempDirVolume },
                    CommandToExecuteOnRun = "/bin/bash",
                    CommandArguments = new[] { "-c", script }
                });

                // Assert
                RunAsserts(
                    () =>
                    {
                        Assert.True(result.IsSuccess);
                        Assert.Contains(
                            $"export FOO=\"BAR\"",
                            result.StdOut);
                    },
                    result.GetDebugInfo());
            }
            finally
            {
                DeleteTempDirectory();
            }
        }

        [Fact, Trait("category", "githubactions")]
        public void BuildScriptGenerated_WithExtensibilityModel_WithSinglePrebuildScript_Succeeds()
        {
            // Arrange
            var configContent = @"
base-os: debian
pre-build: 
  - description: 'Test description'
    scripts:
      - 'echo ""Hello, world!""'
";

            var volume = CreateWebFrontEndVolume();
            CreateTempDirectory();
            try
            {
                CreateExtensibilityConfigFile(configContent);
                var tempDirVolume = CreateTempDirectoryVolume();
                var appDir = volume.ContainerDir;
                var tempDir = tempDirVolume.ContainerDir;
                var script = new ShellScriptBuilder()
                    .AddCommand($"cp {tempDir}/{FilePaths.ExtensibleConfigurationFileName} {appDir}")
                    .AddCommand($"oryx build-script {appDir}")
                    .ToString();

                // Act
                var result = _dockerCli.Run(new DockerRunArguments
                {
                    ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                    Volumes = new List<DockerVolume> { volume, tempDirVolume },
                    CommandToExecuteOnRun = "/bin/bash",
                    CommandArguments = new[] { "-c", script }
                });

                // Assert
                RunAsserts(
                    () =>
                    {
                        Assert.True(result.IsSuccess);
                        Assert.Contains(
                            $"echo \"Hello, world!\"",
                            result.StdOut);
                    },
                    result.GetDebugInfo());
            }
            finally
            {
                DeleteTempDirectory();
            }
        }

        [Fact, Trait("category", "githubactions")]
        public void BuildScriptGenerated_WithExtensibilityModel_WithMultiplePrebuildScripts_Succeeds()
        {
            // Arrange
            var configContent = @"
base-os: debian
pre-build: 
  - description: 'Test description'
    scripts:
      - 'echo ""Hello, world!""'
      - 'echo ""Hello, foobar!""'
";

            var volume = CreateWebFrontEndVolume();
            CreateTempDirectory();
            try
            {
                CreateExtensibilityConfigFile(configContent);
                var tempDirVolume = CreateTempDirectoryVolume();
                var appDir = volume.ContainerDir;
                var tempDir = tempDirVolume.ContainerDir;
                var script = new ShellScriptBuilder()
                    .AddCommand($"cp {tempDir}/{FilePaths.ExtensibleConfigurationFileName} {appDir}")
                    .AddCommand($"oryx build-script {appDir}")
                    .ToString();

                // Act
                var result = _dockerCli.Run(new DockerRunArguments
                {
                    ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                    Volumes = new List<DockerVolume> { volume, tempDirVolume },
                    CommandToExecuteOnRun = "/bin/bash",
                    CommandArguments = new[] { "-c", script }
                });

                // Assert
                RunAsserts(
                    () =>
                    {
                        Assert.True(result.IsSuccess);
                        Assert.Contains(
                            $"echo \"Hello, world!\"",
                            result.StdOut);
                        Assert.Contains(
                            $"echo \"Hello, foobar!\"",
                            result.StdOut);
                    },
                    result.GetDebugInfo());
            }
            finally
            {
                DeleteTempDirectory();
            }
        }

        [Fact, Trait("category", "githubactions")]
        public void BuildScriptGenerated_WithExtensibilityModel_WithHttpGet_Succeeds()
        {
            // Arrange
            var configContent = @"
base-os: debian
pre-build: 
  - description: 'Test description'
    http-get:
      url: 'testurl'
      file-name: 'samplefile'
      headers:
        - 'header1'
        - 'header2'
";

            var volume = CreateWebFrontEndVolume();
            CreateTempDirectory();
            try
            {
                CreateExtensibilityConfigFile(configContent);
                var tempDirVolume = CreateTempDirectoryVolume();
                var appDir = volume.ContainerDir;
                var tempDir = tempDirVolume.ContainerDir;
                var script = new ShellScriptBuilder()
                    .AddCommand($"cp {tempDir}/{FilePaths.ExtensibleConfigurationFileName} {appDir}")
                    .AddCommand($"oryx build-script {appDir}")
                    .ToString();

                // Act
                var result = _dockerCli.Run(new DockerRunArguments
                {
                    ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                    Volumes = new List<DockerVolume> { volume, tempDirVolume },
                    CommandToExecuteOnRun = "/bin/bash",
                    CommandArguments = new[] { "-c", script }
                });

                // Assert
                RunAsserts(
                    () =>
                    {
                        Assert.True(result.IsSuccess);
                        Assert.Contains(
                            $"curl -o samplefile -H header1 -H header2 testurl",
                            result.StdOut);
                    },
                    result.GetDebugInfo());
            }
            finally
            {
                DeleteTempDirectory();
            }
        }

        [Fact, Trait("category", "githubactions")]
        public void BuildScriptGenerated_WithExtensibilityModel_WithMultipleProperties_Succeeds()
        {
            // Arrange
            var configContent = @"
base-os: debian
env: 
  - name: FOO
    value: BAR
pre-build: 
  - description: 'Test description'
    scripts:
      - 'echo ""Hello, world!""'
      - 'echo ""Hello, foobar!""'
    http-get:
      url: 'testurl'
      file-name: 'samplefile'
      headers:
        - 'header1'
";

            var volume = CreateWebFrontEndVolume();
            CreateTempDirectory();
            try
            {
                CreateExtensibilityConfigFile(configContent);
                var tempDirVolume = CreateTempDirectoryVolume();
                var appDir = volume.ContainerDir;
                var tempDir = tempDirVolume.ContainerDir;
                var script = new ShellScriptBuilder()
                    .AddCommand($"cp {tempDir}/{FilePaths.ExtensibleConfigurationFileName} {appDir}")
                    .AddCommand($"oryx build-script {appDir}")
                    .ToString();

                // Act
                var result = _dockerCli.Run(new DockerRunArguments
                {
                    ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                    Volumes = new List<DockerVolume> { volume, tempDirVolume },
                    CommandToExecuteOnRun = "/bin/bash",
                    CommandArguments = new[] { "-c", script }
                });

                // Assert
                RunAsserts(
                    () =>
                    {
                        Assert.True(result.IsSuccess);
                        Assert.Contains(
                            $"export FOO=\"BAR\"",
                            result.StdOut);
                        Assert.Contains(
                            $"echo \"Hello, world!\"",
                            result.StdOut);
                        Assert.Contains(
                            $"echo \"Hello, foobar!\"",
                            result.StdOut);
                        Assert.Contains(
                            $"curl -o samplefile -H header1 testurl",
                            result.StdOut);
                    },
                    result.GetDebugInfo());
            }
            finally
            {
                DeleteTempDirectory();
            }
        }

        [Fact, Trait("category", "githubactions")]
        public void BuildScriptGenerated_WithExtensibilityModel_WithMultiplePrebuildSteps_Succeeds()
        {
            // Arrange
            var configContent = @"
base-os: debian
pre-build: 
  - description: 'Test description 1'
    scripts:
      - 'echo ""Hello, world!""'
      - 'echo ""Hello, foobar!""'
    http-get:
      url: 'testurl'
      file-name: 'samplefile'
      headers:
        - 'header1'
  - description: 'Test description 2'
    scripts:
      - 'echo ""Hello, world 2!""'
      - 'echo ""Hello, foobar 2!""'
";

            var volume = CreateWebFrontEndVolume();
            CreateTempDirectory();
            try
            {
                CreateExtensibilityConfigFile(configContent);
                var tempDirVolume = CreateTempDirectoryVolume();
                var appDir = volume.ContainerDir;
                var tempDir = tempDirVolume.ContainerDir;
                var script = new ShellScriptBuilder()
                    .AddCommand($"cp {tempDir}/{FilePaths.ExtensibleConfigurationFileName} {appDir}")
                    .AddCommand($"oryx build-script {appDir}")
                    .ToString();

                // Act
                var result = _dockerCli.Run(new DockerRunArguments
                {
                    ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                    Volumes = new List<DockerVolume> { volume, tempDirVolume },
                    CommandToExecuteOnRun = "/bin/bash",
                    CommandArguments = new[] { "-c", script }
                });

                // Assert
                RunAsserts(
                    () =>
                    {
                        Assert.True(result.IsSuccess);
                        Assert.Contains(
                            $"echo \"Hello, world!\"",
                            result.StdOut);
                        Assert.Contains(
                            $"echo \"Hello, foobar!\"",
                            result.StdOut);
                        Assert.Contains(
                            $"curl -o samplefile -H header1 testurl",
                            result.StdOut);
                        Assert.Contains(
                            $"echo \"Hello, world 2!\"",
                            result.StdOut);
                        Assert.Contains(
                            $"echo \"Hello, foobar 2!\"",
                            result.StdOut);
                    },
                    result.GetDebugInfo());
            }
            finally
            {
                DeleteTempDirectory();
            }
        }
    }
}
