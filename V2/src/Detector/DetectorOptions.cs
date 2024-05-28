// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector
{
    /// <summary>
    /// Settings used by the detectors when detecting applications.
    /// </summary>
    public class DetectorOptions
    {
        /// <summary>
        /// Gets or sets the relative path to the project file to be built.
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// Gets or sets the type of application that the source directory has,
        /// for example: 'functions' or 'static-sites' etc.
        /// </summary>
        public string AppType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the detectors should consider looking into sub-directories for files.
        /// If <c>true</c>, only the root of the source directory is probed for files. Default is <c>false</c>.
        /// </summary>
        public bool DisableRecursiveLookUp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the detectors should detect and outputs the frameworks that the application
        /// is using. If <c>true</c>, then disable detection for frameworks. Default is <c>false</c>.
        /// </summary>
        public bool DisableFrameworkDetection { get; set; }

        /// <summary>
        /// Gets or sets the path where a requirements.txt locates.
        /// </summary>
        public string CustomRequirementsTxtPath { get; set; }
    }
}
