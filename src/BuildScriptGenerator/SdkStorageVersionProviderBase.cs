// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Primitives;
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
            foreach (var runtimeVersionElement in xdoc.XPathSelectElements(
                $"//Blobs/Blob/Metadata/{versionMetadataElementName}"))
            {
                supportedVersions.Add(runtimeVersionElement.Value);
            }

            var defaultVersionContent = httpClient
                .GetStringAsync($"{sdkStorageBaseUrl}/{platformName}/defaultVersion.txt")
                .Result;

            // Ignore any comments in the file
            string defaultVersion = null;
            var strReader = new StringReader(defaultVersionContent);
            while (true)
            {
                var line = strReader.ReadLine();
                if (line != null && (!line.StartsWith("#") || !line.StartsWith("//")))
                {
                    defaultVersion = line.Trim();
                    break;
                }
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
