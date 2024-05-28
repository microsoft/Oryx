// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Text.Json.Serialization;

namespace Microsoft.Oryx.Automation.DotNet.Models
{
    public class ReleasesIndex
    {
        [JsonPropertyName("latest-release")]
        public string LatestRelease { get; set; }

        [JsonPropertyName("latest-runtime")]
        public string LatestRuntime { get; set; }

        [JsonPropertyName("latest-sdk")]
        public string LatestSdk { get; set; }

        [JsonPropertyName("releases.json")]
        public string ReleasesJsonUrl { get; set; }
    }
}
