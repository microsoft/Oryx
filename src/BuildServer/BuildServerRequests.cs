// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildServer
{
    public class BuildServerRequests
    {
        [JsonProperty(PropertyName = "command")]
        public string command { get; set; }
    }
}
