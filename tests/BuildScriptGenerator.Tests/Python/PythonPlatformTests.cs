// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

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
            Assert.DoesNotContain("Python Virtual Environment", snippet.BashBuildScriptSnippet);
            Assert.True(scriptGenerator.IsCleanRepo(repo));
        }

        private IProgrammingPlatform CreatePlatformInstance(string defaultVersion = null)
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