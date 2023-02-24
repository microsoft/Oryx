// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using Newtonsoft.Json;

namespace Microsoft.Oryx.Automation.DotNet.Models
{
    public class Release
    {
        [JsonProperty(PropertyName = "release-version")]
        public string ReleaseVersion { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "sdk")]
        public DotNetSdk Sdk { get; set; } = new DotNetSdk();

        [JsonProperty(PropertyName = "runtime")]
        public RuntimeDotNet Runtime { get; set; } = new RuntimeDotNet();

        [JsonProperty(PropertyName = "aspnetcore-runtime")]
        public AspNetCoreRuntime AspNetCoreRuntime { get; set; } = new AspNetCoreRuntime();
    }
}
