// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Oryx.Tests.Common;
using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class DefaultRunScriptGeneratorTest
    {
        [Fact(Skip = "WIP")]
        public void GenerateBashScript_ReturnsScript()
        {
            // Arrange
            var platform = new TestProgrammingPlatform(
                "test",
                new[] { "1.0.0" },
                canGenerateScript: true,
                scriptContent: "script-content");
            var gen = new DefaultRunScriptGenerator(new[] { platform }, NullLogger<DefaultRunScriptGenerator>.Instance);
            var ctx = new RunScriptGeneratorOptions { SourceRepo = new MemorySourceRepo() };

            // Act
            var generatedScript = gen.GenerateBashScript(platform.Name, ctx);

            // Assert
            Assert.Contains("script-content", generatedScript);
        }
    }
}
