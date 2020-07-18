// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.Detector.DotNetCore
{
    /// <summary>
    /// Represents a <see cref="PlatformDetectorResult"/> returned by the <see cref="DotNetCoreDetector"/> and
    /// contains additional information related to the detected applications.
    /// </summary>
    public class DotNetCorePlatformDetectorResult : PlatformDetectorResult
    {
        /// <summary>
        /// The relative path to the location of the project file.
        /// </summary>
        /// <example>
        /// "src/ShoppingWeb/ShoppingWeb.csproj"
        /// </example>
        public readonly string ProjectFile;

        /// <summary>
        /// Constructor of DotNetCorePlatformDetectorResult.
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="platformVersion"></param>
        /// <param name="projectFile"></param>
        public DotNetCorePlatformDetectorResult(string platform, string platformVersion, string projectFile) 
            : base(platform, platformVersion)
        {
            this.ProjectFile = projectFile;
        }

        /// <summary>
        /// Gets pairs of property names and values from the DotNetCorePlatformDetectorResult.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetPlatformDetectorResultPropertyInfos()
        {
            var keyValuePairs = new Dictionary<string, string>
            {
                { "Platform", this.Platform },
                { "PlatformVersion", this.PlatformVersion },
                { "ProjectFile", this.ProjectFile }
            };
            return keyValuePairs;
        }
    }
}
