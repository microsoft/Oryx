// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Text.Json.Serialization;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class OciDescriptor
    {
        [JsonPropertyName("mediaType")]
        public string MediaType { get; set; }

        [JsonPropertyName("digest")]
        public string Digest { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }
    }
}
