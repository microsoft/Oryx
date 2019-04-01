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
        private const string NewLine = "\n";
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
            return Path.Combine(_directory, _fileNamePrefix + _collection.Name + ".sh");
        }

        public string GetContent()
        {
            StringBuilder body = new StringBuilder();
            body.Append("# " + Program.BuildAutogenDisclaimer(_collection.SourcePath) + NewLine); // Can't use AppendLine becuase it appends \r\n
            body.Append(NewLine);
            foreach (var constant in _collection.Constants)
            {
                string name = constant.Key.Replace(ConstantCollection.NameSeparator[0], '_').ToUpper();
                body.Append($"{name}='{constant.Value}'{NewLine}");
            }

            return body.ToString();
        }
    }
}
