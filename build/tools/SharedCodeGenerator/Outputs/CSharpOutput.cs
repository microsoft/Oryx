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
        private static readonly Template OutputTemplate;

        private ConstantCollection _collection;
        private string _className;
        private string _directory;
        private string _namespace;

        static CSharpOutput()
        {
            var projectOutputDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            using (var templateReader = new StreamReader(Path.Combine(projectOutputDir, "Outputs", "CSharpConstants.cs.tpl")))
            {
                OutputTemplate = Template.Parse(templateReader.ReadToEnd());
            }
        }

        public void Initialize(ConstantCollection constantCollection, Dictionary<string, string> typeInfo)
        {
            _collection = constantCollection;
            _className = _collection.Name.Camelize();
            _directory = typeInfo["directory"];
            _namespace = typeInfo["namespace"];
        }

        public string GetPath()
        {
            return Path.Combine(_directory, _className + ".cs");
        }

        public string GetContent()
        {
            var model = new ConstantCollectionTemplateModel
            {
                AutogenDisclaimer = Program.BuildAutogenDisclaimer(_collection.SourcePath),
                Namespace = _namespace,
                Name = _className,
                Constants = _collection.Constants.ToDictionary(pair => pair.Key.Camelize(), pair => pair.Value),
            };
            return OutputTemplate.Render(model, member => member.Name);
        }
    }
}
