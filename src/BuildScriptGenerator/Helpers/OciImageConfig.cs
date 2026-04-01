// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Text.Json.Serialization;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class OciImageConfig
    {
        [JsonPropertyName("config")]
        public OciContainerConfig Config { get; set; }
    }
}
