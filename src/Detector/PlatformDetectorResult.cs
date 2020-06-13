// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector
{
    /// <summary>
    /// Represents the result of a <see cref="IPlatformDetector.Detect(RepositoryContext)"/> operation.
    /// </summary>
    public class PlatformDetectorResult
    {
        public string Platform { get; set; }

        public string PlatformVersion { get; set; }
    }
}
