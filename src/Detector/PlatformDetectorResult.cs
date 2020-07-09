// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector
{
    /// <summary>
    /// Represents the result of a <see cref="IPlatformDetector.Detect(DetectorContext)"/> operation.
    /// This contains basic information of the detected application. However, each individual detector could include
    /// additional information about the application. For example <see cref="DotNetCore.DotNetCoreDetector"/> returns
    /// an instance of <see cref="DotNetCore.DotNetCorePlatformDetectorResult"/>.
    /// </summary>
    public class PlatformDetectorResult
    {
        /// <summary>
        /// The name the platform that was detected. For example: nodejs, dotnet, php and python.
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// The version of the platform that was detected.
        /// </summary>
        public string PlatformVersion { get; set; }
    }
}
