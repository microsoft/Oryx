using System;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Php;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests.Php
{
    public class PhpBashBuildSnippetTest
    {

        [Fact]
        public void GeneratedSnippet_CopyNginxConfAndReloadNginx()
        {
            // Arrange
            var snippetProps = new PhpBashBuildSnippetProperties { NginxConfFile = "nginxconffile" }; ;

            // Act
            var text = TemplateHelper.Render(TemplateHelper.TemplateResource.PhpBuildSnippet, snippetProps);

            // Assert
            Assert.NotEmpty(text);
            Assert.NotNull(text);
            string extectedString = "cp " + snippetProps.NginxConfFile + " etc/nginx/nginx.conf" + System.Environment.NewLine + "service nginx reload";
            Assert.Contains(extectedString, text);
        }
    }
}