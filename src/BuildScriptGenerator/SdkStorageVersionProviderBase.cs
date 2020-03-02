// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class SdkStorageVersionProviderBase
    {
        private readonly IEnvironment _environment;
        private readonly IHttpClientFactory _httpClientFactory;

        public SdkStorageVersionProviderBase(IEnvironment environment, IHttpClientFactory httpClientFactory)
        {
            _environment = environment;
            _httpClientFactory = httpClientFactory;
        }

        protected PlatformVersionInfo GetAvailableVersionsFromStorage(
            string platformName,
            string versionMetadataElementName)
        {
            var httpClient = _httpClientFactory.CreateClient("general");

            var sdkStorageBaseUrl = GetPlatformBinariesStorageBaseUrl();
            var blobList = httpClient
                .GetStringAsync($"{sdkStorageBaseUrl}/{platformName}?restype=container&comp=list&include=metadata")
                .Result;
            var xdoc = XDocument.Parse(blobList);
            var supportedVersions = new List<string>();

            foreach (var metadataElement in xdoc.XPathSelectElements($"//Blobs/Blob/Metadata"))
            {
                var childElements = metadataElement.Elements();
                var versionElement = childElements.Where(e => string.Equals(
                        versionMetadataElementName,
                        e.Name.LocalName,
                        StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
                if (versionElement != null)
                {
                    supportedVersions.Add(versionElement.Value);
                }
            }

            // get default version
            var defaultVersionContent = httpClient
                .GetStringAsync($"{sdkStorageBaseUrl}/{platformName}/defaultVersion.txt")
                .Result;

            string defaultVersion = null;
            using (var stringReader = new StringReader(defaultVersionContent))
            {
                string line;
                while ((line = stringReader.ReadLine()) != null)
                {
                    // Ignore any comments in the file
                    if (!line.StartsWith("#") || !line.StartsWith("//"))
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
