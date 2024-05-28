// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.Detector;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class PlatformInfo
    {
        /// <summary>
        /// Gets or sets the detected result.
        /// </summary>
        public PlatformDetectorResult DetectorResult { get; set; }

        /// <summary>
        /// Gets or sets the tools that need to be set in path by benv script.
        /// Key presents the tool name and the value represents the version.
        /// </summary>
        public IDictionary<string, string> RequiredToolsInPath { get; set; }
    }
}
