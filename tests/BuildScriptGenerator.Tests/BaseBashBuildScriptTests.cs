// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class BaseBashBuildScriptTests
    {
        [Fact]
        public void BuildSnippets_ShouldBeIncluded_InOrder()
        {
            const string script1 = "abcdefg";
            const string script2 = "123456";
            var scriptProps = new BaseBashBuildScriptProperties()
            {
                BuildScriptSnippets = new List<string>() { script1, script2 },
            };

            var script = TemplateHelper.Render(TemplateHelper.TemplateResource.BaseBashScript, scriptProps);

            var indexOfScript1 = script.IndexOf(script1);
            var indexOfScript2 = script.IndexOf(script2);
            Assert.True(indexOfScript1 < indexOfScript2);
            Assert.DoesNotContain("Executing pre-build script", script);
            Assert.DoesNotContain("Executing post-build script", script);
        }

        [Fact]
        public void PrePostBuildScripts_ShouldBeIncluded_IfSupplied()
        {
            const string script1 = "abcdefg";
            const string script2 = "hijklmn";
            var scriptProps = new BaseBashBuildScriptProperties
            {
                PreBuildCommand = script1,
                PostBuildCommand = script2,
            };

            var script = TemplateHelper.Render(TemplateHelper.TemplateResource.BaseBashScript, scriptProps);

            Assert.Contains("Executing pre-build command", script);
            Assert.Contains(script1, script);
            Assert.Contains("Executing post-build command", script);
            Assert.Contains(script2, script);
        }

        [Fact]
        public void ProgressSinkFailure_DoesNotChangeOutputOrExitCode()
        {
            if (!OperatingSystem.IsLinux())
            {
                return;
            }

            using (var fixture = new ScriptFixture(
                "printf 'customer-output\\n'\nprintf 'customer-error\\n' >&2\nexit 37"))
            {
                var disabled = fixture.Run(endpoint: null);
                var missingSink = fixture.Run($"file:{Path.Combine(fixture.Root, "missing.jsonl")}");

                Assert.Equal(37, disabled.ExitCode);
                Assert.Equal(disabled.ExitCode, missingSink.ExitCode);
                AssertEquivalentOutput(disabled.StandardOutput, missingSink.StandardOutput);
                Assert.Equal(disabled.StandardError, missingSink.StandardError);
            }
        }

        [Fact]
        public void ProgressWriter_RejectsSymlinkAndSymlinkAncestor()
        {
            if (!OperatingSystem.IsLinux())
            {
                return;
            }

            using (var fixture = new ScriptFixture(
                "$ORYX_PROGRESS_PHASE_COMMAND \"build.execute\"\nprintf 'customer-output\\n'"))
            {
                var targetPath = Path.Combine(fixture.Root, "target.jsonl");
                File.WriteAllText(targetPath, string.Empty);
                var symlinkPath = Path.Combine(fixture.Root, "link.jsonl");
                File.CreateSymbolicLink(symlinkPath, targetPath);

                var realDirectory = Path.Combine(fixture.Root, "real");
                Directory.CreateDirectory(realDirectory);
                var ancestorTargetPath = Path.Combine(realDirectory, "progress.jsonl");
                File.WriteAllText(ancestorTargetPath, string.Empty);
                var directorySymlink = Path.Combine(fixture.Root, "linked-directory");
                Directory.CreateSymbolicLink(directorySymlink, realDirectory);

                var disabled = fixture.Run(endpoint: null);
                var symlink = fixture.Run($"file:{symlinkPath}");
                var ancestor = fixture.Run(
                    $"file:{Path.Combine(directorySymlink, "progress.jsonl")}");

                Assert.Equal(disabled.ExitCode, symlink.ExitCode);
                AssertEquivalentOutput(disabled.StandardOutput, symlink.StandardOutput);
                Assert.Equal(disabled.StandardError, symlink.StandardError);
                Assert.Equal(disabled.ExitCode, ancestor.ExitCode);
                AssertEquivalentOutput(disabled.StandardOutput, ancestor.StandardOutput);
                Assert.Equal(disabled.StandardError, ancestor.StandardError);
                Assert.Empty(File.ReadAllBytes(targetPath));
                Assert.Empty(File.ReadAllBytes(ancestorTargetPath));
            }
        }

        [Fact]
        public void ProgressEnabled_EmitsUniqueOrderedTransitions_WithoutLeakingEnvironment()
        {
            if (!OperatingSystem.IsLinux())
            {
                return;
            }

            const string snippet =
                "if /usr/bin/env | /usr/bin/grep -q '^ORYX_PROGRESS_'; then exit 91; fi\n" +
                "$ORYX_PROGRESS_PHASE_COMMAND \"dependencies.restore\"\n" +
                "$ORYX_PROGRESS_PHASE_COMMAND \"dependencies.restore\"\n" +
                "$ORYX_PROGRESS_PHASE_COMMAND \"pre_build\"\n" +
                "$ORYX_PROGRESS_PHASE_COMMAND \"build.execute\"\n" +
                "printf 'customer-output\\n'";
            using (var fixture = new ScriptFixture(snippet))
            {
                var progressPath = Path.Combine(fixture.Root, "progress.jsonl");
                File.WriteAllText(progressPath, string.Empty);

                var disabled = fixture.Run(endpoint: null);
                var enabled = fixture.Run($"file:{progressPath}");

                Assert.Equal(0, enabled.ExitCode);
                AssertEquivalentOutput(disabled.StandardOutput, enabled.StandardOutput);
                Assert.Equal(disabled.StandardError, enabled.StandardError);

                var lines = File.ReadAllLines(progressPath);
                Assert.Collection(
                    lines,
                    line => AssertPhase(line, "dependencies.restore"),
                    line => AssertPhase(line, "build.execute"),
                    line => AssertPhase(line, "output.prepare"),
                    line => AssertPhase(line, "manifest.write"),
                    AssertBuildCompleted);
            }
        }

        [Fact]
        public void ProgressWriter_DisablesPermanentlyAfterAppendFailure()
        {
            if (!OperatingSystem.IsLinux())
            {
                return;
            }

            using (var fixture = new ScriptFixture(
                "$ORYX_PROGRESS_PHASE_COMMAND \"build.execute\"\n" +
                "printf 'customer-output\\n'"))
            {
                var progressPath = Path.Combine(fixture.Root, "progress.jsonl");
                var fullFile = new byte[32768];
                File.WriteAllBytes(progressPath, fullFile);

                var disabled = fixture.Run(endpoint: null);
                var enabled = fixture.Run($"file:{progressPath}");

                Assert.Equal(0, enabled.ExitCode);
                AssertEquivalentOutput(disabled.StandardOutput, enabled.StandardOutput);
                Assert.Equal(disabled.StandardError, enabled.StandardError);
                Assert.Equal(fullFile, File.ReadAllBytes(progressPath));
            }
        }

        [Fact]
        public void ProgressEnabled_PreservesInheritedBashEnvironmentExitTrap()
        {
            if (!OperatingSystem.IsLinux())
            {
                return;
            }

            using (var fixture = new ScriptFixture(
                "$ORYX_PROGRESS_PHASE_COMMAND \"build.execute\"\n" +
                "exit 43"))
            {
                var bashEnvironmentPath = Path.Combine(fixture.Root, "bash-environment.sh");
                File.WriteAllText(
                    bashEnvironmentPath,
                    $"trap 'printf cleanup >> \"{fixture.Source}/cleanup\"; " +
                    "printf \"customer-cleanup\\n\" >&2' EXIT\n" +
                    "printf 'inherited-bootstrap\\n'\n");
                var progressPath = Path.Combine(fixture.Root, "progress.jsonl");
                File.WriteAllText(progressPath, string.Empty);

                var disabled = fixture.Run(
                    endpoint: null,
                    bashEnvironmentPath: bashEnvironmentPath);
                var enabled = fixture.Run($"file:{progressPath}", bashEnvironmentPath);

                Assert.Equal(43, enabled.ExitCode);
                Assert.Equal(disabled.ExitCode, enabled.ExitCode);
                AssertEquivalentOutput(disabled.StandardOutput, enabled.StandardOutput);
                Assert.Equal(disabled.StandardError, enabled.StandardError);
                Assert.Equal(
                    "cleanupcleanup",
                    File.ReadAllText(Path.Combine(fixture.Source, "cleanup")));
                Assert.Single(File.ReadAllLines(progressPath));
            }
        }

        [Fact]
        public void ProgressEnabled_DoesNotCallOrReplaceInheritedProgressFunction()
        {
            if (!OperatingSystem.IsLinux())
            {
                return;
            }

            using (var fixture = new ScriptFixture(
                "$ORYX_PROGRESS_PHASE_COMMAND \"build.execute\"\n" +
                "oryx_progress_transition customer-transition customer-value\n" +
                "oryx_progress_phase_started customer-phase\n" +
                "oryx_progress_build_completed"))
            {
                var bashEnvironmentPath = Path.Combine(fixture.Root, "bash-environment.sh");
                File.WriteAllText(
                    bashEnvironmentPath,
                    "oryx_progress_transition() { " +
                    "printf 'customer-transition:%s:%s\\n' \"$1\" \"$2\"; }\n" +
                    "oryx_progress_phase_started() { " +
                    "printf 'customer-phase:%s\\n' \"$1\"; }\n" +
                    "oryx_progress_build_completed() { " +
                    "printf 'customer-completed\\n'; }\n");
                var progressPath = Path.Combine(fixture.Root, "progress.jsonl");
                File.WriteAllText(progressPath, string.Empty);

                var disabled = fixture.Run(
                    endpoint: null,
                    bashEnvironmentPath: bashEnvironmentPath);
                var enabled = fixture.Run($"file:{progressPath}", bashEnvironmentPath);

                Assert.Equal(0, enabled.ExitCode);
                AssertEquivalentOutput(disabled.StandardOutput, enabled.StandardOutput);
                Assert.Equal(disabled.StandardError, enabled.StandardError);
                Assert.Contains(
                    "customer-transition:customer-transition:customer-value\n",
                    enabled.StandardOutput);
                Assert.Contains("customer-phase:customer-phase\n", enabled.StandardOutput);
                Assert.Contains("customer-completed\n", enabled.StandardOutput);
                Assert.Empty(File.ReadAllBytes(progressPath));
            }
        }

        [Fact]
        public void ProgressEnabled_DoesNotExposeProtocolMetadataThroughInheritedXtrace()
        {
            if (!OperatingSystem.IsLinux())
            {
                return;
            }

            using (var fixture = new ScriptFixture(
                "$ORYX_PROGRESS_PHASE_COMMAND \"build.execute\"\n" +
                "case \"$-\" in *x*) printf 'xtrace-restored\\n' ;; *) exit 92 ;; esac"))
            {
                var bashEnvironmentPath = Path.Combine(fixture.Root, "bash-environment.sh");
                File.WriteAllText(bashEnvironmentPath, "set -x\n");
                var progressPath = Path.Combine(fixture.Root, "progress.jsonl");
                File.WriteAllText(progressPath, string.Empty);

                var enabled = fixture.Run($"file:{progressPath}", bashEnvironmentPath);

                Assert.Equal(0, enabled.ExitCode);
                Assert.Contains("xtrace-restored\n", enabled.StandardOutput);
                Assert.DoesNotContain(progressPath, enabled.StandardError);
                Assert.DoesNotContain("operation-1", enabled.StandardError);
                Assert.DoesNotContain("schemaVersion", enabled.StandardError);
                Assert.Collection(
                    File.ReadAllLines(progressPath),
                    line => AssertPhase(line, "build.execute"),
                    line => AssertPhase(line, "output.prepare"),
                    line => AssertPhase(line, "manifest.write"),
                    AssertBuildCompleted);
            }
        }

        [Fact]
        public void ProgressEnvironment_RemainsUntouchedWithoutCliHandoff()
        {
            if (!OperatingSystem.IsLinux())
            {
                return;
            }

            using (var fixture = new ScriptFixture(
                "printf 'endpoint=%s\\noperation=%s\\n' " +
                "\"$ORYX_PROGRESS_ENDPOINT\" \"$ORYX_PROGRESS_OPERATION_ID\""))
            {
                var result = fixture.Run(
                    endpoint: "customer-endpoint",
                    transferOwnership: false);

                Assert.Equal(0, result.ExitCode);
                Assert.Contains("endpoint=customer-endpoint\n", result.StandardOutput);
                Assert.Contains("operation=operation-1\n", result.StandardOutput);
            }
        }

        [Fact]
        public void ProgressEnabled_PreservesInheritedMonitorMode()
        {
            if (!OperatingSystem.IsLinux())
            {
                return;
            }

            using (var fixture = new ScriptFixture(
                "$ORYX_PROGRESS_PHASE_COMMAND \"build.execute\"\n" +
                "case \"$-\" in *m*) printf 'monitor-mode-restored\\n' ;; *) exit 93 ;; esac"))
            {
                var bashEnvironmentPath = Path.Combine(fixture.Root, "bash-environment.sh");
                File.WriteAllText(bashEnvironmentPath, "set -m\n");
                var progressPath = Path.Combine(fixture.Root, "progress.jsonl");
                File.WriteAllText(progressPath, string.Empty);

                var enabled = fixture.Run($"file:{progressPath}", bashEnvironmentPath);

                Assert.Equal(0, enabled.ExitCode);
                Assert.Contains("monitor-mode-restored\n", enabled.StandardOutput);
                Assert.Collection(
                    File.ReadAllLines(progressPath),
                    line => AssertPhase(line, "build.execute"),
                    line => AssertPhase(line, "output.prepare"),
                    line => AssertPhase(line, "manifest.write"),
                    AssertBuildCompleted);
            }
        }

        private static void AssertPhase(string record, string phase)
        {
            using (var document = JsonDocument.Parse(record))
            {
                var root = document.RootElement;
                Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
                Assert.Equal("phase_started", root.GetProperty("eventType").GetString());
                Assert.Equal("operation-1", root.GetProperty("operationId").GetString());
                Assert.Equal(phase, root.GetProperty("phase").GetString());
                Assert.Equal(5, root.EnumerateObject().Count());
            }
        }

        private static void AssertBuildCompleted(string record)
        {
            using (var document = JsonDocument.Parse(record))
            {
                var root = document.RootElement;
                Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
                Assert.Equal("build_completed", root.GetProperty("eventType").GetString());
                Assert.Equal("operation-1", root.GetProperty("operationId").GetString());
                Assert.Equal("succeeded", root.GetProperty("outcome").GetString());
                Assert.Equal(5, root.EnumerateObject().Count());
            }
        }

        private static void AssertEquivalentOutput(string expected, string actual)
        {
            Assert.Equal(NormalizeElapsedTimes(expected), NormalizeElapsedTimes(actual));
        }

        private static string NormalizeElapsedTimes(string output)
        {
            return Regex.Replace(
                output,
                @"done in \d+ sec\(s\)\.",
                "done in <elapsed> sec(s).");
        }

        private sealed class ScriptFixture : IDisposable
        {
            public ScriptFixture(string snippet)
            {
                this.Root = Path.Combine(Path.GetTempPath(), "oryx-progress-" + Guid.NewGuid().ToString("N"));
                this.Source = Path.Combine(this.Root, "source");
                this.Destination = Path.Combine(this.Root, "destination");
                Directory.CreateDirectory(this.Source);
                Directory.CreateDirectory(this.Destination);
                this.ScriptPath = Path.Combine(this.Root, "build.sh");
                var script = TemplateHelper.Render(
                    TemplateHelper.TemplateResource.BaseBashScript,
                    new BaseBashBuildScriptProperties
                    {
                        BuildScriptSnippets = new[] { snippet },
                        LoggerPath = Path.Combine(this.Root, "missing-logger.sh"),
                        ManifestFileName = "oryx-manifest.toml",
                        ManifestDir = this.Destination,
                    });
                File.WriteAllText(this.ScriptPath, script.Replace("\r\n", "\n"));
            }

            public string Root { get; }

            public string Source { get; }

            private string Destination { get; }

            private string ScriptPath { get; }

            public ProcessResult Run(
                string endpoint,
                string bashEnvironmentPath = null,
                bool transferOwnership = true)
            {
                var startInfo = new ProcessStartInfo("/bin/bash")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                };
                startInfo.ArgumentList.Add(this.ScriptPath);
                startInfo.ArgumentList.Add(this.Source);
                startInfo.ArgumentList.Add(this.Destination);
                startInfo.ArgumentList.Add(string.Empty);
                startInfo.Environment["DEBIAN_FLAVOR"] = "bookworm";
                if (bashEnvironmentPath == null)
                {
                    startInfo.Environment.Remove("BASH_ENV");
                }
                else
                {
                    startInfo.Environment["BASH_ENV"] = bashEnvironmentPath;
                }
                if (endpoint == null)
                {
                    startInfo.Environment.Remove("ORYX_PROGRESS_ENDPOINT");
                }
                else
                {
                    if (transferOwnership)
                    {
                        startInfo.ArgumentList.Add("oryx-cli");
                    }

                    startInfo.Environment["ORYX_PROGRESS_ENDPOINT"] = endpoint;
                    startInfo.Environment["ORYX_PROGRESS_OPERATION_ID"] = "operation-1";
                }

                using (var process = Process.Start(startInfo))
                {
                    var standardOutput = process.StandardOutput.ReadToEndAsync();
                    var standardError = process.StandardError.ReadToEndAsync();
                    if (!process.WaitForExit(15000))
                    {
                        process.Kill(entireProcessTree: true);
                        Assert.Fail("Generated build script did not exit within 15 seconds.");
                    }

                    return new ProcessResult(
                        process.ExitCode,
                        standardOutput.GetAwaiter().GetResult(),
                        standardError.GetAwaiter().GetResult());
                }
            }

            public void Dispose()
            {
                Directory.Delete(this.Root, recursive: true);
            }
        }

        private sealed class ProcessResult
        {
            public ProcessResult(int exitCode, string standardOutput, string standardError)
            {
                this.ExitCode = exitCode;
                this.StandardOutput = standardOutput;
                this.StandardError = standardError;
            }

            public int ExitCode { get; }

            public string StandardOutput { get; }

            public string StandardError { get; }
        }
    }
}
