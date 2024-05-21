// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector
{
    /// <summary>
    /// This contains basic information about the detected frameworks that an application is using.
    /// </summary>
    public class FrameworkInfo
    {
        /// <summary>
        /// Gets or sets the framework name that was detected. For example: react, angular.
        /// </summary>
        public string Framework { get; set; }

        /// <summary>
        /// Gets or sets the framework version that was detected.
        /// </summary>
        public string FrameworkVersion { get; set; }

        public override string ToString()
        {
            return this.Framework + ": " + this.FrameworkVersion;
        }
    }
}
