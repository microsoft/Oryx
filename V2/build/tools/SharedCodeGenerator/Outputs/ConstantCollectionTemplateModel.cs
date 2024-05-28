// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.SharedCodeGenerator.Outputs
{
    internal class ConstantCollectionTemplateModel
    {
        public string Header { get; set; }

        public string Namespace { get; set; }

        public string Name { get; set; }

        public string Scope { get; set; }

        public Dictionary<string, string> StringConstants { get; set; }

        public Dictionary<string, string> ListConstants { get; set; }

        public Dictionary<string, string> DictionaryConstants { get; set; }
    }
}
