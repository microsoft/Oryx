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
    public class PythonSampleAppsTest : SampleAppsTestBase
    {
        private const string PackagesDirectory = "__oryx_packages__";
        private readonly DockerCli _dockerCli = new DockerCli();

        public PythonSampleAppsTest(ITestOutputHelper output) : base(output)
        {
        }

        private DockerVolume CreateSampleAppVolume(string sampleAppName) =>
            DockerVolume.Create(Path.Combine(_hostSamplesDir, "python", sampleAppName));

        [Fact]
        public override void GeneratesScript_AndBuilds()
        {
            // Arrange
            var volume = CreateSampleAppVolume("flask-app");
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{PackagesDirectory}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("flask-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        $"Python Version: /opt/python/{PythonVersions.Python37Version}/bin/python3",
                        result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public override void Builds_AndCopiesContentToOutputDirectory_Recursively()
        {
            // Arrange
            var volume = CreateSampleAppVolume("flask-app");
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var subDir = Guid.NewGuid();
            var script = new ShellScriptBuilder()
                // Add a test sub-directory with a file
                .CreateDirectory($"{appDir}/{subDir}")
                .CreateFile($"{appDir}/{subDir}/file1.txt", "file1.txt")
                // Execute command
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{PackagesDirectory}")
                // Check the output directory for the sub directory
                .AddFileExistsCheck($"{appOutputDir}/{subDir}/file1.txt")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("flask-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
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
        public override void Build_CopiesOutput_ToNestedOutputDirectory()
        {
            // Arrange
            var volume = CreateSampleAppVolume("flask-app");
            var appDir = volume.ContainerDir;
            var nestedOutputDir = "/tmp/app-output/subdir1";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {nestedOutputDir}")
                .AddDirectoryExistsCheck($"{nestedOutputDir}/{PackagesDirectory}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("flask-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
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
        public override void GeneratesScriptAndBuilds_WhenSourceAndDestinationFolders_AreSame()
        {
            // Arrange
            var volume = CreateSampleAppVolume("flask-app");
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir}")
                .AddDirectoryExistsCheck($"{appDir}/{PackagesDirectory}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("flask-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
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
        public override void GeneratesScriptAndBuilds_WhenDestination_IsSubDirectoryOfSource()
        {
            // Arrange
            var volume = CreateSampleAppVolume("flask-app");
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{PackagesDirectory}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("flask-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
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
        public override void Build_ReplacesContentInDestinationDir_WhenDestinationDirIsNotEmpty()
        {
            // Arrange
            var volume = CreateSampleAppVolume("flask-app");
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                // Pre-populate the output directory with content
                .CreateDirectory(appOutputDir)
                .CreateFile($"{appOutputDir}/hi.txt", "hi")
                .CreateDirectory($"{appOutputDir}/blah")
                .CreateFile($"{appOutputDir}/blah/hi.txt", "hi")
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{PackagesDirectory}")
                .AddFileDoesNotExistCheck($"{appOutputDir}/hi.txt")
                .AddDirectoryDoesNotExistCheck($"{appOutputDir}/blah")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("flask-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
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
        public override void ErrorDuringBuild_ResultsIn_NonSuccessfulExitCode()
        {
            // Try building a Python 2.7 app with 3.7 version. This should fail as there are major API changes between these versions

            // Arrange
            var langVersion = PythonVersions.Python37Version;
            var volume = CreateSampleAppVolume("python2-flask-app");
            var appDir = volume.ContainerDir;
            var generatedScript = "/tmp/build.sh";
            var appOutputDir = "/tmp/app-output";
            var tempDir = "/tmp/" + Guid.NewGuid();
            var script = new ShellScriptBuilder()
                .AddScriptCommand($"{appDir} -l python --language-version {langVersion} > {generatedScript}")
                .SetExecutePermissionOnFile(generatedScript)
                .CreateDirectory(tempDir)
                .AddCommand($"{generatedScript} {appDir} {appOutputDir} {tempDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("python2-flask-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.False(result.IsSuccess);
                    Assert.Contains("Missing parentheses in call to 'print'", result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public override void GeneratesScript_AndBuilds_WhenExplicitLanguageAndVersion_AreProvided()
        {
            // Arrange
            var volume = CreateSampleAppVolume("flask-app");
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir} -l python --language-version {Settings.Python36Version}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{PackagesDirectory}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("flask-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        $"Python Version: /opt/python/{Settings.Python36Version}/bin/python3",
                        result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public override void CanBuild_UsingScriptGeneratedBy_ScriptOnlyOption()
        {
            // Arrange
            var volume = CreateSampleAppVolume("flask-app");
            var appDir = volume.ContainerDir;
            var generatedScript = "/tmp/build.sh";
            var appOutputDir = "/tmp/app-output";
            var tempDir = "/tmp/" + Guid.NewGuid();
            var script = new ShellScriptBuilder()
                .AddScriptCommand($"{appDir} -l python --language-version {Settings.Python36Version} > {generatedScript}")
                .SetExecutePermissionOnFile(generatedScript)
                .CreateDirectory(tempDir)
                .AddCommand($"{generatedScript} {appDir} {appOutputDir} {tempDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{PackagesDirectory}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("flask-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
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
        public void CanthrowException_ForInvalidPythonVersion()
        {
            // Arrange
            var volume = CreateSampleAppVolume("flask-app");
            var appDir = volume.ContainerDir;
            var generatedScript = "/tmp/build.sh";
            var appOutputDir = "/tmp/app-output";
            var tempDir = "/tmp/" + Guid.NewGuid();
            var script = new ShellScriptBuilder()
                .AddScriptCommand($"{appDir} -l python --language-version 4.0.1 > {generatedScript}")
                .SetExecutePermissionOnFile(generatedScript)
                .CreateDirectory(tempDir)
                .AddCommand($"{generatedScript} {appDir} {appOutputDir} {tempDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{PackagesDirectory}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("flask-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    string errorMessage =
                    "The 'python' version '4.0.1' is not supported. Supported versions are: " +
                    $"{Settings.Python27Version}, {Settings.Python35Version}, {Settings.Python36Version}, {PythonVersions.Python37Version}";
                    Assert.False(result.IsSuccess);
                    Assert.Contains(errorMessage, result.Error);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void CanBuild_Python2_WithScriptOnlyOption()
        {
            // Arrange
            var langVersion = Settings.Python27Version;
            var volume = CreateSampleAppVolume("python2-flask-app");
            var appDir = volume.ContainerDir;
            var generatedScript = "/tmp/build.sh";
            var appOutputDir = "/tmp/app-output";
            var tempDir = "/tmp/" + Guid.NewGuid();
            var script = new ShellScriptBuilder()
                .AddScriptCommand($"{appDir} -l python --language-version {langVersion} > {generatedScript}")
                .SetExecutePermissionOnFile(generatedScript)
                .CreateDirectory(tempDir)
                .AddCommand($"{generatedScript} {appDir} {appOutputDir} {tempDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{PackagesDirectory}/flask")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("python2-flask-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
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
        public override void GeneratesScript_AndBuilds_UsingSuppliedIntermediateDir()
        {
            // Arrange
            var volume = CreateSampleAppVolume("flask-app");
            var appDir = volume.ContainerDir;
            var appIntermediateDir = "/tmp/app-intermediate";
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir} -i {appIntermediateDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{PackagesDirectory}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("flask-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
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
        public void GeneratesScript_AndBuilds_UsingCustomVirtualEnvironmentName()
        {
            // Arrange
            var virtualEnvironmentName = "myenv";
            var volume = CreateSampleAppVolume("flask-app");
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir} -p virtualenv_name={virtualEnvironmentName}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{virtualEnvironmentName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("flask-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        $"Python Version: /opt/python/{PythonVersions.Python37Version}/bin/python3",
                        result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Build_InstallsVirtualEnvironment_AndPackagesInIt()
        {
            // Arrange
            var volume = CreateSampleAppVolume("flask-app");
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir} -l python --language-version {PythonVersions.Python37Version}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{PackagesDirectory}/jinja2")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("flask-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        $"Python Version: /opt/python/{PythonVersions.Python37Version}/bin/python3",
                        result.Output);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("false")]
        public void GeneratesScript_AndBuilds_DjangoApp_RunningCollectStatic(string disableCollectStatic)
        {
            // Arrange
            var volume = CreateSampleAppVolume("django-app");
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var scriptBuilder = new ShellScriptBuilder();
            if (string.IsNullOrEmpty(disableCollectStatic))
            {
                scriptBuilder.AddCommand(
                    $"export {EnvironmentSettingsKeys.DisableCollectStatic}={disableCollectStatic}");
            }
            var script = scriptBuilder
                .AddBuildCommand($"{appDir} -o {appOutputDir} -l python --language-version {PythonVersions.Python37Version}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{PackagesDirectory}/django")
                // These css files should be available since 'collectstatic' is run in the script
                .AddFileExistsCheck($"{appOutputDir}/staticfiles/css/boards.css")
                .AddFileExistsCheck($"{appOutputDir}/staticfiles/css/uservoice.css")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("django-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("true")]
        [InlineData("True")]
        public void GeneratesScript_AndBuilds_DjangoApp_WithoutRunningCollectStatic_IfDisableCollectStatic_IsTrue(
            string disableCollectStatic)
        {
            // Arrange
            var volume = CreateSampleAppVolume("django-app");
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddCommand($"export {EnvironmentSettingsKeys.DisableCollectStatic}={disableCollectStatic}")
                .AddBuildCommand($"{appDir} -o {appOutputDir} -l python --language-version {PythonVersions.Python37Version}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{PackagesDirectory}/django")
                // These css files should NOT be available since 'collectstatic' is set off
                .AddFileDoesNotExistCheck($"{appOutputDir}/staticfiles/css/boards.css")
                .AddFileDoesNotExistCheck($"{appOutputDir}/staticfiles/css/uservoice.css")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("django-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
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
        public void Build_ExecutesPreAndPostBuildScripts_UsingBuildEnvironmentFile()
        {
            // Arrange
            var volume = CreateSampleAppVolume("flask-app");
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
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{PackagesDirectory}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("flask-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        "Executing the pre-build script from a standalone script!",
                        result.Output);
                    Assert.Contains(
                        "Executing the post-build script from a standalone script!",
                        result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Build_ExecutesPreAndPostBuildScripts_UsingEnvironmentVariables()
        {
            // Arrange
            var volume = CreateSampleAppVolume("flask-app");
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
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{PackagesDirectory}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                new List<EnvironmentVariable>()
                {
                    CreateAppNameEnvVar("flask-app"),
                    new EnvironmentVariable("PRE_BUILD_SCRIPT_PATH", "scripts/prebuild.sh"),
                    new EnvironmentVariable("POST_BUILD_SCRIPT_PATH", "scripts/postbuild.sh")
                },
                new List<DockerVolume>() { volume },
                portMapping: null,
                link: null,
                runContainerInBackground: false,
                command: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        "Executing the pre-build script from a standalone script!",
                        result.Output);
                    Assert.Contains(
                        "Executing the post-build script from a standalone script!",
                        result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Build_UsesEnvironmentSettings_InOrderOfPrecedence()
        {
            // Order of precedence is: EnvironmentVariables -> build.env file settings

            // Arrange
            var volume = CreateSampleAppVolume("flask-app");
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
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{PackagesDirectory}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                new List<EnvironmentVariable>()
                {
                    CreateAppNameEnvVar("flask-app"),
                    new EnvironmentVariable("PRE_BUILD_SCRIPT_PATH", "scripts/prebuild.sh"),
                    new EnvironmentVariable("POST_BUILD_SCRIPT_PATH", "scripts/postbuild.sh"),
                    new EnvironmentVariable("key2", "value-from-environmentvariable")
                },
                new List<DockerVolume>() { volume },
                portMapping: null,
                link: null,
                runContainerInBackground: false,
                command: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    var values = "value-from-buildenv-file, value-from-environmentvariable";
                    Assert.True(result.IsSuccess);
                    Assert.Contains(
                        $"From pre-build script: {values}",
                        result.Output);
                    Assert.Contains(
                        $"From post-build script: {values}",
                        result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void Django_CollectStaticFailure_DoesNotFailBuild()
        {
            // Arrange
            var volume = CreateSampleAppVolume("django-realworld-example-app");
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir} -l python --language-version {PythonVersions.Python37Version}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{PackagesDirectory}/django")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("django-realworld-example-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("'collectstatic' exited with exit code 1.", result.Output);
                },
                result.GetDebugInfo());
        }

        [Theory]
        [InlineData("3")]
        [InlineData("2")]
        public void Build_ExecutesPreAndPostBuildScripts_WithinBenvContext(string version)
        {
            // Arrange
            var volume = CreateSampleAppVolume("flask-app");
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
                .AddBuildCommand($"{appDir} -o {appOutputDir} -l python --language-version {version}")
                .AddDirectoryExistsCheck($"{appOutputDir}/{PackagesDirectory}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                CreateAppNameEnvVar("flask-app"),
                volume,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    script
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Matches(@"Pre-build script: /opt/python/" + version + @".\d+.\d+/bin/python" + version, result.Output);
                    Assert.Matches(@"Pre-build script: /opt/python/" + version + @".\d+.\d+/bin/pip", result.Output);
                    Assert.Matches(@"Post-build script: /opt/python/" + version + @".\d+.\d+/bin/python" + version, result.Output);
                    Assert.Matches(@"Post-build script: /opt/python/" + version + @".\d+.\d+/bin/pip", result.Output);
                },
                result.GetDebugInfo());
        }
    }
}