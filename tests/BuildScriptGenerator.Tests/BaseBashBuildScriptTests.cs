// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class BaseBashBuildScriptTests
    {
        [Fact]
        public void BuildSnippets_ShouldBeIncluded_InOrder()
        {
            // Arrange
            const string script1 = "abcdefg";
            const string script2 = "123456";
            var scriptProps = new BaseBashBuildScriptProperties()
            {
                BuildScriptSnippets = new List<string>() { script1, script2 }
            };

            // Act
            var script = TemplateHelper.Render(TemplateHelper.TemplateResource.BaseBashScript, scriptProps);

            // Assert
            var indexOfScript1 = script.IndexOf(script1);
            var indexOfScript2 = script.IndexOf(script2);
            Assert.True(indexOfScript1 < indexOfScript2);
            Assert.DoesNotContain("Executing pre-build script", script);
            Assert.DoesNotContain("Executing post-build script", script);
        }

        [Fact]
        public void PrePostBuildScripts_ShouldBeIncluded_IfSupplied()
        {
            // Arrange
            const string script1 = "abcdefg";
            const string script2 = "hijklmn";
            var scriptProps = new BaseBashBuildScriptProperties
            {
                PreBuildCommand = script1,
                PostBuildCommand = script2,
            };

            // Act
            var script = TemplateHelper.Render(TemplateHelper.TemplateResource.BaseBashScript, scriptProps);

            // Assert
            Assert.Contains("Executing pre-build command", script);
            Assert.Contains(script1, script);
            Assert.Contains("Executing post-build command", script);
            Assert.Contains(script2, script);
        }

        [Fact]
        public void DeploymentProgressPhases_AreEmittedAtSharedTruthfulBoundaries()
        {
            // Arrange
            var scriptProps = new BaseBashBuildScriptProperties
            {
                BuildScriptSnippets = new[] { "build-snippet" },
                PreBuildCommand = "pre-build-command",
                PostBuildCommand = "post-build-command",
                CopySourceDirectoryContentToDestinationDirectory = true,
                CompressDestinationDir = true,
                ManifestFileName = "oryx-manifest.toml",
                BuildProperties = new Dictionary<string, string>
                {
                    ["property"] = "value",
                },
            };

            // Act
            var script = TemplateHelper.Render(TemplateHelper.TemplateResource.BaseBashScript, scriptProps);

            // Assert
            AssertPhasePrecedes(script, "pre_build", "pre-build-command");
            AssertPhasePrecedes(script, "build.execute", "build-snippet");
            AssertPhasePrecedes(script, "post_build", "post-build-command");
            AssertPhasePrecedes(script, "output.prepare", "Using standard output preparation");
            AssertPhasePrecedes(script, "manifest.write", "Creating a manifest file");
            Assert.Equal(2, CountOccurrences(script, "oryx_report_phase \"output.compress\""));
            AssertPhasesAreOrdered(
                script,
                "pre_build",
                "build.execute",
                "post_build",
                "output.prepare",
                "output.compress",
                "manifest.write");

            Assert.DoesNotContain("oryx_report_phase \"script.generate\"", script);
            Assert.DoesNotContain("oryx_report_phase \"dependencies.restore\"", script);
            Assert.DoesNotContain("build_started", script);
            Assert.DoesNotContain("build_completed", script);
        }

        [Fact]
        public void DeploymentProgressPhases_AreOmittedWhenOptionalBoundariesAreAbsent()
        {
            // Arrange
            var scriptProps = new BaseBashBuildScriptProperties
            {
                BuildScriptSnippets = new[] { "build-snippet" },
                BuildProperties = new Dictionary<string, string>(),
            };

            // Act
            var script = TemplateHelper.Render(TemplateHelper.TemplateResource.BaseBashScript, scriptProps);

            // Assert
            Assert.Contains("oryx_report_phase \"build.execute\"", script);
            Assert.DoesNotContain("oryx_report_phase \"pre_build\"", script);
            Assert.DoesNotContain("oryx_report_phase \"post_build\"", script);
            Assert.DoesNotContain("oryx_report_phase \"manifest.write\"", script);
        }

        [Fact]
        public void DeploymentProgressShell_DisablesPermanentlyAfterWriterFailure()
        {
            var bash = GetBashPath();
            if (bash == null)
            {
                return;
            }

            var testRoot = Path.Combine(Path.GetTempPath(), "oryx-progress-tests", Guid.NewGuid().ToString());
            var binDir = Path.Combine(testRoot, "bin");
            Directory.CreateDirectory(binDir);
            try
            {
                var renderedScript = TemplateHelper.Render(
                    TemplateHelper.TemplateResource.BaseBashScript,
                    new BaseBashBuildScriptProperties
                    {
                        BuildScriptSnippets = new[] { "true" },
                    });
                var reporterEnd = renderedScript.IndexOf("if [ -f ", StringComparison.Ordinal);
                var reporterScript = renderedScript.Substring(0, reporterEnd) +
                    Environment.NewLine +
                    "testRoot=\"$(cd \"$(dirname \"$0\")\" && pwd)\"" + Environment.NewLine +
                    "PATH=\"$testRoot/bin:$PATH\"" + Environment.NewLine +
                    "oryx_report_phase \"build.execute\"" + Environment.NewLine +
                    "oryx_report_phase \"post_build\"" + Environment.NewLine;

                var scriptPath = Path.Combine(testRoot, "test.sh");
                var countPath = Path.Combine(testRoot, "count");
                File.WriteAllText(scriptPath, reporterScript);
                File.WriteAllText(countPath, "0");
                File.WriteAllText(
                    Path.Combine(binDir, "timeout"),
                    "#!/bin/bash\nshift 2\nexec \"$@\"\n");
                File.WriteAllText(
                    Path.Combine(binDir, "oryx"),
                    "#!/bin/bash\n" +
                    "countFile=\"$(cd \"$(dirname \"$0\")/..\" && pwd)/count\"\n" +
                    "count=$(cat \"$countFile\")\n" +
                    "printf '%s' \"$((count + 1))\" > \"$countFile\"\n" +
                    "exit 1\n");

                var chmodResult = ProcessHelper.RunProcess(
                    bash,
                    new[] { "-c", "chmod +x test.sh bin/timeout bin/oryx" },
                    testRoot,
                    TimeSpan.FromSeconds(10));
                Assert.Equal(0, chmodResult.ExitCode);

                var result = ProcessHelper.RunProcess(
                    bash,
                    new[] { scriptPath },
                    testRoot,
                    TimeSpan.FromSeconds(10));

                Assert.Equal(0, result.ExitCode);
                Assert.True(string.IsNullOrWhiteSpace(result.Output));
                Assert.True(string.IsNullOrWhiteSpace(result.Error));
                Assert.Equal("0", File.ReadAllText(countPath));

                result = ProcessHelper.RunProcess(
                    bash,
                    new[]
                    {
                        "-c",
                        "ORYX_PROGRESS_ENDPOINT=file:/tmp/progress.json " +
                        "ORYX_PROGRESS_OPERATION_ID=operation-123 ./test.sh",
                    },
                    testRoot,
                    TimeSpan.FromSeconds(10));

                Assert.Equal(0, result.ExitCode);
                Assert.True(string.IsNullOrWhiteSpace(result.Output));
                Assert.True(string.IsNullOrWhiteSpace(result.Error));
                Assert.Equal("1", File.ReadAllText(countPath));

                File.WriteAllText(countPath, "0");
                const string PrivateEndpoint = "file:/tmp/private-progress.json";
                const string PrivateOperationId = "private-operation-123";
                result = ProcessHelper.RunProcess(
                    bash,
                    new[]
                    {
                        "-c",
                        $"ORYX_PROGRESS_ENDPOINT={PrivateEndpoint} " +
                        $"ORYX_PROGRESS_OPERATION_ID={PrivateOperationId} bash -x ./test.sh",
                    },
                    testRoot,
                    TimeSpan.FromSeconds(10));

                Assert.Equal(0, result.ExitCode);
                Assert.DoesNotContain(PrivateEndpoint, result.Error);
                Assert.DoesNotContain(PrivateOperationId, result.Error);
                Assert.Equal("1", File.ReadAllText(countPath));
            }
            finally
            {
                Directory.Delete(testRoot, recursive: true);
            }
        }

        [Fact]
        public void GeneratedBuildScript_HasValidBashSyntax()
        {
            var bash = GetBashPath();
            if (bash == null)
            {
                return;
            }

            var script = TemplateHelper.Render(
                TemplateHelper.TemplateResource.BaseBashScript,
                new BaseBashBuildScriptProperties
                {
                    BuildScriptSnippets = new[] { "true" },
                    BuildProperties = new Dictionary<string, string>
                    {
                        ["property"] = "value",
                    },
                    ManifestFileName = "oryx-manifest.toml",
                });
            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, script);
                var result = ProcessHelper.RunProcess(
                    bash,
                    new[] { "-n", path },
                    workingDirectory: null,
                    waitTimeForExit: TimeSpan.FromSeconds(10));

                Assert.Equal(0, result.ExitCode);
                Assert.True(string.IsNullOrWhiteSpace(result.Error));
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static void AssertPhasePrecedes(string script, string phase, string boundary)
        {
            var phaseIndex = script.IndexOf($"oryx_report_phase \"{phase}\"", StringComparison.Ordinal);
            var boundaryIndex = script.IndexOf(boundary, StringComparison.Ordinal);
            Assert.True(phaseIndex >= 0);
            Assert.True(boundaryIndex >= 0);
            Assert.True(phaseIndex < boundaryIndex);
        }

        private static void AssertPhasesAreOrdered(string script, params string[] phases)
        {
            var previousIndex = -1;
            foreach (var phase in phases)
            {
                var currentIndex = script.IndexOf(
                    $"oryx_report_phase \"{phase}\"",
                    StringComparison.Ordinal);
                Assert.True(currentIndex > previousIndex);
                previousIndex = currentIndex;
            }
        }

        private static int CountOccurrences(string source, string value)
        {
            var count = 0;
            var startIndex = 0;
            while ((startIndex = source.IndexOf(value, startIndex, StringComparison.Ordinal)) >= 0)
            {
                count++;
                startIndex += value.Length;
            }

            return count;
        }

        private static string GetBashPath()
        {
            if (!OperatingSystem.IsWindows())
            {
                return "bash";
            }

            const string GitBashPath = @"C:\Program Files\Git\bin\bash.exe";
            return File.Exists(GitBashPath) ? GitBashPath : null;
        }
    }
}