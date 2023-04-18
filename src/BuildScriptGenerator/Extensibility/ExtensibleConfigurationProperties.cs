// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Extensibility
{
    /// <summary>
    /// Class used to store properties that will be used when generating the script for the extensibility model.
    /// </summary>
    public class ExtensibleConfigurationProperties
    {
        /// <summary>
        /// Gets or sets the Debian flavor targeted for the build.
        /// </summary>
        public string DebianFlavor { get; set; }

        /// <summary>
        /// Gets or sets the directory that packages will be dynamically installed to.
        /// </summary>
        public string DynamicInstallRootDir { get; set; }

        /// <summary>
        /// Gets or sets the access token used to access the Oryx storage account.
        /// </summary>
        public string OryxSdkStorageAccountAccessToken { get; set; }

        /// <summary>
        /// Gets or sets the base URL of the Oryx storage account to dynamically pull binaries from.
        /// </summary>
        public string OryxSdkStorageBaseUrl { get; set; }
    }
}
