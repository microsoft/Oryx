// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Oryx.Automation.DotNet.Models
{
    public class ReleasesJson
    {
        [JsonProperty(PropertyName = "releases")]
        public List<Release> Releases { get; set; } = new List<Release>();
    }
}
