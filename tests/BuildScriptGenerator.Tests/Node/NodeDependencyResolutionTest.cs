// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Node
{
    public class NodeDependencyResolutionTest
    {
        [Fact]
        public void GeneratedSnippet_PublishesSanitizedResolutionAtomicallyAndFailOpen()
        {
            var text = RenderSnippet("/tmp/dependency-resolution");

            Assert.Contains("npm ls --all --production --json", text);
            Assert.Contains("const packagesByKey = new Map();", text);
            Assert.Contains("JSON.parse(fs.readFileSync(sourcePath, 'utf8')", text);
            Assert.Contains("{ name, version: dependency.version }", text);
            Assert.Contains("writeJson(stagedResolutionPath, { schemaVersion: 1, packages });", text);
            Assert.Contains("manager: 'npm'", text);
            Assert.Contains("dependencyResolutionFilePath", text);
            Assert.Contains("\"$output_dir/dependency-resolution-metadata.json\"", text);
            Assert.Contains("\"$output_dir/dependency-resolution.json\"", text);
            Assert.Contains("\"$output_dir/dependency-resolution.txt\"", text);
            Assert.Contains("staging_suffix=\".$$.${RANDOM}.tmp\"", text);
            Assert.Contains("mv -f -- \"$staged_resolution_file\" \"$resolution_file\"", text);
            Assert.Contains("mv -f -- \"$staged_metadata_file\" \"$metadata_file\"", text);
            Assert.Contains("deployment will continue.", text);
            Assert.True(
                text.IndexOf(
                    "clear_dependency_resolution_artifacts \"$dependencyResolutionOutputDir\"",
                    StringComparison.Ordinal) <
                text.IndexOf(
                    "Running 'npm install'",
                    StringComparison.Ordinal));
            Assert.True(
                text.IndexOf(
                    "rsync -rcE --links \"$prodModulesDirName/node_modules/\" node_modules --delete",
                    StringComparison.Ordinal) <
                text.IndexOf(
                    "publish_dependency_resolution \"$dependencyResolutionOutputDir\"",
                    StringComparison.Ordinal));
            Assert.True(
                text.IndexOf(
                    "publish_dependency_resolution \"$dependencyResolutionOutputDir\"",
                    StringComparison.Ordinal) <
                text.IndexOf(
                    "Archiving existing 'node_modules' folder",
                    StringComparison.Ordinal));
            Assert.DoesNotContain("registry", ExtractNormalizer(text));
            Assert.DoesNotContain("integrity", ExtractNormalizer(text));
        }

        [Fact]
        public void DependencyNormalizer_PreservesScopedNamesAndDistinctVersionsAndSanitizesFields()
        {
            var tempDir = Path.Combine(
                Path.GetTempPath(),
                $"oryx-node-dependency-resolution-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);
            var scriptPath = Path.Combine(tempDir, "normalize.js");
            var sourcePath = Path.Combine(tempDir, "npm-tree.json");
            var destinationPath = Path.Combine(tempDir, "dependency-resolution.json");
            var metadataPath = Path.Combine(tempDir, "dependency-resolution-metadata.json");

            try
            {
                File.WriteAllText(scriptPath, ExtractNormalizer(RenderSnippet("/tmp/output")));
                File.WriteAllText(
                    sourcePath,
                    @"{
  ""name"": ""customer-app"",
  ""version"": ""1.0.0"",
  ""resolved"": ""https://user:secret@example.test/customer-app.tgz"",
  ""dependencies"": {
    ""z-package"": {
      ""version"": ""2.0.0"",
      ""integrity"": ""secret-integrity"",
      ""dependencies"": {
        ""shared"": { ""version"": ""1.0.0"" },
        ""@scope/private"": {
          ""name"": ""@scope/private"",
          ""version"": ""3.4.5"",
          ""resolved"": ""https://token@example.test/private.tgz""
        }
      }
    },
    ""a-package"": {
      ""version"": ""1.0.0"",
      ""dependencies"": {
        ""shared"": { ""version"": ""2.0.0"" },
        ""duplicate"": {
          ""version"": ""5.0.0"",
          ""dependencies"": {
            ""shared"": { ""version"": ""1.0.0"" }
          }
        }
      }
    },
    ""duplicate"": { ""version"": ""5.0.0"" }
  }
}");

                var result = ProcessHelper.RunProcess(
                    "node",
                    new[]
                    {
                        scriptPath,
                        sourcePath,
                        destinationPath,
                        metadataPath,
                        destinationPath,
                    },
                    workingDirectory: tempDir,
                    waitTimeForExit: TimeSpan.FromSeconds(10));

                Assert.True(result.ExitCode == 0, result.Error);
                Assert.Equal(
                    "{\n" +
                    "  \"schemaVersion\": 1,\n" +
                    "  \"packages\": [\n" +
                    "    {\n" +
                    "      \"name\": \"@scope/private\",\n" +
                    "      \"version\": \"3.4.5\"\n" +
                    "    },\n" +
                    "    {\n" +
                    "      \"name\": \"a-package\",\n" +
                    "      \"version\": \"1.0.0\"\n" +
                    "    },\n" +
                    "    {\n" +
                    "      \"name\": \"duplicate\",\n" +
                    "      \"version\": \"5.0.0\"\n" +
                    "    },\n" +
                    "    {\n" +
                    "      \"name\": \"shared\",\n" +
                    "      \"version\": \"1.0.0\"\n" +
                    "    },\n" +
                    "    {\n" +
                    "      \"name\": \"shared\",\n" +
                    "      \"version\": \"2.0.0\"\n" +
                    "    },\n" +
                    "    {\n" +
                    "      \"name\": \"z-package\",\n" +
                    "      \"version\": \"2.0.0\"\n" +
                    "    }\n" +
                    "  ]\n" +
                    "}\n",
                    File.ReadAllText(destinationPath).Replace("\r\n", "\n"));
                Assert.Contains(
                    "\"manager\": \"npm\"",
                    File.ReadAllText(metadataPath));
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [Fact]
        public void GeneratedDependencyResolutionSnippet_HasValidBashSyntax()
        {
            var bash = OperatingSystem.IsWindows()
                ? @"C:\Program Files\Git\bin\bash.exe"
                : "bash";
            if (OperatingSystem.IsWindows() && !File.Exists(bash))
            {
                return;
            }

            var path = Path.Combine(
                Path.GetTempPath(),
                $"oryx-node-dependency-resolution-{Guid.NewGuid():N}.sh");

            try
            {
                File.WriteAllText(path, RenderSnippet("/tmp/dependency-resolution"));
                var result = ProcessHelper.RunProcess(
                    bash,
                    new[] { "-n", path },
                    workingDirectory: null,
                    waitTimeForExit: TimeSpan.FromSeconds(10));

                Assert.True(result.ExitCode == 0, result.Error);
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static string RenderSnippet(string outputDir)
        {
            return TemplateHelper.Render(
                TemplateHelper.TemplateResource.NodeBuildSnippet,
                new NodeBashBuildSnippetProperties
                {
                    DependencyResolutionOutputDir = outputDir,
                    DependencyResolutionOutputDirBashValue = $"'{outputDir}'",
                    DependencyResolutionRequired = true,
                    PackageInstallCommand = "npm install",
                    CompressedNodeModulesFileName = "node_modules.tar.gz",
                    CompressNodeModulesCommand = "tar -zcf",
                });
        }

        private static string ExtractNormalizer(string snippet)
        {
            snippet = snippet.Replace("\r\n", "\n");
            const string startMarker =
                "\"$resolution_file\" <<'NODE' || exit 1\n";
            const string endMarker = "\nNODE\n";
            var start = snippet.IndexOf(startMarker, StringComparison.Ordinal);
            Assert.True(start >= 0);
            start += startMarker.Length;
            var end = snippet.IndexOf(endMarker, start, StringComparison.Ordinal);
            Assert.True(end > start);
            return snippet.Substring(start, end - start);
        }
    }
}
