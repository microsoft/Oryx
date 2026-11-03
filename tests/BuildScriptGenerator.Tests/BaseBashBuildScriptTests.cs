// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
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
            Assert.True(
                script.IndexOf("Removing existing manifest file", StringComparison.Ordinal) <
                script.IndexOf("oryx_report_phase \"manifest.write\"", StringComparison.Ordinal));
            Assert.Equal(2, CountOccurrences(script, "oryx_report_phase \"output.compress\""));
            AssertPhasesAreOrdered(
                script,
                "pre_build",
                "build.execute",
                "post_build",
                "output.prepare",
                "output.compress",
                "manifest.write");
            foreach (var phase in new[]
            {
                "dotnet.dependencies.restore",
                "dotnet.application.build",
                "nodejs.dependencies.install",
                "nodejs.application.build",
                "python.dependencies.install",
                "php.dependencies.install",
            })
            {
                Assert.Contains($"\"{phase}\"", script);
            }

            var minimalScript = TemplateHelper.Render(
                TemplateHelper.TemplateResource.BaseBashScript,
                new BaseBashBuildScriptProperties
                {
                    BuildScriptSnippets = new[] { "build-snippet" },
                    BuildProperties = new Dictionary<string, string>(),
                });
            Assert.Contains("oryx_report_phase \"build.execute\"", minimalScript);
            Assert.DoesNotContain("oryx_report_phase \"pre_build\"", minimalScript);
            Assert.DoesNotContain("oryx_report_phase \"post_build\"", minimalScript);
            Assert.DoesNotContain("oryx_report_phase \"manifest.write\"", minimalScript);
        }

        [Fact]
        public void DeploymentProgressShell_WritesExactRecordsAndFailsOpen()
        {
            var bash = GetBashPath();
            if (bash == null)
            {
                return;
            }

            var testRoot = Path.Combine(Path.GetTempPath(), "oryx-progress-tests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(testRoot);
            try
            {
                var scriptPath = Path.Combine(testRoot, "test.sh");
                var progressPath = Path.Combine(testRoot, "progress spaces ; [literal].jsonl");
                WriteProgressScript(
                    scriptPath,
                    "test -z \"${ORYX_PROGRESS_FILE+x}\"",
                    "oryx_report_phase \"pre_build\"",
                    "oryx_report_phase \"build.execute\"",
                    "oryx_report_phase \"post_build\"",
                    "oryx_report_phase \"output.prepare\"",
                    "oryx_report_phase \"output.compress\"",
                    "oryx_report_phase \"manifest.write\"",
                    "oryx_report_phase \"unknown\"");
                File.WriteAllBytes(progressPath, Array.Empty<byte>());
                RunBashAndAssertBuildUnaffected(
                    bash,
                    testRoot,
                    "-c",
                    "progressFile=\"$PWD/progress spaces ; [literal].jsonl\"; " +
                    "ORYX_PROGRESS_FILE=\"$progressFile\" bash ./test.sh");

                var expectedPhases = new[]
                {
                    "pre_build",
                    "build.execute",
                    "post_build",
                    "output.prepare",
                    "output.compress",
                    "manifest.write",
                };
                AssertProgressRecords(progressPath, expectedPhases);

                WriteProgressScript(
                    scriptPath,
                    "oryx_report_phase \"build.execute\"",
                    "oryx_report_phase \"post_build\"");

                RunBashAndAssertBuildUnaffected(bash, testRoot, scriptPath);

                var missingPath = Path.Combine(testRoot, "missing.jsonl");
                RunBashAndAssertBuildUnaffected(
                    bash,
                    testRoot,
                    "-c",
                    "ORYX_PROGRESS_FILE=\"$PWD/missing.jsonl\" bash ./test.sh");
                Assert.False(File.Exists(missingPath));

                var relativePath = Path.Combine(testRoot, "relative.jsonl");
                File.WriteAllBytes(relativePath, Array.Empty<byte>());
                RunBashAndAssertBuildUnaffected(
                    bash,
                    testRoot,
                    "-c",
                    "ORYX_PROGRESS_FILE=relative.jsonl bash ./test.sh");
                Assert.Empty(File.ReadAllBytes(relativePath));

                progressPath = Path.Combine(testRoot, "progress.jsonl");
                File.WriteAllBytes(progressPath, Array.Empty<byte>());
                WriteProgressScript(
                    scriptPath,
                    "rm -f \"$ORYX_PROGRESS_FILE_VALUE\"",
                    "oryx_report_phase \"build.execute\"",
                    ": > \"$ORYX_PROGRESS_FILE_VALUE\"",
                    "oryx_report_phase \"post_build\"");
                RunBashAndAssertBuildUnaffected(
                    bash,
                    testRoot,
                    "-c",
                    "ORYX_PROGRESS_FILE=\"$PWD/progress.jsonl\" bash ./test.sh");
                Assert.Empty(File.ReadAllBytes(progressPath));
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

                Assert.True(result.ExitCode == 0, result.Error);
                Assert.True(string.IsNullOrWhiteSpace(result.Error));
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static string RenderProgressReporterScript(params string[] commands)
        {
            var renderedScript = TemplateHelper.Render(
                TemplateHelper.TemplateResource.BaseBashScript,
                new BaseBashBuildScriptProperties
                {
                    BuildScriptSnippets = new[] { "true" },
                });
            var functionStart = renderedScript.IndexOf("oryx_report_phase() {", StringComparison.Ordinal);
            var functionEnd = renderedScript.IndexOf("\n}\n", functionStart, StringComparison.Ordinal);
            Assert.True(functionStart >= 0);
            Assert.True(functionEnd >= 0);

            return renderedScript.Substring(0, functionEnd + 3) +
                string.Join(Environment.NewLine, commands) +
                Environment.NewLine;
        }

        private static void WriteProgressScript(string path, params string[] commands)
        {
            File.WriteAllText(path, RenderProgressReporterScript(commands));
        }

        private static void AssertProgressRecords(string path, string[] expectedPhases)
        {
            var records = File.ReadAllLines(path);
            Assert.Equal(expectedPhases.Length, records.Length);
            for (var index = 0; index < records.Length; index++)
            {
                var match = Regex.Match(
                    records[index],
                    "^\\{\"schemaVersion\":1,\"eventType\":\"phase_started\",\"phase\":\"" +
                    Regex.Escape(expectedPhases[index]) +
                    "\",\"timestampUtc\":\"(?<timestamp>\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}\\.\\d{7}Z)\"\\}$");
                Assert.True(match.Success, records[index]);
                Assert.True(DateTimeOffset.TryParse(
                    match.Groups["timestamp"].Value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out _));
            }
        }

        private static void RunBashAndAssertBuildUnaffected(
            string bash,
            string testRoot,
            params string[] arguments)
        {
            var result = ProcessHelper.RunProcess(
                bash,
                arguments,
                testRoot,
                TimeSpan.FromSeconds(10));
            Assert.Equal(0, result.ExitCode);
            Assert.True(string.IsNullOrWhiteSpace(result.Output), result.Output);
            Assert.True(string.IsNullOrWhiteSpace(result.Error), result.Error);
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
