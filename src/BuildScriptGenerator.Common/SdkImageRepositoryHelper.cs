// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Common
{
    public static class SdkImageRepositoryHelper
    {
        /// <summary>
        /// Maps a platform name to its OCI SDK image repository path.
        /// e.g. "nodejs" → "oryx/nodejs-sdk", "php" → "oryx/php-sdk".
        /// Final image ref: mcr.microsoft.com/oryx/nodejs-sdk:bookworm-20.20.2
        /// </summary>
        public static string GetSdkImageRepository(string platformName, string prefix = null)
        {
            prefix = string.IsNullOrEmpty(prefix) ? SdkStorageConstants.DefaultAcrSdkRepositoryPrefix : prefix;
            return $"{prefix}/{platformName}-sdk";
        }
    }
}