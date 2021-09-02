// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildServer
{
    public class BuildServerRequests
    {
        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }

        [JsonProperty(PropertyName = "destination")]
        public string Destination { get; set; }

        [JsonProperty(PropertyName = "platform")]
        public string Platform { get; set; }

        [JsonProperty(PropertyName = "platform-version")]
        public string PlatformVersion { get; set; }

        [JsonProperty(PropertyName = "build-temp-path")]
        public string BuildTempPath { get; set; }

        [JsonProperty(PropertyName = "compress-node-modules")]
        public string CompressNodeModules { get; set; }

        [JsonProperty(PropertyName = "compress-python-modules")]
        public string CompressPythonModules { get; set; }

        [JsonProperty(PropertyName = "python-virtual-env")]
        public string PythonVirtualEnv { get; set; }

        [JsonProperty(PropertyName = "log-file")]
        public string LogFile { get; set; }
    }
}
