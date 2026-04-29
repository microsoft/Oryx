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
        private static Template outputTemplate = CreateOutputTemplate();

        private ConstantCollection collection;
        private string directory;
        private string package;

        public void Initialize(ConstantCollection constantCollection, Dictionary<string, string> typeInfo)
        {
            this.collection = constantCollection;
            this.directory = typeInfo["directory"];
            this.package = typeInfo.GetValueOrDefault("package") ?? Path.GetFileName(this.directory);
        }

        public string GetPath()
        {
            return Path.Combine(this.directory, this.collection.Name.Replace(ConstantCollection.NameSeparator, "_") + ".go");
        }

        public string GetContent()
        {
            var model = new ConstantCollectionTemplateModel
            {
                Header = $"// {Program.BuildAutogenDisclaimer(this.collection.SourcePath)}",
                Namespace = this.package,
                StringConstants = this.collection.StringConstants?.ToDictionary(pair => pair.Key.Camelize(), pair => pair.Value),
            };
            return outputTemplate.Render(model, member => member.Name);
        }

        private static Template CreateOutputTemplate()
        {
            var projectOutputDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            using (var templateReader = new StreamReader(Path.Combine(projectOutputDir, "Outputs", "GoConstants.go.tpl")))
            {
                return Template.Parse(templateReader.ReadToEnd());
            }
        }
    }
}
