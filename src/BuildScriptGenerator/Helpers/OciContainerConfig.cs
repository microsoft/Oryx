// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class OciContainerConfig
    {
        [JsonPropertyName("Labels")]
        public Dictionary<string, string> Labels { get; set; }
    }
}
