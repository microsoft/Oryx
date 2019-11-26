// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class PythonSampleAppsTestBase : SampleAppsTestBase
    {
        public const string PackagesDirectory = "__oryx_packages__";
        public DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "python", sampleAppName));

        public PythonSampleAppsTestBase(ITestOutputHelper output) : base(output)
        {
        }
    }

    public class PythonSampleAppsOtherTests : PythonSampleAppsTestBase
    {
        public PythonSampleAppsOtherTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData(Settings.BuildImageName)]
        [InlineData(Settings.SlimBuildImageName)]
        public void GeneratesScript_AndBuilds(string buildImageName)
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = buildImageName,
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
                        $"Python Version: /opt/python/{PythonVersions.Python37Version}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_AndBuilds_WithPackageDir()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} -p packagedir={PackagesDirectory}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{PackagesDirectory}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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
                        $"Python Version: /opt/python/{PythonVersions.Python37Version}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }


        [Fact]
        public void Builds_AndCopiesContentToOutputDirectory_Recursively()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var subDir = Guid.NewGuid();
            var script = new ShellScriptBuilder()
                // Add a test sub-directory with a file
                .CreateDirectory($"{appDir}/{subDir}")
                .CreateFile($"{appDir}/{subDir}/file1.txt", "file1.txt")
                // Execute command
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                // Check the output directory for the sub directory
                .AddFileExistsCheck($"{appOutputDir}/{subDir}/file1.txt")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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

        [Fact]
        public void Build_CopiesOutput_ToNestedOutputDirectory()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var nestedOutputDir = "/tmp/app-output/subdir1";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {nestedOutputDir}")
                .AddDirectoryExistsCheck($"{nestedOutputDir}/pythonenv")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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

        [Fact]
        public void GeneratesScriptAndBuilds_WhenSourceAndDestinationFolders_AreSame()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir}")
                .AddDirectoryExistsCheck($"{appDir}/pythonenv")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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

        [Fact]
        public void GeneratesScriptAndBuilds_WhenDestination_IsSubDirectoryOfSource()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/pythonenv")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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

        [Fact]
        public void Build_DoestNotCleanDestinationDir()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                // Pre-populate the output directory with content
                .CreateDirectory(appOutputDir)
                .CreateFile($"{appOutputDir}/hi.txt", "hi")
                .CreateDirectory($"{appOutputDir}/blah")
                .CreateFile($"{appOutputDir}/blah/hi.txt", "hi")
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/pythonenv")
                .AddFileExistsCheck($"{appOutputDir}/hi.txt")
                .AddFileExistsCheck($"{appOutputDir}/blah/hi.txt")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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

        [Fact]
        public void ErrorDuringBuild_ResultsIn_NonSuccessfulExitCode()
        {
            // Try building a Python 2.7 app with 3.7 version. This should fail as there are major
            // API changes between these versions

            // Arrange
            var langVersion = PythonVersions.Python37Version;
            var appName = "python2-flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var generatedScript = "/tmp/build.sh";
            var appOutputDir = "/tmp/app-output";
            var tempDir = "/tmp/" + Guid.NewGuid();
            var script = new ShellScriptBuilder()
                .AddScriptCommand($"{appDir} --platform python --platform-version {langVersion} > {generatedScript}")
                .SetExecutePermissionOnFile(generatedScript)
                .CreateDirectory(tempDir)
                .AddCommand($"{generatedScript} {appDir} {appOutputDir} {tempDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.False(result.IsSuccess);
                    Assert.Contains("Missing parentheses in call to 'print'", result.StdErr);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void GeneratesScript_AndBuilds_WhenExplicitLanguageAndVersion_AreProvided()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} --platform python " +
                $"--platform-version {PythonVersions.Python36Version}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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
                        $"Python Version: /opt/python/{PythonVersions.Python36Version}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void CanBuild_UsingScriptGeneratedBy_ScriptOnlyOption()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var generatedScript = "/tmp/build.sh";
            var appOutputDir = "/tmp/app-output";
            var tempDir = "/tmp/" + Guid.NewGuid();
            var script = new ShellScriptBuilder()
                .AddScriptCommand(
                $"{appDir} --platform python --platform-version {PythonVersions.Python36Version} > {generatedScript}")
                .SetExecutePermissionOnFile(generatedScript)
                .CreateDirectory(tempDir)
                .AddCommand($"{generatedScript} {appDir} {appOutputDir} {tempDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/pythonenv/lib/python3.6/site-packages/flask/")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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

        [Fact]
        public void ThrowsException_ForInvalidPythonVersion()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var generatedScript = "/tmp/build.sh";
            var appOutputDir = "/tmp/app-output";
            var tempDir = "/tmp/" + Guid.NewGuid();
            var script = new ShellScriptBuilder()
                .AddScriptCommand($"{appDir} --platform python --platform-version 4.0.1 > {generatedScript}")
                .SetExecutePermissionOnFile(generatedScript)
                .CreateDirectory(tempDir)
                .AddCommand($"{generatedScript} {appDir} {appOutputDir} {tempDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(appName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    var errorMessage =
                    "Error: Platform 'python' version '4.0.1' is unsupported. Supported versions: >=2.7 <4";
                    Assert.False(result.IsSuccess);
                    Assert.Contains(errorMessage, result.StdErr);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void CanBuild_Python2_WithScriptOnlyOption()
        {
            // Arrange
            var langVersion = PythonVersions.Python27Version;
            var appName = "python2-flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var generatedScript = "/tmp/build.sh";
            var appOutputDir = "/tmp/app-output";
            var tempDir = "/tmp/" + Guid.NewGuid();
            var script = new ShellScriptBuilder()
                .AddScriptCommand($"{appDir} --platform python --platform-version {langVersion} > {generatedScript}")
                .SetExecutePermissionOnFile(generatedScript)
                .CreateDirectory(tempDir)
                .AddCommand($"{generatedScript} {appDir} {appOutputDir} {tempDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/pythonenv/lib/python2.7/site-packages/flask/")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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

        [Fact]
        public void GeneratesScript_AndBuilds_UsingSuppliedIntermediateDir()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appIntermediateDir = "/tmp/app-intermediate";
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir} -i {appIntermediateDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/pythonenv")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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

        [Fact]
        public void Build_VirtualEnv_Unzipped_ByDefault()
        {
            // Arrange
            var virtualEnvironmentName = "pythonenv";
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{virtualEnvironmentName}")
                .AddFileDoesNotExistCheck($"{appOutputDir}/{virtualEnvironmentName}.tar.gz")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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
                        $"Python Version: /opt/python/{PythonVersions.Python37Version}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("tar-gz")]
        public void Build_CompressesVirtualEnv_InTargGzFormat(string compressionFormat)
        {
            // Arrange
            var virtualEnvironmentName = "myenv";
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"-p virtualenv_name={virtualEnvironmentName} -p compress_virtualenv={compressionFormat}")
                .AddDirectoryDoesNotExistCheck($"{appOutputDir}/{virtualEnvironmentName}")
                .AddFileExistsCheck($"{appOutputDir}/{virtualEnvironmentName}.tar.gz")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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
                        $"Python Version: /opt/python/{PythonVersions.Python37Version}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Build_CompressesVirtualEnv_InZipFormat()
        {
            // Arrange
            var virtualEnvironmentName = "myenv";
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"-p virtualenv_name={virtualEnvironmentName} -p compress_virtualenv=zip")
                .AddDirectoryDoesNotExistCheck($"{appOutputDir}/{virtualEnvironmentName}")
                .AddFileExistsCheck($"{appOutputDir}/{virtualEnvironmentName}.zip")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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
                        $"Python Version: /opt/python/{PythonVersions.Python37Version}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }


        [Fact]
        public void Build_InstallsVirtualEnvironment_AndPackagesInIt()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} --platform python " +
                $"--platform-version {PythonVersions.Python37Version}")
                .AddDirectoryExistsCheck($"{appOutputDir}/pythonenv/lib/python3.7/site-packages/flask")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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
                        $"Python Version: /opt/python/{PythonVersions.Python37Version}/bin/python3",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Build_ExecutesPreAndPostBuildScripts_UsingBuildEnvironmentFile()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            using (var sw = File.AppendText(Path.Combine(volume.MountedHostDir, "build.env")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("PRE_BUILD_SCRIPT_PATH=scripts/prebuild.sh");
                sw.WriteLine("POST_BUILD_SCRIPT_PATH=scripts/postbuild.sh");
            }
            var scriptsDir = Directory.CreateDirectory(Path.Combine(volume.MountedHostDir, "scripts"));
            using (var sw = File.AppendText(Path.Combine(scriptsDir.FullName, "prebuild.sh")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("#!/bin/bash");
                sw.WriteLine("echo Executing the pre-build script from a standalone script!");
            }
            using (var sw = File.AppendText(Path.Combine(scriptsDir.FullName, "postbuild.sh")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("#!/bin/bash");
                sw.WriteLine("echo Executing the post-build script from a standalone script!");
            }
            if (RuntimeInformation.IsOSPlatform(Settings.LinuxOS))
            {
                ProcessHelper.RunProcess(
                    "chmod",
                    new[] { "-R", "777", scriptsDir.FullName },
                    workingDirectory: null,
                    waitTimeForExit: null);
            }
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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
                        "Executing the pre-build script from a standalone script!",
                        result.StdOut);
                    Assert.Contains(
                        "Executing the post-build script from a standalone script!",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Build_ExecutesPreAndPostBuildScripts_UsingEnvironmentVariables()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var scriptsDir = Directory.CreateDirectory(Path.Combine(volume.MountedHostDir, "scripts"));
            using (var sw = File.AppendText(Path.Combine(scriptsDir.FullName, "prebuild.sh")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("#!/bin/bash");
                sw.WriteLine("echo Executing the pre-build script from a standalone script!");
            }
            using (var sw = File.AppendText(Path.Combine(scriptsDir.FullName, "postbuild.sh")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("#!/bin/bash");
                sw.WriteLine("echo Executing the post-build script from a standalone script!");
            }
            if (RuntimeInformation.IsOSPlatform(Settings.LinuxOS))
            {
                ProcessHelper.RunProcess(
                    "chmod",
                    new[] { "-R", "777", scriptsDir.FullName },
                    workingDirectory: null,
                    waitTimeForExit: null);
            }
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                EnvironmentVariables = new List<EnvironmentVariable>
                {
                    CreateAppNameEnvVar(appName),
                    new EnvironmentVariable("PRE_BUILD_SCRIPT_PATH", "scripts/prebuild.sh"),
                    new EnvironmentVariable("POST_BUILD_SCRIPT_PATH", "scripts/postbuild.sh")
                },
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
                        "Executing the pre-build script from a standalone script!",
                        result.StdOut);
                    Assert.Contains(
                        "Executing the post-build script from a standalone script!",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void PreAndPostBuildScripts_HaveAccessToSourceAndDestinationDirectoryVariables()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            var scriptsDir = Directory.CreateDirectory(Path.Combine(volume.MountedHostDir, "scripts"));
            using (var sw = File.AppendText(Path.Combine(scriptsDir.FullName, "prebuild.sh")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("#!/bin/bash");
                sw.WriteLine("echo \"pre-build: $SOURCE_DIR, $DESTINATION_DIR\"");
            }
            using (var sw = File.AppendText(Path.Combine(scriptsDir.FullName, "postbuild.sh")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("#!/bin/bash");
                sw.WriteLine("echo \"post-build: $SOURCE_DIR, $DESTINATION_DIR\"");
            }
            if (RuntimeInformation.IsOSPlatform(Settings.LinuxOS))
            {
                ProcessHelper.RunProcess(
                    "chmod",
                    new[] { "-R", "777", scriptsDir.FullName },
                    workingDirectory: null,
                    waitTimeForExit: null);
            }
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                EnvironmentVariables = new List<EnvironmentVariable>
                {
                    CreateAppNameEnvVar(appName),
                    new EnvironmentVariable("PRE_BUILD_SCRIPT_PATH", "scripts/prebuild.sh"),
                    new EnvironmentVariable("POST_BUILD_SCRIPT_PATH", "scripts/postbuild.sh")
                },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains($"pre-build: /tmp/int, {appOutputDir}", result.StdOut);
                    Assert.Contains($"post-build: /tmp/int, {appOutputDir}", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Build_UsesEnvironmentSettings_InOrderOfPrecedence()
        {
            // Order of precedence is: EnvironmentVariables -> build.env file settings

            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            using (var sw = File.AppendText(Path.Combine(volume.MountedHostDir, "build.env")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("PRE_BUILD_SCRIPT_PATH=scripts/prebuild.sh");
                sw.WriteLine("POST_BUILD_SCRIPT_PATH=scripts/postbuild.sh");
                sw.WriteLine("key1=value-from-buildenv-file");
                sw.WriteLine("key2=value-from-buildenv-file");
            }
            var scriptsDir = Directory.CreateDirectory(Path.Combine(volume.MountedHostDir, "scripts"));
            using (var sw = File.AppendText(Path.Combine(scriptsDir.FullName, "prebuild.sh")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("#!/bin/bash");
                sw.WriteLine("echo From pre-build script: \"$key1, $key2\"");
            }
            using (var sw = File.AppendText(Path.Combine(scriptsDir.FullName, "postbuild.sh")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("#!/bin/bash");
                sw.WriteLine("echo From post-build script: \"$key1, $key2\"");
            }
            if (RuntimeInformation.IsOSPlatform(Settings.LinuxOS))
            {
                ProcessHelper.RunProcess(
                    "chmod",
                    new[] { "-R", "777", scriptsDir.FullName },
                    workingDirectory: null,
                    waitTimeForExit: null);
            }
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                EnvironmentVariables = new List<EnvironmentVariable>
                {
                    CreateAppNameEnvVar(appName),
                    new EnvironmentVariable("PRE_BUILD_SCRIPT_PATH", "scripts/prebuild.sh"),
                    new EnvironmentVariable("POST_BUILD_SCRIPT_PATH", "scripts/postbuild.sh"),
                    new EnvironmentVariable("key2", "value-from-environmentvariable")
                },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    var values = "value-from-buildenv-file, value-from-environmentvariable";
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        $"From pre-build script: {values}",
                        result.StdOut);
                    Assert.Contains(
                        $"From post-build script: {values}",
                        result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Build_Executes_InlinePreAndPostBuildCommands()
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            using (var sw = File.AppendText(Path.Combine(volume.MountedHostDir, "build.env")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("PRE_BUILD_COMMAND=\"echo from pre-build command\"");
                sw.WriteLine("POST_BUILD_COMMAND=\"echo from post-build command\"");
            }

            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o /tmp/output")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                EnvironmentVariables = new List<EnvironmentVariable>
                {
                    CreateAppNameEnvVar(appName),
                },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("from pre-build command", result.StdOut);
                    Assert.Contains("from post-build command", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Django_CollectStaticFailure_DoesNotFailBuild()
        {
            // Arrange
            var appName = "django-realworld-example-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} --platform python " +
                $"--platform-version {PythonVersions.Python37Version}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                EnvironmentVariables = new List<EnvironmentVariable>
                {
                    CreateAppNameEnvVar(appName),
                },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("'collectstatic' exited with exit code 1.", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("3", PythonVersions.Python38Version)]
        [InlineData("2.7", PythonVersions.Python27Version)]
        public void Build_ExecutesPreAndPostBuildScripts_WithinBenvContext(string version, string expectedVersion)
        {
            // Arrange
            var appName = "flask-app";
            var volume = CreateSampleAppVolume(appName);
            using (var sw = File.AppendText(Path.Combine(volume.MountedHostDir, "build.env")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("PRE_BUILD_SCRIPT_PATH=scripts/prebuild.sh");
                sw.WriteLine("POST_BUILD_SCRIPT_PATH=scripts/postbuild.sh");
            }
            var scriptsDir = Directory.CreateDirectory(Path.Combine(volume.MountedHostDir, "scripts"));
            using (var sw = File.AppendText(Path.Combine(scriptsDir.FullName, "prebuild.sh")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("#!/bin/bash");
                sw.WriteLine("echo \"Pre-build script: $python\"");
                sw.WriteLine("echo \"Pre-build script: $pip\"");
            }
            using (var sw = File.AppendText(Path.Combine(scriptsDir.FullName, "postbuild.sh")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("#!/bin/bash");
                sw.WriteLine("echo \"Post-build script: $python\"");
                sw.WriteLine("echo \"Post-build script: $pip\"");
            }
            if (RuntimeInformation.IsOSPlatform(Settings.LinuxOS))
            {
                ProcessHelper.RunProcess(
                    "chmod",
                    new[] { "-R", "777", scriptsDir.FullName },
                    workingDirectory: null,
                    waitTimeForExit: null);
            }
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} --platform python --platform-version {version}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                EnvironmentVariables = new List<EnvironmentVariable>
                {
                    CreateAppNameEnvVar(appName),
                },
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
                        $"Pre-build script: /opt/python/{expectedVersion}/bin/python",
                        result.StdOut);
                    Assert.Matches(
                        $"Pre-build script: /opt/python/{expectedVersion}/bin/pip",
                        result.StdOut);
                    Assert.Matches(
                        $"Post-build script: /opt/python/{expectedVersion}/bin/python",
                        result.StdOut);
                    Assert.Matches($"Post-build script: /opt/python/{expectedVersion}/bin/pip", result.StdOut);
                },
                result.GetDebugInfo());
        }
    }

    public class PythonSampleAppsDjangoAppRunningCollectStatic : PythonSampleAppsTestBase
    {
        public PythonSampleAppsDjangoAppRunningCollectStatic(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData(null)]
        [InlineData("false")]
        public void GeneratesScript_AndBuilds_DjangoApp_RunningCollectStatic(string disableCollectStatic)
        {
            // Arrange
            var appName = "django-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var scriptBuilder = new ShellScriptBuilder();
            if (string.IsNullOrEmpty(disableCollectStatic))
            {
                scriptBuilder.AddCommand(
                    $"export {EnvironmentSettingsKeys.DisableCollectStatic}={disableCollectStatic}");
            }
            var script = scriptBuilder
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} --platform python " +
                $"--platform-version {PythonVersions.Python37Version}")
                // These css files should be available since 'collectstatic' is run in the script
                .AddFileExistsCheck($"{appOutputDir}/staticfiles/css/boards.css")
                .AddFileExistsCheck($"{appOutputDir}/staticfiles/css/uservoice.css")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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
    }

    public class PythonSampleAppsBuildsShapely : PythonSampleAppsTestBase
    {
        public PythonSampleAppsBuildsShapely(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [MemberData(nameof(TestValueGenerator.GetPythonVersions), MemberType = typeof(TestValueGenerator))]
        public void GeneratesScript_AndBuilds_Shapely_With_Python(string version)
        {
            // Arrange
            var appName = "shapely-flask-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} --platform python --platform-version {version}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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
    }

    public class PythonSampleAppsDjangoAppWithoutRunningCollectStatic : PythonSampleAppsTestBase
    {
        public PythonSampleAppsDjangoAppWithoutRunningCollectStatic(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData("true")]
        [InlineData("True")]
        public void GeneratesScript_AndBuilds_DjangoApp_WithoutRunningCollectStatic_IfDisableCollectStatic_IsTrue(
            string disableCollectStatic)
        {
            // Arrange
            var appName = "django-app";
            var volume = CreateSampleAppVolume(appName);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddCommand($"export {EnvironmentSettingsKeys.DisableCollectStatic}={disableCollectStatic}")
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} --platform python " +
                $"--platform-version {PythonVersions.Python37Version}")
                // These css files should NOT be available since 'collectstatic' is set off
                .AddFileDoesNotExistCheck($"{appOutputDir}/staticfiles/css/boards.css")
                .AddFileDoesNotExistCheck($"{appOutputDir}/staticfiles/css/uservoice.css")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
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
    }
}