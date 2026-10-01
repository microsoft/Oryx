// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;
using Microsoft.Oryx.BuildScriptGenerator.Java;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class DeploymentProgressTemplateTests
    {
        [Fact]
        public void BaseTemplate_ContainsBoundedWriterAndOrderedCommonTransitions()
        {
            var script = TemplateHelper.Render(
                TemplateHelper.TemplateResource.BaseBashScript,
                new BaseBashBuildScriptProperties
                {
                    PreBuildCommand = "true",
                    PostBuildCommand = "true",
                    CompressDestinationDir = true,
                    ManifestFileName = "oryx-manifest.toml",
                    ManifestDir = "/tmp",
                });

            Assert.Contains("ORYX_PROGRESS_OWNER=\"${4:-standalone}\"", script);
            Assert.Contains("unset ORYX_PROGRESS_ENDPOINT ORYX_PROGRESS_OPERATION_ID", script);
            Assert.Contains("ORYX_PROGRESS_MAX_EVENTS=32", script);
            Assert.Contains("ORYX_PROGRESS_MAX_RECORD_BYTES=1024", script);
            Assert.Contains("ORYX_PROGRESS_MAX_FILE_BYTES=32768", script);
            Assert.Contains("/usr/bin/setsid /usr/bin/env -u BASH_ENV", script);
            Assert.Contains("disown \"$writerPid\"", script);
            Assert.Contains("/usr/bin/realpath -m -s", script);
            Assert.Contains("$ORYX_PROGRESS_PHASE_COMMAND \"pre_build\"", script);
            Assert.Contains("$ORYX_PROGRESS_PHASE_COMMAND \"post_build\"", script);
            Assert.Contains("$ORYX_PROGRESS_PHASE_COMMAND \"output.prepare\"", script);
            Assert.Contains("$ORYX_PROGRESS_PHASE_COMMAND \"output.compress\"", script);
            Assert.Contains("$ORYX_PROGRESS_PHASE_COMMAND \"manifest.write\"", script);
            Assert.Contains("$ORYX_PROGRESS_TERMINAL_COMMAND", script);
            Assert.DoesNotContain("oryx_progress_phase_completed", script);
            Assert.DoesNotContain("oryx_progress_write()", script);
            Assert.DoesNotContain("trap 'oryx_progress", script);
        }

        [Fact]
        public void PlatformTemplates_EmitOnlyFiniteGenericTransitions()
        {
            var node = TemplateHelper.Render(
                TemplateHelper.TemplateResource.NodeBuildSnippet,
                new NodeBashBuildSnippetProperties
                {
                    PackageInstallCommand = "npm install",
                    PackageInstallerVersionCommand = "npm --version",
                    NpmRunBuildCommand = "npm run build",
                });
            var python = TemplateHelper.Render(
                TemplateHelper.TemplateResource.PythonSnippet,
                new PythonBashBuildSnippetProperties(
                    virtualEnvironmentName: null,
                    virtualEnvironmentModule: null,
                    virtualEnvironmentParameters: null,
                    packagesDirectory: ".python_packages",
                    enableCollectStatic: false,
                    compressVirtualEnvCommand: null,
                    compressedVirtualEnvFileName: null,
                    runPythonPackageCommand: false,
                    pythonVersion: "3.12",
                    dependencyResolutionOutputDir: "/tmp/dependencies"));
            var pythonVirtualEnvironment = TemplateHelper.Render(
                TemplateHelper.TemplateResource.PythonSnippet,
                new PythonBashBuildSnippetProperties(
                    virtualEnvironmentName: "antenv",
                    virtualEnvironmentModule: "venv",
                    virtualEnvironmentParameters: string.Empty,
                    packagesDirectory: ".python_packages",
                    enableCollectStatic: false,
                    compressVirtualEnvCommand: null,
                    compressedVirtualEnvFileName: null,
                    runPythonPackageCommand: false,
                    pythonVersion: "3.12",
                    dependencyResolutionOutputDir: "/tmp/dependencies"));
            var dotnet = TemplateHelper.Render(
                TemplateHelper.TemplateResource.DotNetCoreSnippet,
                new DotNetCoreBashBuildSnippetProperties
                {
                    ProjectFile = "app.csproj",
                    Configuration = "Release",
                });
            var php = TemplateHelper.Render(
                TemplateHelper.TemplateResource.PhpBuildSnippet,
                new PhpBashBuildSnippetProperties
                {
                    ComposerFileExists = true,
                });
            var java = TemplateHelper.Render(
                TemplateHelper.TemplateResource.JavaBuildSnippet,
                new JavaBashBuildSnippetProperties
                {
                    Command = "mvn clean package",
                });

            AssertRestoreAndBuild(node);
            Assert.Contains("$ORYX_PROGRESS_PHASE_COMMAND \"dependencies.restore\"", python);
            AssertRestoreAndBuild(dotnet);
            Assert.Contains("$ORYX_PROGRESS_PHASE_COMMAND \"dependencies.restore\"", php);
            Assert.Contains("$ORYX_PROGRESS_PHASE_COMMAND \"build.execute\"", java);
            var poetryFallbacks = (python + pythonVirtualEnvironment)
                .Split(new[] { "# Fallback to poetry" }, StringSplitOptions.None)
                .Skip(1)
                .ToArray();
            Assert.Equal(2, poetryFallbacks.Length);
            Assert.All(
                poetryFallbacks,
                poetryFallback => Assert.Contains(
                    "$ORYX_PROGRESS_PHASE_COMMAND \"dependencies.restore\"",
                    poetryFallback));

            var combined = node + python + dotnet + php + java;
            Assert.DoesNotContain("$ORYX_PROGRESS_PHASE_COMMAND \"dependencies.restore\" \"", combined);
            Assert.DoesNotContain("$ORYX_PROGRESS_PHASE_COMMAND \"build.execute\" \"", combined);
            Assert.DoesNotContain("oryx_progress_phase_completed", combined);
        }

        private static void AssertRestoreAndBuild(string script)
        {
            Assert.Contains("$ORYX_PROGRESS_PHASE_COMMAND \"dependencies.restore\"", script);
            Assert.Contains("$ORYX_PROGRESS_PHASE_COMMAND \"build.execute\"", script);
        }
    }
}
