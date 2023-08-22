// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Oryx.Automation.Commons
{
    public static class SdkStorageHelper
    {
        /// <summary>
        /// Gets the SDK storage URL by combining the base URL with a platform-specific suffix.
        /// If the base URL is not provided, the default base URL will be used.
        /// For the staging account, a SAS token is required and will be appended to the URL.
        /// </summary>
        /// <param name="oryxSdkStorageBaseUrl">The base URL of the SDK storage.</param>
        /// <param name="platformSuffixUrl">The platform-specific suffix URL.</param>
        /// <returns>The SDK storage URL.</returns>
        public static string GetSdkStorageUrl(string oryxSdkStorageBaseUrl, string platformSuffixUrl)
        {
            if (string.IsNullOrEmpty(oryxSdkStorageBaseUrl))
            {
                oryxSdkStorageBaseUrl = Constants.OryxSdkStorageBaseUrl;
            }

            string sdkVersionsUrl = oryxSdkStorageBaseUrl + platformSuffixUrl;

            // A SAS token is required for the staging account.
            if (sdkVersionsUrl.StartsWith(Constants.OryxSdkStagingStorageBaseUrl))
            {
                string sasToken = Environment.GetEnvironmentVariable(Constants.OryxSdkStagingPrivateSasTokenEnvVar);
                if (string.IsNullOrEmpty(sasToken))
                {
                    throw new ArgumentException($"The environment variable {Constants.OryxSdkStagingPrivateSasTokenEnvVar} " +
                        $"must be provided in order to access {Constants.OryxSdkStagingStorageBaseUrl}");
                }

                sdkVersionsUrl += "&" + sasToken;
            }

            return sdkVersionsUrl;
        }

        /// <summary>
        /// Extracts blocked versions from a comma-separated string,
        /// stored in the GitHub Actions environment variable, and returns them as a list.
        /// </summary>
        /// <param name="blockedVersions">The comma-separated string of blocked versions.</param>
        /// <returns>The list of extracted blocked versions.</returns>
        public static List<string> ExtractBlockedVersions(string blockedVersions)
        {
            return string.IsNullOrEmpty(blockedVersions) ?
                new List<string>() :
                blockedVersions.Split(',').Select(v => v.Trim()).ToList();
        }
    }
}
