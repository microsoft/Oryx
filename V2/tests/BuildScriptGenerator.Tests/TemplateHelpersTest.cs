// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Linq;
using Scriban.Syntax;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class TemplateHelpersTest
    {
        [Fact]
        public void RequiredTemplatesExist()
        {
            var assembly = typeof(IBuildScriptGenerator).Assembly;

            string[] existingResources = assembly.GetManifestResourceNames();
            string[] requiredTemplates = {
                "BaseBashBuildScript.sh.tpl",
                "DotNetCoreBashBuildSnippet.sh.tpl",
                "NodeBashBuildSnippet.sh.tpl",
                "PythonBashBuildSnippet.sh.tpl",
                "HugoBashBuildSnippet.sh.tpl",
            };

            foreach (string name in requiredTemplates)
            {
                Assert.True(
                    existingResources.Count(s => s.EndsWith(name)) == 1, $"Could not find resource \"{name}\"");
            }
        }

        [Fact]
        public void ExtraFunctions()
        {
            string template = "{{ if IsNotBlank SomeVar }}not blank{{ else }}blank{{ end }}";
            Assert.Equal("blank", TemplateHelper.RenderString(template, new { SomeVar = "" }));
            Assert.Equal("not blank", TemplateHelper.RenderString(template, new { SomeVar = "bla" }));

            string templateWithPipe = "{{ if SomeVar | IsNotBlank }}not blank{{ else }}blank{{ end }}";
            Assert.Equal("blank", TemplateHelper.RenderString(templateWithPipe, new { SomeVar = "" }));
            Assert.Equal("not blank", TemplateHelper.RenderString(templateWithPipe, new { SomeVar = "bla" }));
        }

        [Fact]
        public void Render_Throws_WhenNonExistentVariableIsUsed()
        {
            Assert.Throws<ScriptRuntimeException>(
                () => TemplateHelper.RenderString("Hello {{ World }}!", new { Foo = "Bar" }));
        }

        [Fact]
        public void Render_ProducesUnixLineEndings()
        {
            Assert.Equal("Hello\nWorld!", TemplateHelper.RenderString("Hello\r\nWorld!", null));
        }
    }
}
