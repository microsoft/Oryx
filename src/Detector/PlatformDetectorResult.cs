// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector
{
    /// <summary>
    /// Represents the result of a <see cref="IPlatformDetector.Detect(DetectorContext)"/> operation.
    /// This contains basic information about the detected application. However, each individual detector could include
    /// additional information about the application. For example <see cref="DotNetCore.DotNetCoreDetector"/> returns
    /// an instance of <see cref="DotNetCore.DotNetCorePlatformDetectorResult"/>.
    /// </summary>
    public class PlatformDetectorResult
    {
        /// <summary>
        /// Gets or sets the name of the platform that was detected. For example: nodejs, dotnet, php and python.
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Gets or sets the version of the platform that was detected.
        /// </summary>
        public string PlatformVersion { get; set; }

        /// <summary>
        /// Gets or sets the directory of the platform that was detected.
        /// </summary>
        public string AppDirectory { get; set; }
    }
}
