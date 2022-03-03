// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

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

        private ConstantCollection _collection;
        private string _className;
        private string _directory;
        private string _namespace;
        private string _scope;

        public void Initialize(ConstantCollection constantCollection, Dictionary<string, string> typeInfo)
        {
            _collection = constantCollection;
            _className = _collection.Name.Camelize();
            _directory = typeInfo["directory"];
            _namespace = typeInfo["namespace"];
            typeInfo.TryGetValue("scope", out _scope);
        }

        public string GetPath()
        {
            return Path.Combine(_directory, _className + ".cs");
        }

        public string GetContent()
        {
            var scope = "public";
            if (!string.IsNullOrEmpty(_scope))
            {
                scope = _scope;
            }

            var model = new ConstantCollectionTemplateModel
            {
                AutogenDisclaimer = Program.BuildAutogenDisclaimer(_collection.SourcePath),
                Namespace = _namespace,
                Name = _className,
                Scope = scope,
                Constants = _collection.Constants.ToDictionary(pair => pair.Key.Camelize(), pair => pair.Value),
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
