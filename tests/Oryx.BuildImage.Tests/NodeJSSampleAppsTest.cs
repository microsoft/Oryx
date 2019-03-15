// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class NodeJSSampleAppsTest : SampleAppsTestBase
    {
        private static readonly string SampleAppName = "webfrontend";

        public NodeJSSampleAppsTest(ITestOutputHelper output) :
            base(output, new DockerCli(new EnvironmentVariable[] { new EnvironmentVariable(LoggingConstants.AppServiceAppNameEnvironmentVariableName, SampleAppName) }))
        {
        }

        private DockerVolume CreateWebFrontEndVolume() => DockerVolume.Create(Path.Combine(_hostSamplesDir, "nodejs", SampleAppName));

        [Fact]
        public void GeneratesScript_AndBuilds()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
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
        public void Builds_AndCopiesContentToOutputDirectory_Recursively()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
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
        public void Build_CopiesOutput_ToNestedOutputDirectory()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
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
        public void Build_ReplacesContentInDestinationDir_WhenDestinationDirIsNotEmpty()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
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
            //tmp
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

        [Fact(Skip = "structured data is not logged as custom dimension in file, 801985")]
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
                    Assert.Contains("Could not detect the language from repo", result.StdErr);
                },
                result.GetDebugInfo());
        }

        [Fact(Skip = "structured data is not logged as custom dimension in file, 801985")]
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
        public void ErrorDuringBuild_ResultsIn_NonSuccessfulExitCode()
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
        public void GeneratesScript_AndBuilds_WhenExplicitLanguageAndVersion_AreProvided()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
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
        public void CanBuild_UsingScriptGeneratedBy_ScriptOnlyOption()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
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
            var volume = CreateWebFrontEndVolume();
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
        public void GeneratesScript_AndBuilds_UsingSuppliedIntermediateDir()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
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
        public void GeneratesScriptAndBuilds_WhenSourceAndDestinationFolders_AreSame()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
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

        [Fact(Skip = "Work item 819489 - output folder as a subdirectory of source is not yet supported.)")]
        public void GeneratesScriptAndBuilds_WhenDestination_IsSubDirectoryOfSource()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/output";
            var buildScript = new ShellScriptBuilder()
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
                    buildScript
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
            var volume = CreateWebFrontEndVolume();
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
                    Assert.Matches(@"Pre-build script: /opt/nodejs/6.\d+.\d+/bin/node", result.StdOut);
                    Assert.Matches(@"Pre-build script: /opt/nodejs/6.\d+.\d+/bin/npm", result.StdOut);
                    Assert.Matches(@"Post-build script: /opt/nodejs/6.\d+.\d+/bin/node", result.StdOut);
                    Assert.Matches(@"Post-build script: /opt/nodejs/6.\d+.\d+/bin/npm", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact]
        public void BuildsNodeApp_AndZipsNodeModules_IfZipNodeModulesIsTrue_UsingLegacyEnvironmentVariableName()
        {
            // NOTE: Use intermediate directory(which here is local to container) to avoid errors like 
            //  "tar: node_modules/form-data: file changed as we read it"
            // related to zipping files on a folder which is volume mounted.

            // Arrange
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddCommand("export ENABLE_NODE_MODULES_ZIP=true")
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/node_modules.zip")
                .AddDirectoryDoesNotExistCheck($"{appOutputDir}/node_modules")
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
        public void BuildsNodeApp_AndZipsNodeModules_IfZipNodeModulesIsTrue()
        {
            // NOTE: Use intermediate directory(which here is local to container) to avoid errors like 
            //  "tar: node_modules/form-data: file changed as we read it"
            // related to zipping files on a folder which is volume mounted.

            // Arrange
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddCommand("export ORYX_ZIP_NODE_MODULES=true")
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddFileExistsCheck($"{appOutputDir}/node_modules.zip")
                .AddDirectoryDoesNotExistCheck($"{appOutputDir}/node_modules")
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
        public void BuildsNodeApp_AndDoesNotZipNodeModules_IfZipNodeModulesIsFalse()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddCommand("export ORYX_ZIP_NODE_MODULES=false")
                .AddBuildCommand($"{appDir} -o {appOutputDir}")
                .AddFileDoesNotExistCheck($"{appOutputDir}/node_modules.zip")
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
    }
}