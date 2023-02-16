// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using Newtonsoft.Json;

namespace Microsoft.Oryx.Automation.DotNet.Models
{
    public class Release
    {
        [JsonProperty(PropertyName = "release-date")]
        public string ReleaseDate { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "sdk")]
        public SdkObj Sdk { get; set; } = new SdkObj();

        [JsonProperty(PropertyName = "runtime")]
        public RuntimeDotNet Runtime { get; set; } = new RuntimeDotNet();

        [JsonProperty(PropertyName = "aspnetcore-runtime")]
        public AspNetCoreRuntime AspNetCoreRuntime { get; set; } = new AspNetCoreRuntime();
    }
}
