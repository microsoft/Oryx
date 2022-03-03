// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Oryx.SharedCodeGenerator.Outputs
{
    [OutputType("shell")]
    internal class ShellOutput : IOutputFile
    {
        private const char NewLine = '\n';
        private ConstantCollection _collection;
        private string _directory;
        private string _fileNamePrefix;

        public void Initialize(ConstantCollection constantCollection, Dictionary<string, string> typeInfo)
        {
            _collection = constantCollection;
            _directory = typeInfo["directory"];
            _fileNamePrefix = typeInfo["file-name-prefix"];
        }

        public string GetPath()
        {
            var name = _collection.Name.Camelize();
            name = char.ToLowerInvariant(name[0]) + name.Substring(1);
            return Path.Combine(_directory, _fileNamePrefix + name + ".sh");
        }

        public string GetContent()
        {
            StringBuilder body = new StringBuilder();
            body.Append("# " + Program.BuildAutogenDisclaimer(_collection.SourcePath) + NewLine); // Can't use AppendLine becuase it appends \r\n
            body.Append(NewLine);
            foreach (var constant in _collection.Constants)
            {
                string name = constant.Key.Replace(ConstantCollection.NameSeparator[0], '_').ToUpper();
                var value = constant.Value.WrapValueInQuotes();

                // Ex: PYTHON_VERSION='3.7.7'
                body.Append($"{name}={value}{NewLine}");
            }

            return body.ToString();
        }
    }
}
