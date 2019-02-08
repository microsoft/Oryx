// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.SharedCodeGenerator
{
    internal class ConstantCollection
    {
        public string SourcePath { get; set; }

        public string Name { get; set; }

        public Dictionary<string, string> Constants { get; set; }

        public List<Dictionary<string, string>> Outputs { get; set; }
    }
}
