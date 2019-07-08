// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
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
            Assert.Contains(
                script1 +
                "\n\n# Makes sure every snippet starts in the context of the source directory.\ncd \"$SOURCE_DIR\"\n" +
                script2,
                script); // The template engine uses UNIX-style line endings
            Assert.DoesNotContain("Executing pre-build script", script);
            Assert.DoesNotContain("Executing post-build script", script);
        }

        [Fact]
        public void PrePostBuildScripts_ShouldBeIncluded_IfSupplied()
        {
            // Arrange
            const string script1 = "abcdefg";
            const string script2 = "hijklmn";
            var scriptProps = new BaseBashBuildScriptProperties()
            {
                PreBuildCommand = script1,
                PostBuildCommand = script2
            };

            // Act
            var script = TemplateHelper.Render(TemplateHelper.TemplateResource.BaseBashScript, scriptProps);

            // Assert
            Assert.Contains("Executing pre-build command", script);
            Assert.Contains(script1, script);
            Assert.Contains("Executing post-build command", script);
            Assert.Contains(script2, script);
        }
    }
}