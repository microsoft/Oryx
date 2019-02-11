// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Scriban;

namespace Microsoft.Oryx.SharedCodeGenerator.Outputs
{
    [OutputType("go")]
    internal class GoOutput : IOutputFile
    {
        private static readonly Template OutputTemplate;

        static GoOutput()
        {
            var projectOutputDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            using (var templateReader = new StreamReader(Path.Combine(projectOutputDir, "Outputs", "GoConstants.go.tpl")))
            {
                OutputTemplate = Template.Parse(templateReader.ReadToEnd());
            }
        }

        private ConstantCollection _collection;
        private string _directory;

        public void Initialize(ConstantCollection constantCollection, Dictionary<string, string> typeInfo)
        {
            _collection = constantCollection;
            _directory = typeInfo["directory"];
        }

        public string GetPath()
        {
            return Path.Combine(_directory, _collection.Name.Replace("-", "_") + ".go");
        }

        public string GetContent()
        {
            var model = new ConstantCollectionTemplateModel
            {
                AutogenDisclaimer = Program.BuildAutogenDisclaimer(_collection.SourcePath),
                Namespace = Path.GetFileName(_directory), // "/path/to/project/package" => "package"
                Constants = _collection.Constants.ToDictionary(pair => pair.Key.Camelize(), pair => pair.Value)
            };
            return OutputTemplate.Render(model, member => member.Name);
        }
    }
}
