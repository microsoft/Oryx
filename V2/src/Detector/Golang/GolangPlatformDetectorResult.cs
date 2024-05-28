// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector.Golang
{
    public class GolangPlatformDetectorResult : PlatformDetectorResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether a 'go.mod' file exists in the repo.
        /// </summary>
        public bool GoModExists { get; set; }
    }
}
