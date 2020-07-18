// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

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
        /// The name of the platform that was detected. For example: nodejs, dotnet, php and python.
        /// </summary>
        public readonly string Platform;

        /// <summary>
        /// The version of the platform that was detected. For example: python 3.7.3, php 7.3.15.
        /// </summary>
        public readonly string PlatformVersion;

        /// <summary>
        /// Constructor of PlatformDetectorResult.
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="platformVersion"></param>
        public PlatformDetectorResult(string platform, string platformVersion) {
            this.Platform = platform;
            this.PlatformVersion = platformVersion;
        }

        /// <summary>
        /// Gets pairs of property names and values from the PlatformDetectorResult.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetPlatformDetectorResultPropertyInfos()
        {
            var keyValuePairs = new Dictionary<string, string>
            {
                { "Platform", this.Platform },
                { "PlatformVersion", this.PlatformVersion }
            };
            return keyValuePairs;
        }
    }
}
