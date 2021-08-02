// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------


namespace Microsoft.Oryx.Detector.Go
{
    public class GoPlatformDetectorResult : PlatformDetectorResult
    {
        /// <summary>
        /// Gets or sets the value indicating if a 'go.mod' file exists in the repo.
        /// </summary>
        public bool goDotModExists { get; set; }
    }
}
