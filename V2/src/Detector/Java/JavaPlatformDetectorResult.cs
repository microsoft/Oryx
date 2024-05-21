// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector.Java
{
    /// <summary>
    /// Represents a <see cref="PlatformDetectorResult"/> returned by the <see cref="JavaDetector"/> and
    /// contains additional information related to the detected applications.
    /// </summary>
    public class JavaPlatformDetectorResult : PlatformDetectorResult
    {
        public bool UsesMaven { get; set; }

        public bool UsesMavenWrapperTool { get; set; }

        public string MavenVersion { get; set; }
    }
}
