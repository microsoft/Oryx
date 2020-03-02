// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class SdkStorageVersionProviderBase
    {
        private readonly IEnvironment _environment;

        public SdkStorageVersionProviderBase(IEnvironment environment)
        {
            _environment = environment;
        }

        protected PlatformVersionInfo GetAvailableVersionsFromStorage(
            string platformName,
            string versionMetadataElementName)
        {
            var sdkStorageBaseUrl = GetPlatformBinariesStorageBaseUrl();
            var blobServiceClient = new BlobServiceClient(serviceUri: new Uri(sdkStorageBaseUrl));
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(platformName);

            // Example: nodejs-10.1.0.tar.gz is the format of a blob
            var blobList = blobContainerClient.GetBlobs(BlobTraits.Metadata, prefix: $"{platformName}-");
            var supportedVersions = new List<string>();
            foreach (var blob in blobList)
            {
                // Metadata dictionary here is case-insensitive
                if (blob.Metadata.TryGetValue(versionMetadataElementName, out var version))
                {
                    supportedVersions.Add(version);
                }
            }

            // get default version
            var blobClient = blobContainerClient.GetBlobClient(SdkStorageConstants.DefaultVersionFileName);
            var blobStream = blobClient.Download().Value.Content;
            string blobContent = null;
            using (var streamReader = new StreamReader(blobStream))
            {
                blobContent = streamReader.ReadToEnd();
            }

            string defaultVersion = null;
            using (var stringReader = new StringReader(blobContent))
            {
                string line;
                while ((line = stringReader.ReadLine()) != null)
                {
                    // Ignore any comments in the file
                    if (!line.StartsWith("#") && !line.StartsWith("//"))
                    {
                        defaultVersion = line.Trim();
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(defaultVersion))
            {
                throw new InvalidOperationException("Default version cannot be empty.");
            }

            return PlatformVersionInfo.CreateAvailableOnWebVersionInfo(supportedVersions, defaultVersion);
        }

        private string GetPlatformBinariesStorageBaseUrl()
        {
            var platformBinariesStorageBaseUrl = _environment.GetEnvironmentVariable(
                SdkStorageConstants.SdkStorageBaseUrlKeyName);
            if (string.IsNullOrEmpty(platformBinariesStorageBaseUrl))
            {
                throw new InvalidOperationException(
                    $"Environment variable '{SdkStorageConstants.SdkStorageBaseUrlKeyName}' is required.");
            }

            platformBinariesStorageBaseUrl = platformBinariesStorageBaseUrl.TrimEnd('/');
            return platformBinariesStorageBaseUrl;
        }
    }
}
