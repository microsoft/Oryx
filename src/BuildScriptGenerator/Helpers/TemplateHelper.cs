// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    using System.Collections.Generic;
    using System.IO;
    using JetBrains.Annotations;
    using Microsoft.ApplicationInsights;
    using Microsoft.Extensions.Logging;
    using Microsoft.Oryx.BuildScriptGenerator.Common.Extensions;
    using Scriban;
    using Scriban.Runtime;

    internal static class TemplateHelper
    {
        private static readonly MemberRenamerDelegate NoOpRenamer = member => member.Name;

        public static string Render(TemplateResource templateResource, [CanBeNull] object model, ILogger logger = null, TelemetryClient telemetryClient = null)
        {
            var assembly = typeof(IBuildScriptGenerator).Assembly;
            using (var stream = assembly.GetManifestResourceStream(templateResource.Name))
            {
                if (stream == null)
                {
                    logger?.LogError(
                        "Could not get resource {resourceName}. Available resources: {availableResourceNames}",
                        templateResource.Name,
                        string.Join("|", assembly.GetManifestResourceNames()));
                }

                using (TextReader tplReader = new StreamReader(stream))
                using (telemetryClient?.LogTimedEvent(
                    "RenderTemplate",
                    new Dictionary<string, string> { { "templateName", templateResource.Name } }))
                {
                    return RenderString(tplReader.ReadToEnd(), model);
                }
            }
        }

        public static string RenderString(string templateBody, object model)
        {
            var ctx = new TemplateContext
            {
                MemberRenamer = NoOpRenamer,
                StrictVariables = true,
            };

            if (model != null)
            {
                var modelObj = new ScriptObject();
                modelObj.Import(model, renamer: NoOpRenamer);
                ctx.PushGlobal(modelObj);
            }

            // Injects the function IsNullOrWhiteSpace so that it's available for use in templates.
            // Further reading:
            // https://github.com/lunet-io/scriban/blob/master/doc/runtime.md#the-stack-of-scriptobject
            ctx.BuiltinObject.Import(typeof(TemplateFunctions), renamer: NoOpRenamer);

            return Template.Parse(templateBody).Render(ctx).Replace("\r\n", "\n");
        }

        public static class TemplateFunctions
        {
            public static bool IsNotBlank([CanBeNull] string value)
            {
                return !string.IsNullOrWhiteSpace(value);
            }
        }

        public class TemplateResource
        {
            private TemplateResource(string name)
            {
                this.Name = name;
            }

            public static TemplateResource BaseBashScript
            {
                get => new TemplateResource("Microsoft.Oryx.BuildScriptGenerator.BaseBashBuildScript.sh.tpl");
            }

            public static TemplateResource Dockerfile
            {
                get => new TemplateResource("Microsoft.Oryx.BuildScriptGenerator.Dockerfile.oryx.tpl");
            }

            public static TemplateResource PhpBuildSnippet
            {
                get => new TemplateResource("Microsoft.Oryx.BuildScriptGenerator.Php.PhpBashBuildSnippet.sh.tpl");
            }

            public static TemplateResource GolangSnippet
            {
                get => new TemplateResource(
                    "Microsoft.Oryx.BuildScriptGenerator.Golang.GolangBashBuildSnippet.sh.tpl");
            }

            public static TemplateResource PythonSnippet
            {
                get => new TemplateResource(
                    "Microsoft.Oryx.BuildScriptGenerator.Python.PythonBashBuildSnippet.sh.tpl");
            }

            public static TemplateResource PythonJupyterNotebookSnippet
            {
                get => new TemplateResource(
                    "Microsoft.Oryx.BuildScriptGenerator.Python.JupyterNotebookBashBuildSnippet.sh.tpl");
            }

            public static TemplateResource NodeBuildSnippet
            {
                get => new TemplateResource("Microsoft.Oryx.BuildScriptGenerator.Node.NodeBashBuildSnippet.sh.tpl");
            }

            public static TemplateResource DotNetCoreSnippet
            {
                get => new TemplateResource(
                    "Microsoft.Oryx.BuildScriptGenerator.DotNetCore.DotNetCoreBashBuildSnippet.sh.tpl");
            }

            public static TemplateResource HugoSnippet
            {
                get => new TemplateResource(
                    "Microsoft.Oryx.BuildScriptGenerator.Hugo.HugoBashBuildSnippet.sh.tpl");
            }

            public static TemplateResource JavaBuildSnippet
            {
                get => new TemplateResource(
                    "Microsoft.Oryx.BuildScriptGenerator.Java.JavaBashBuildSnippet.sh.tpl");
            }

            public string Name { get; private set; }
        }
    }
}