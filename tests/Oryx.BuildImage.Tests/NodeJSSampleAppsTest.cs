// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Oryx.Common.Utilities;
using Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.BuildImage.Tests
{
    public class NodeJSSampleAppsTest : SampleAppsTestBase
    {
        private readonly ITestOutputHelper _output;
        private readonly DockerCli _dockerCli;
        private readonly string _hostSamplesDir;

        public NodeJSSampleAppsTest(ITestOutputHelper output)
        {
            _output = output;

            _dockerCli = new DockerCli();

            _hostSamplesDir = Path.Combine(Directory.GetCurrentDirectory(), "SampleApps");
        }

        [Fact]
        public override void GeneratesScript_AndBuilds()
        {
            // Arrange
            var volume = DockerVolume.Create(Path.Combine(_hostSamplesDir, "nodejs", "webfrontend"));
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
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
        public override void Builds_AndCopiesContentToOutputDirectory_Recursively()
        {
            // Arrange
            var volume = DockerVolume.Create(Path.Combine(_hostSamplesDir, "nodejs", "webfrontend"));
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var subDir = Guid.NewGuid();
            var script = new ShellScriptBuilder()
                // Add a test sub-directory with a file
                .CreateDirectory($"{appDir}/{subDir}")
                .CreateFile($"{appDir}/{subDir}/file1.txt", "file1.txt")
                // Execute command
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                // Check the output directory for the sub directory
                .AddFileExistsCheck($"{appOutputDir}/{subDir}/file1.txt")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
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
            var volume = DockerVolume.Create(Path.Combine(_hostSamplesDir, "nodejs", "webfrontend"));
            var appDir = volume.ContainerDir;
            var nestedOutputDir = "/tmp/output/subdir1";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {nestedOutputDir}")
                .AddDirectoryExistsCheck($"{nestedOutputDir}/node_modules")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
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
            var volume = DockerVolume.Create(Path.Combine(_hostSamplesDir, "nodejs", "webfrontend"));
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var script = new ShellScriptBuilder()
                // Pre-populate the output directory with content
                .CreateDirectory(appOutputDir)
                .CreateFile($"{appOutputDir}/hi.txt", "hi")
                .CreateDirectory($"{appOutputDir}/blah")
                .CreateFile($"{appOutputDir}/blah/hi.txt", "hi")
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddFileDoesNotExistCheck($"{appOutputDir}/hi.txt")
                .AddDirectoryDoesNotExistCheck($"{appOutputDir}/blah")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
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

        private ShellScriptBuilder SetupEnvironment_ErrorDetectingNodeTest(string appDir, string appOutputDir, string logFile)
        {
            var nodeCode =
            @"var http = require('http'); var server = http.createServer(function(req, res) { res.writeHead(200); res.end('Hi oryx');}); server.listen(8080);";

            //following is the directory structure of the source repo in the test
            //temp
            //  app1
            //    idea.js
            //    1.log
            //    app2
            //      2.log
            //      app3
            //        3.log
            //        app4

            var script = new ShellScriptBuilder()
                .CreateDirectory($"{appDir}/app2/app3/app4")
                .CreateFile($"{appDir}/1.log", "hello1")
                .CreateFile($"{appDir}/app2/2.log", "hello2")
                .CreateFile($"{appDir}/app2/app3/3.log", "hello3")
                .CreateFile($"{appDir}/idea.js", nodeCode)
                .AddBuildCommand($"{appDir} -o {appOutputDir} --log-file {logFile}");

            return script;
        }

        [Fact]
        public void ErrorDetectingNode_FailedExitCode_StringContentFound()
        {
            var appDir = "/tmp/app1";
            var appOutputDir = "/tmp/app-output";
            var logFile = "/tmp/directory.log";

            var script = SetupEnvironment_ErrorDetectingNodeTest(appDir, appOutputDir, logFile)
                .AddFileExistsCheck(logFile, true)
                .AddStringExistsInFileCheck("idea.js", logFile)
                .AddStringExistsInFileCheck("app2", logFile)
                .AddStringExistsInFileCheck("app3", logFile)
                .AddStringExistsInFileCheck("2.log", logFile)
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
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
                    Assert.Equal(0, result.ExitCode);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void ErrorDetectingNode_FailedExitCode_StringContentNotFound()
        {
            var appDir = "/tmp/app1";
            var appOutputDir = "/tmp/app-output";
            var logFile = "/tmp/directory.log";

            var script = SetupEnvironment_ErrorDetectingNodeTest(appDir, appOutputDir, logFile)
                .AddBuildCommand($"{appDir} -o {appOutputDir} --log-file {logFile}")
                .AddStringNotExistsInFileCheck("app4", logFile, true)
                .AddStringNotExistsInFileCheck("3.log", logFile)
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
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
                    Assert.Equal(1, result.ExitCode);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public override void ErrorDuringBuild_ResultsIn_NonSuccessfulExitCode()
        {
            // Arrange
            // Here 'createServerFoooo' is a non-existing function in 'http' library
            var serverJsWithErrors = @"var http = require(""http""); http.createServerFoooo();";
            var appDir = "/app";
            var appOutputDir = "/tmp/app-output";
            var script = new ShellScriptBuilder()
                .CreateDirectory(appDir)
                .CreateFile($"{appDir}/server.js", serverJsWithErrors)
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
                commandToExecuteOnRun: "/bin/bash",
                commandArguments:
                new[]
                {
                    "-c",
                    "\"" +
                    script +
                    "\""
                });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.False(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public override void GeneratesScript_AndBuilds_WhenExplicitLanguageAndVersion_AreProvided()
        {
            // Arrange
            var volume = DockerVolume.Create(Path.Combine(_hostSamplesDir, "nodejs", "webfrontend"));
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir} -l nodejs --language-version 8.2.1")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
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
        public override void CanBuild_UsingScriptGeneratedBy_ScriptOnlyOption()
        {
            // Arrange
            var volume = DockerVolume.Create(Path.Combine(_hostSamplesDir, "nodejs", "webfrontend"));
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var generatedScript = "/tmp/build.sh";
            var tempDir = "/tmp/" + Guid.NewGuid();
            var script = new ShellScriptBuilder()
                .AddScriptCommand($"{appDir} > {generatedScript}")
                .SetExecutePermissionOnFile(generatedScript)
                .CreateDirectory(tempDir)
                .AddCommand($"{generatedScript} {appDir} {appOutputDir} {tempDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
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
        public void CanBuild_UsingScriptGeneratedBy_ScriptOnlyOption_AndWhenExplicitLanguageAndVersion_AreProvided()
        {
            // Arrange
            var volume = DockerVolume.Create(Path.Combine(_hostSamplesDir, "nodejs", "webfrontend"));
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var generatedScript = "/tmp/build.sh";
            var tempDir = "/tmp/" + Guid.NewGuid();
            var script = new ShellScriptBuilder()
                .AddScriptCommand($"{appDir} -l nodejs --language-version 8.2.1 > {generatedScript}")
                .SetExecutePermissionOnFile(generatedScript)
                .CreateDirectory(tempDir)
                .AddCommand($"{generatedScript} {appDir} {appOutputDir} {tempDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
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
            var volume = DockerVolume.Create(Path.Combine(_hostSamplesDir, "nodejs", "webfrontend"));
            var appDir = volume.ContainerDir;
            var intermediateDir = "/tmp/app-intermediate";
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir} -i {intermediateDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
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
            var volume = DockerVolume.Create(Path.Combine(_hostSamplesDir, "nodejs", "webfrontend"));
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir}")
                .AddDirectoryExistsCheck($"{appDir}/node_modules")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
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
            var volume = DockerVolume.Create(Path.Combine(_hostSamplesDir, "nodejs", "webfrontend"));
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
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
        public void Build_ExecutesPreAndPostBuildScripts_WithinBenvContext()
        {
            // Arrange
            var volume = DockerVolume.Create(Path.Combine(_hostSamplesDir, "nodejs", "webfrontend"));
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
                sw.WriteLine("echo \"Pre-build script: $node\"");
                sw.WriteLine("echo \"Pre-build script: $npm\"");
            }
            using (var sw = File.AppendText(Path.Combine(scriptsDir.FullName, "postbuild.sh")))
            {
                sw.NewLine = "\n";
                sw.WriteLine("#!/bin/bash");
                sw.WriteLine("echo \"Post-build script: $node\"");
                sw.WriteLine("echo \"Post-build script: $npm\"");
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
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -l nodejs --language-version 6")
                .ToString();

            // Act
            var result = _dockerCli.Run(
                Settings.BuildImageName,
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
                    Assert.Matches(@"Pre-build script: /opt/nodejs/6.\d+.\d+/bin/node", result.Output);
                    Assert.Matches(@"Pre-build script: /opt/nodejs/6.\d+.\d+/bin/npm", result.Output);
                    Assert.Matches(@"Post-build script: /opt/nodejs/6.\d+.\d+/bin/node", result.Output);
                    Assert.Matches(@"Post-build script: /opt/nodejs/6.\d+.\d+/bin/npm", result.Output);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void TestRegex()
        {
            Assert.Matches(@"Pre-build script: /opt/nodejs/6.\d+.\d+/bin/node", @"
a
d
d
d
d
d
Pre-build script: /opt/nodejs/6.11.0/bin/node
d
a

");
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