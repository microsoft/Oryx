// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Python
{
    public class PythonPlatformTests
    {
        [Fact]
        public void GeneratedScript_DoesNotUseVenv()
        {
            // Arrange
            var scriptGenerator = CreatePlatformInstance();
            var repo = new MemorySourceRepo();
            repo.AddFile("", PythonConstants.RequirementsFileName);
            repo.AddFile("print(1)", "bla.py");
            var context = new BuildScriptGeneratorContext { SourceRepo = repo };

            // Act
            var snippet = scriptGenerator.GenerateBashBuildScriptSnippet(context);

            // Assert
            Assert.NotNull(snippet);
            Assert.Contains("Python Virtual Environment", snippet.BashBuildScriptSnippet);
            Assert.True(scriptGenerator.IsCleanRepo(repo));
        }

        [Theory]
        [InlineData(null, "bla.tar.gz")]
        [InlineData("tar-gz", "bla.tar.gz")]
        [InlineData("zip", "bla.zip")]
        public void ExlcudedDirs_DoesNotContainVirtualEnvDir_IfCompressVirtualEnv_IsEnabled(
            string compressOption,
            string compressedVirtualEnvFileName)
        {
            // Arrange
            var scriptGenerator = CreatePlatformInstance();
            var repo = new MemorySourceRepo();
            repo.AddFile("", PythonConstants.RequirementsFileName);
            var venvName = "bla";
            var context = new BuildScriptGeneratorContext
            {
                SourceRepo = repo,
                Properties = new Dictionary<string, string> {
                    { "virtualenv_name", venvName },
                    { "compress_virtualenv", compressOption }
                }
            };

            // Act
            var excludedDirs = scriptGenerator.GetDirectoriesToExcludeFromCopyToBuildOutputDir(context);

            // Assert
            Assert.NotNull(excludedDirs);
            Assert.Contains(venvName, excludedDirs);
            Assert.DoesNotContain(compressedVirtualEnvFileName, excludedDirs);
        }

        private PythonPlatform CreatePlatformInstance(string defaultVersion = null)
        {
            var testEnv = new TestEnvironment();
            testEnv.Variables[PythonConstants.PythonDefaultVersionEnvVarName] = defaultVersion;

            var nodeVersionProvider = new TestVersionProvider(new[] { Common.PythonVersions.Python37Version });

            var scriptGeneratorOptions = Options.Create(new PythonScriptGeneratorOptions());
            var optionsSetup = new PythonScriptGeneratorOptionsSetup(testEnv);
            optionsSetup.Configure(scriptGeneratorOptions.Value);

            return new PythonPlatform(scriptGeneratorOptions, nodeVersionProvider, testEnv, NullLogger<PythonPlatform>.Instance, null);
        }
    }
}