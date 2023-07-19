// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using Newtonsoft.Json;

namespace Microsoft.Oryx.Automation.DotNet.Models
{
    public class ReleaseNote
    {
        [JsonProperty(PropertyName = "channel-version")]
        public string ChannelVersion { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "latest-release")]
        public string LatestRelease { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "latest-release-date")]
        public string LatestReleaseDate { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "security")]
        public string Security { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "latest-runtime")]
        public string LatestRuntime { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "latest-sdk")]
        public string LatestSdk { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "product")]
        public string Product { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "support-phase")]
        public string SupportPhase { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "eol-date")]
        public string EolDate { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "releases.json")]
        public string ReleasesJsonUrl { get; set; } = string.Empty;
    }
}
