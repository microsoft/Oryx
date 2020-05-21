// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector
{
    /// <summary>
    /// Options to create a detector.
    /// </summary>
    internal class DetectorOptions
    {
        /// <summary>
        /// Gets or sets the source repo where the application is stored.
        /// </summary>
        public string SourceDir { get; set; }

        public bool EnableTelemetry { get; set; }

        public bool EnableCheckers { get; set; }

        public bool EnableDynamicInstall { get; set; }

        public string OryxSdkStorageBaseUrl { get; set; }

        public bool OutputJson { get; set; }

        /// <summary>
        /// Gets or sets the relative path to the project to be built.
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// Gets or sets the arguments to be passed into the detector.
        /// </summary>
        public string[] PassThruArguments { get; set; }
    }
}