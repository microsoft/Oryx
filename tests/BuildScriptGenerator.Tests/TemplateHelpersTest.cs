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
                "PythonBashBuildSnippet.sh.tpl"
            };

            foreach (string name in requiredTemplates)
            {
                Assert.True(existingResources.Count(s => s.EndsWith(name)) == 1, $"Could not find resource \"{name}\"");
            }
        }

        [Fact]
        public void ExtraFunctions()
        {
            string template = "{{ if IsNotBlank SomeVar }}not blank{{ else }}blank{{ end }}";
            Assert.Equal("blank",     TemplateHelpers.RenderString(template,         new { SomeVar = "" }));
            Assert.Equal("not blank", TemplateHelpers.RenderString(template,         new { SomeVar = "bla" }));

            string templateWithPipe = "{{ if SomeVar | IsNotBlank }}not blank{{ else }}blank{{ end }}";
            Assert.Equal("blank",     TemplateHelpers.RenderString(templateWithPipe, new { SomeVar = "" }));
            Assert.Equal("not blank", TemplateHelpers.RenderString(templateWithPipe, new { SomeVar = "bla" }));
        }

        [Fact]
        public void Render_Throws_WhenNonExistentVariableIsUsed()
        {
            Assert.Throws<ScriptRuntimeException>(() => TemplateHelpers.RenderString("Hello {{ World }}!", new { Foo = "Bar" }));
        }

        [Fact]
        public void Render_ProducesUnixLineEndings()
        {
            Assert.Equal("Hello\nWorld!", TemplateHelpers.RenderString("Hello\r\nWorld!", null));
        }
    }
}
