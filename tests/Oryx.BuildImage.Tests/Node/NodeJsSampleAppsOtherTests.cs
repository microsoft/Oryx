// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Resources;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Microsoft.Oryx.Tests.Common;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Oryx.BuildImage.Tests
{
    public class NodeJsSampleAppsOtherTests : NodeJSSampleAppsTestBase, IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempRootDir;

        public NodeJsSampleAppsOtherTests(ITestOutputHelper output, TestTempDirTestFixture testTempDirTestFixture)
            : base(output)
        {
            _tempRootDir = testTempDirTestFixture.RootDirPath;
        }

        [Fact, Trait("category", "githubactions")]
        public void PipelineTestInvocationGithubactions()
        {
            var imageTestHelper = new ImageTestHelper();
            GeneratesScript_AndBuilds(imageTestHelper.GetGitHubActionsBuildImage());
        }

        private void GeneratesScript_AndBuilds(string buildImageName)
        {
            // Please note:
            // This test method has at least 1 wrapper function that pases the imageName parameter.

            // Arrange
            var devPackageName = "nodemon";
            var prodPackageName = "express";
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{devPackageName}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{prodPackageName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = buildImageName,
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

        [Fact, Trait("category", "githubactions")]
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
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                // Check the output directory for the sub directory
                .AddFileExistsCheck($"{appOutputDir}/{subDir}/file1.txt")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Fact, Trait("category", "githubactions")]
        public void Build_CopiesOutput_ToOutputDirectory_NestedUnderSourceDirectory()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appDir}/output")
                .AddDirectoryExistsCheck($"{appDir}/output/node_modules")
                .AddDirectoryDoesNotExistCheck($"{appDir}/output/output")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Fact, Trait("category", "gitubhactions")]
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
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddFileExistsCheck($"{appOutputDir}/hi.txt")
                .AddFileExistsCheck($"{appOutputDir}/blah/hi.txt")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        private ShellScriptBuilder SetupEnvironment_ErrorDetectingNodeTest(
            string appDir,
            string appOutputDir,
            string logFile)
        {
            var nodeCode =
            @"var http = require('http'); var server = http.createServer(function(req, res) { res.writeHead(200); " +
            "res.end('Hi oryx');}); server.listen(8080);";

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
                .CreateFile($"{appDir}/idea.js", $"\"{nodeCode}\"")
                .AddBuildCommand($"{appDir} -o {appOutputDir} --log-file {logFile}");

            return script;
        }

        [Fact(Skip = "structured data is not logged as custom dimension in file, 801985"), Trait("category", "latest")]
        public void ErrorDetectingNode_FailedExitCode_StringContentFound()
        {
            var appDir = "/tmp/app1";
            var appOutputDir = "/tmp/app-output";
            var logFile = "/tmp/directory.log";

            var script = SetupEnvironment_ErrorDetectingNodeTest(appDir, appOutputDir, logFile)
                .AddFileExistsCheck(logFile)
                .AddStringExistsInFileCheck("idea.js", logFile)
                .AddStringExistsInFileCheck("app2", logFile)
                .AddStringExistsInFileCheck("app3", logFile)
                .AddStringExistsInFileCheck("2.log", logFile)
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Equal(0, result.ExitCode);
                    Assert.Contains(Labels.UnableToDetectPlatformMessage, result.StdErr);
                },
                result.GetDebugInfo());
        }

        [Fact(Skip = "structured data is not logged as custom dimension in file, 801985"), Trait("category", "latest")]
        public void ErrorDetectingNode_FailedExitCode_StringContentNotFound()
        {
            var appDir = "/tmp/app1";
            var appOutputDir = "/tmp/app-output";
            var logFile = "/tmp/directory.log";

            var script = SetupEnvironment_ErrorDetectingNodeTest(appDir, appOutputDir, logFile)
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} --log-file {logFile}")
                .AddStringDoesNotExistInFileCheck("app4", logFile)
                .AddStringDoesNotExistInFileCheck("3.log", logFile)
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
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

        [Fact, Trait("category", "githubactions")]
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
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", "\"" + script + "\"" }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.False(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void GeneratesScript_AndBuilds_WhenExplicitPlatformAndVersion_AreProvided()
        {
            // Arrange
            var version = "18.2.0";
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var manifestFile = $"{appOutputDir}/{FilePaths.BuildManifestFileName}";
            var osTypeFile = $"{appOutputDir}/{FilePaths.OsTypeFileName}";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} --platform {NodeConstants.PlatformName} --platform-version {version}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddFileExistsCheck(osTypeFile)
                .AddCommand($"cat {manifestFile}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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
                       $"{ManifestFilePropertyKeys.NodeVersion}=\"{version}\"",
                       result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
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
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Fact, Trait("category", "githubactions")]
        public void CanBuild_UsingScriptGeneratedBy_ScriptOnlyOption_AndWhenExplicitPlatformAndVersion_AreProvided()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var generatedScript = "/tmp/build.sh";
            var tempDir = "/tmp/" + Guid.NewGuid();
            var script = new ShellScriptBuilder()
                .AddScriptCommand($"{appDir} --platform {NodeConstants.PlatformName} --platform-version 8.2.1 > {generatedScript}")
                .SetExecutePermissionOnFile(generatedScript)
                .CreateDirectory(tempDir)
                .AddCommand($"{generatedScript} {appDir} {appOutputDir} {tempDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Fact, Trait("category", "githubactions")]
        public void GeneratesScript_AndBuilds_UsingSuppliedIntermediateDir()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var intermediateDir = "/tmp/app-intermediate";
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i {intermediateDir} -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Fact, Trait("category", "githubactions")]
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
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Fact(Skip = "Work item 819489 - output folder as a subdirectory of source is not yet supported.)")]
        [Trait("category", "latest")]
        public void GeneratesScriptAndBuilds_WhenDestination_IsSubDirectoryOfSource()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = $"{appDir}/output";
            var buildScript = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", buildScript }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "latest")]
        public void Build_ExecutesPreAndPostBuildScripts_WithinBenvContext()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
            using (var sw = File.AppendText(
                Path.Combine(volume.MountedHostDir, BuildScriptGeneratorCli.Constants.BuildEnvironmentFileName)))
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
                .AddBuildCommand($"{appDir} --platform {NodeConstants.PlatformName} --platform-version 8")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.BuildImageName,
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Matches(@"Pre-build script: /opt/nodejs/8.\d+.\d+/bin/node", result.StdOut);
                    Assert.Matches(@"Pre-build script: /opt/nodejs/8.\d+.\d+/bin/npm", result.StdOut);
                    Assert.Matches(@"Post-build script: /opt/nodejs/8.\d+.\d+/bin/node", result.StdOut);
                    Assert.Matches(@"Post-build script: /opt/nodejs/8.\d+.\d+/bin/npm", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void BuildsNodeApp_AndZipsNodeModules_WithTarGz_IfZipNodeModulesIsTarGz()
        {
            // NOTE: Use intermediate directory(which here is local to container) to avoid errors like
            //  "tar: node_modules/form-data: file changed as we read it"
            // related to zipping files on a folder which is volume mounted.

            // Arrange
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} -p compress_node_modules=tar-gz")
                .AddFileExistsCheck($"{appOutputDir}/node_modules.tar.gz")
                .AddDirectoryDoesNotExistCheck($"{appOutputDir}/node_modules")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Fact, Trait("category", "githubactions")]
        public void BuildsNodeApp_AndZipsNodeModules_IfCompressNodeModulesIsZip()
        {
            // NOTE: Use intermediate directory(which here is local to container) to avoid errors like
            //  "tar: node_modules/form-data: file changed as we read it"
            // related to zipping files on a folder which is volume mounted.

            // Arrange
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var buildScript = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir} -p compress_node_modules=zip")
                .AddFileExistsCheck($"{appOutputDir}/node_modules.zip")
                .AddFileExistsCheck($"{appOutputDir}/{FilePaths.OsTypeFileName}")
                .AddDirectoryDoesNotExistCheck($"{appOutputDir}/node_modules")
                .AddStringExistsInFileCheck(
                "compressedNodeModulesFile=\"node_modules.zip\"",
                $"{appOutputDir}/{FilePaths.BuildManifestFileName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", buildScript }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void BuildsNodeApp_AndDoesNotZipNodeModules_IfZipNodeModulesIsFalse()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddFileDoesNotExistCheck($"{appOutputDir}/node_modules.zip")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Fact, Trait("category", "githubactions")]
        public void GeneratesScript_AndBuilds_UsingSuppliedPackageDir()
        {
            // Arrange
            var volume = DockerVolume.CreateMirror(
                Path.Combine(_hostSamplesDir, "nodejs", "node-nested-nodemodules"));
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} --package -p {NodePlatform.PackageDirectoryPropertyKey}=another-directory")
                .AddFileExistsCheck($"{appDir}/another-directory/kudu-bug-0.0.0.tgz")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Fact(Skip = "InstallLernaCommand is never executed in NodeBashBuildSnippet.sh.tpl to install lerna globally before use"), Trait("category", "githubactions")]
        public void GeneratesScript_AndBuilds_UsingSuppliedPackageDir_WhenPackageDirAndSourceDirAreSame()
        {
            // Arrange
            var volume = DockerVolume.CreateMirror(
                Path.Combine(_hostSamplesDir, "nodejs", "monorepo-lerna-yarn"));
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                    SettingsKeys.EnableNodeMonorepoBuild,
                    true.ToString())
                .AddBuildCommand($"{appDir} --package -p {NodePlatform.PackageDirectoryPropertyKey}=''")
                .AddFileExistsCheck($"{appDir}/lerna-monorepo-post-1.0.0.tgz")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Fact, Trait("category", "githubactions")]
        public void BuildsNodeApp_AndDoesNotCopyDevDependencies_IfPruneDevDependenciesIsTrue_AndNoProdDependencies()
        {
            // Arrange
            var volume = DockerVolume.CreateMirror(
                Path.Combine(_hostSamplesDir, "nodejs", "azure-pages-sample"));

            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} -p {NodePlatform.PruneDevDependenciesPropertyKey}=true")
                .AddDirectoryDoesNotExistCheck($"{appOutputDir}/node_modules")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Theory, Trait("category", "githubactions")]
        [InlineData("empty-dependencies")]
        [InlineData("no-dependeny-nodes")]
        public void BuildsNodeApp_IfPruneDevDependenciesIsTrue_AndNoProd_OrDevDependencies(string appName)
        {
            // Arrange
            var volume = DockerVolume.CreateMirror(
                Path.Combine(_hostSamplesDir, "nodejs", "app-with-no-dependencies", appName));

            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} -p {NodePlatform.PruneDevDependenciesPropertyKey}=true")
                .AddDirectoryDoesNotExistCheck($"{appOutputDir}/node_modules")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Theory, Trait("category", "githubactions")]
        [InlineData("webfrontend")]
        [InlineData("webfrontend-yarnlock")]
        public void BuildsNodeApp_AndDoesNotCopyDevDependencies_IfPruneDevDependenciesIsTrue(string appName)
        {
            // Arrange
            var volume = DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "nodejs", appName));

            // Make sure there is a package in devDependencies node that we verify is not copied to
            // destination folder
            var unexpectedPackageName = "nodemon";
            var expectedPackageName = "express";
            var packageJsonContent = File.ReadAllText(Path.Combine(volume.OriginalHostDir, "package.json"));
            dynamic packageJson = JsonConvert.DeserializeObject(packageJsonContent);
            Assert.NotNull(packageJson);
            Assert.NotNull(packageJson.devDependencies);
            Assert.NotNull(packageJson.devDependencies.nodemon);

            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} -p {NodePlatform.PruneDevDependenciesPropertyKey}=true")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddFileDoesNotExistCheck($"{appOutputDir}/node_modules.zip")
                .AddDirectoryDoesNotExistCheck($"{appOutputDir}/node_modules/{unexpectedPackageName}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{expectedPackageName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Fact, Trait("category", "githubactions")]
        public void BuildsNodeApp_DoesNotPruneDevDependencies_IfPruneDevDependenciesIsFalse()
        {
            // Arrange
            var volume = DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "nodejs", "webfrontend"));

            // Make sure there is a package in devDependencies node that we verify is still copied to
            // destination folder
            var devPackageName = "nodemon";
            var prodPackageName = "express";
            var packageJsonContent = File.ReadAllText(Path.Combine(volume.OriginalHostDir, "package.json"));
            dynamic packageJson = JsonConvert.DeserializeObject(packageJsonContent);
            Assert.NotNull(packageJson);
            Assert.NotNull(packageJson.devDependencies);
            Assert.NotNull(packageJson.devDependencies.nodemon);

            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand(
                $"{appDir} -i /tmp/int -o {appOutputDir} " +
                $"-p {NodePlatform.PruneDevDependenciesPropertyKey}=false")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{devPackageName}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/{prodPackageName}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Fact, Trait("category", "githubactions")]
        public void BuildsApp_ByRunningCustomBuildCommand_AndSkipNpmInstallCommand()
        {
            // Arrange
            var appName = "node-makefile-sample";
            var volume = DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "nodejs", appName));

            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(SettingsKeys.CustomBuildCommand, "make")
                .AddCommand($"oryx build {appDir}")
                // 'make' command will simply generate an index.html by using reveal.js template.
                .AddFileExistsCheck($"{appDir}/index.html")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("> index.html", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void BuildsApp_ByRunningCustomBuildScript_AndSkipNpmInstallCommand()
        {
            // Arrange
            var appName = "node-makefile-sample";
            var volume = DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "nodejs", appName));
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(SettingsKeys.CustomBuildCommand, $"./customBuildScript.sh")
                .AddCommand($"oryx build {appDir} -i /tmp/int")
                .AddFileExistsCheck($"{appDir}/index.html")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("> index.html", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact(Skip = "https://msazure.visualstudio.com/Antares/_workitems/edit/27122108")]
        [Trait("category", "ltsversions")]
        public void CanBuildAndRunNodeApp_WithWorkspace_UsingYarn2ForBuild()
        {
            // Arrange
            var appName = "nextjs-yarn2-example";
            var volume = DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "nodejs", appName));
            var appOutputDir = "/tmp/nextjs-yarn2-example";
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddCommand($"cd {appDir}")
                .AddCommand($"yarn set version berry")
                .AddCommand($"yarn plugin import workspace-tools@2.2.0")
                .AddCommand($"yarn set version 2.4.1")
                .AddCommand($"oryx build . -o {appOutputDir}")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = Settings.LtsVersionsBuildImageWithRootAccess,
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

        [Fact, Trait("category", "githubactions")]
        public void CanBuildAndRunNodeAppWithoutWorkspace_UsingYarn2ForBuild()
        {
            // Arrange
            var appName = "nextjs-yarn2-example";
            var volume = DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "nodejs", appName));
            var appOutputDir = "/tmp/nextjs-yarn2-example";
            var appDir = volume.ContainerDir;
            var script = new ShellScriptBuilder()
                .AddCommand($"oryx build {appDir} -o {appOutputDir} --platform {NodeConstants.PlatformName} --platform-version 20")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Fact, Trait("category", "githubactions")]
        public void BuildsApp_ByRunningNpmInstall_AndCustomRunBuildCommand()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/webfrontend-output";
            var subDir = Guid.NewGuid();
            var script = new ShellScriptBuilder()
                .SetEnvironmentVariable(
                SettingsKeys.CustomRunBuildCommand,
                $"echo > /tmp/foo.txt")
                .AddCommand($"oryx build {appDir} -i /tmp/int -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddFileExistsCheck($"/tmp/foo.txt")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Fact, Trait("category", "githubactions")]
        public void CanBuildAppHavingAppDynamicsNpmPackage()
        {
            // Arrange
            // Create an app folder with a package.json having the 'appdynamics' package
            var packageJsonContent = "{\"dependencies\": { \"appdynamics\": \"22.11.0\" }}";
            var sampleAppPath = Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(sampleAppPath);
            File.WriteAllText(Path.Combine(sampleAppPath, NodeConstants.PackageJsonFileName), packageJsonContent);
            var volume = DockerVolume.CreateMirror(sampleAppPath);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/appdynamics")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Fact, Trait("category", "githubactions")]
        public void CanBuildAppHavingUsingYarnEngine()
        {
            // Arrange  
            // Create an app folder with a package.json having the yarn engine
            var packageJsonContent = @"{
              ""engines"": {
                ""yarn"": ""~1.22.10""
              }
            }";
            var sampleAppPath = Path.Combine(_tempRootDir, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(sampleAppPath);
            File.WriteAllText(Path.Combine(sampleAppPath, NodeConstants.PackageJsonFileName), packageJsonContent);
            var volume = DockerVolume.CreateMirror(sampleAppPath);
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/output";
            var script = new ShellScriptBuilder()
                .AddBuildCommand($"{appDir} -i /tmp/int -o {appOutputDir}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.True(result.IsSuccess);
                    Assert.Contains("Using Yarn version", result.StdOut);
                },
                result.GetDebugInfo());
        }

        [Fact, Trait("category", "githubactions")]
        public void CanBuildVuePressSampleAppWithPruneDevDependencies()
        {
            // Arrange
            var appName = "vuepress";
            var volume = DockerVolume.CreateMirror(Path.Combine(_hostSamplesDir, "nodejs", appName));
            // Make sure 'vuepress' package is in 'dependencies' node because that is what triggered the original issue
            var packageJsonContent = File.ReadAllText(Path.Combine(volume.OriginalHostDir, "package.json"));
            dynamic packageJson = JsonConvert.DeserializeObject(packageJsonContent);
            Assert.NotNull(packageJson);
            Assert.NotNull(packageJson.dependencies);
            Assert.NotNull(packageJson.dependencies.vuepress);
            Assert.Null(packageJson.devDependencies);

            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/vuepress-output";
            var script = new ShellScriptBuilder()
                .AddCommand(
                $"oryx build {appDir} -i /tmp/int -o {appOutputDir} -p {NodePlatform.PruneDevDependenciesPropertyKey}")
                .AddDirectoryExistsCheck($"{appOutputDir}/node_modules/vuepress")
                .ToString();

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
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

        [Fact, Trait("category", "githubactions")]
        public void GeneratesScript_AndLoggerFormatCheck()
        {
            // Arrange
            var volume = CreateWebFrontEndVolume();
            var appDir = volume.ContainerDir;
            var appOutputDir = "/tmp/" + SampleAppName + "-output";
            var script = new ShellScriptBuilder()
                .AddCommand($"echo RandomText >> {appDir}/Program.cs") // triggers a failure
                .AddBuildCommand(
                $"{appDir} -o {appOutputDir} --package --property package_directory='oryxteststring'")
                .ToString();
            // Regex will match:
            // "yyyy-mm-dd hh:mm:ss"|WARNING|Package directory '/oryxtests/webfrontend/oryxteststring' does not exist.
            Regex regex = new Regex(@"""[0-9]{4}-(0[1-9]|1[0-2])-(0[1-9]|[1-2][0-9]|3[0-1]) (0[0-9]|1[0-9]|2[0-3]):([0-5][0-9]):([0-5][0-9])""\|WARNING\|Package directory '/oryxtests/webfrontend/oryxteststring' does not exist.");

            // Act
            var result = _dockerCli.Run(new DockerRunArguments
            {
                ImageId = _imageHelper.GetGitHubActionsBuildImage(),
                EnvironmentVariables = new List<EnvironmentVariable> { CreateAppNameEnvVar(SampleAppName) },
                Volumes = new List<DockerVolume> { volume },
                CommandToExecuteOnRun = "/bin/bash",
                CommandArguments = new[] { "-c", script }
            });

            // Assert
            RunAsserts(
                () =>
                {
                    Assert.False(result.IsSuccess);
                    Match match = regex.Match(result.StdOut);
                    Assert.True(match.Success);
                },
                result.GetDebugInfo());
        }
    }
}