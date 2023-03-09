// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Oryx.Automation.DotNet.Models
{
    public class ReleaseNotes
    {
        [JsonProperty(PropertyName = "releases-index")]
        public List<ReleaseNote> ReleaseIndexes { get; set; } = new List<ReleaseNote>();
    }
}
