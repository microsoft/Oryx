// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class OciManifest
    {
        [JsonPropertyName("schemaVersion")]
        public int SchemaVersion { get; set; }

        [JsonPropertyName("mediaType")]
        public string MediaType { get; set; }

        [JsonPropertyName("config")]
        public OciDescriptor Config { get; set; }

        [JsonPropertyName("layers")]
        public List<OciDescriptor> Layers { get; set; }
    }
}
