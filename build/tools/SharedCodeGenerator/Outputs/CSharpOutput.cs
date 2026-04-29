// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Scriban;

namespace Microsoft.Oryx.SharedCodeGenerator.Outputs.CSharp
{
    [OutputType("csharp")]
    internal class CSharpOutput : IOutputFile
    {
        private static Template outputTemplate = CreateOutputTemplate();

        private ConstantCollection collection;
        private string className;
        private string directory;
        private string namespaceProperty;
        private string scope;

        public void Initialize(ConstantCollection constantCollection, Dictionary<string, string> typeInfo)
        {
            this.collection = constantCollection;
            this.className = this.collection.Name.Camelize();
            this.directory = typeInfo["directory"];
            this.namespaceProperty = typeInfo["namespace"];
            typeInfo.TryGetValue("scope", out this.scope);
        }

        public string GetPath()
        {
            return Path.Combine(this.directory, this.className + ".cs");
        }

        public string GetContent()
        {
            var scope = "public";
            if (!string.IsNullOrEmpty(this.scope))
            {
                scope = this.scope;
            }

            var header = $"// {Program.BuildAutogenDisclaimer(this.collection.SourcePath)}";
            if (this.collection.ListConstants?.Any() == true ||
                this.collection.DictionaryConstants?.Any() == true)
            {
                header += $"{Environment.NewLine}{Environment.NewLine}using System.Collections.Generic;";
            }

            var model = new ConstantCollectionTemplateModel
            {
                Header = header,
                Namespace = this.namespaceProperty,
                Name = this.className,
                Scope = scope,
                StringConstants = this.collection.StringConstants?.ToDictionary(pair => pair.Key.Camelize(), pair => pair.Value),
                ListConstants = this.collection.ListConstants?.ToDictionary(
                    pair => pair.Key.Camelize(),
                    pair => pair.Value.Any(s => !string.IsNullOrEmpty(s?.ToString()))
                        ? $"{{ \"{string.Join("\", \"", pair.Value)}\" }}"
                        : "{ }"),
                DictionaryConstants = this.collection.DictionaryConstants?.ToDictionary(
                    pair => pair.Key.Camelize(),
                    pair => pair.Value.Any(s => !string.IsNullOrEmpty(s.Key?.ToString()) && !string.IsNullOrEmpty(s.Value?.ToString()))
                        ? $"{{ {string.Join(", ", pair.Value.Select(p => $"{{ \"{p.Key}\", \"{p.Value}\" }}"))} }}"
                        : "{ }"),
            };

            return outputTemplate.Render(model, member => member.Name);
        }

        private static Template CreateOutputTemplate()
        {
            var projectOutputDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            using (var templateReader = new StreamReader(
                Path.Combine(projectOutputDir, "Outputs", "CSharpConstants.cs.tpl")))
            {
                return Template.Parse(templateReader.ReadToEnd());
            }
        }
    }
}
