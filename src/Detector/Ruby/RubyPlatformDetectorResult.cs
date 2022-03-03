// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.Detector.Ruby
{
    /// <summary>
    /// Represents the model which contains Ruby specific detected metadata.
    /// </summary>
    public class RubyPlatformDetectorResult : PlatformDetectorResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether a 'Gemfile' file exists in the repo.
        /// </summary>
        public bool GemfileExists { get; set; }

        /// <summary>
        /// Gets or sets the value if a bundler version specified in 'Gemfile.lock' in the repo.
        /// </summary>
        public string BundlerVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a 'config.yml' file exists in the repo.
        /// </summary>
        public bool ConfigYmlFileExists { get; set; }
    }
}
