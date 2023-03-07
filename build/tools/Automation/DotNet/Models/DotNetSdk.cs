// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Oryx.Automation.DotNet.Models
{
    public class DotNetSdk
    {
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "version-display")]
        public string VersionDisplay { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "runtime-version")]
        public string RuntimeVersion { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "files")]
        public List<File> Files { get; set; } = new List<File>();
    }
}
