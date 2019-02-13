// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;

namespace Microsoft.Oryx.SharedCodeGenerator.Outputs
{
    internal class ConstantCollectionTemplateModel
    {
        public string AutogenDisclaimer { get; set; }

        public string Namespace { get; set; }

        public string Name { get; set; }

        public Dictionary<string, string> Constants { get; set; }
    }
}
