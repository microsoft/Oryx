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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class SdkStorageVersionProviderBase
    {
        private readonly ILogger logger;
        private readonly BuildScriptGeneratorOptions commonOptions;

        public SdkStorageVersionProviderBase(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
        {
            this.commonOptions = commonOptions.Value;
            this.HttpClientFactory = httpClientFactory;
            this.logger = loggerFactory.CreateLogger(this.GetType());
        }

        protected IHttpClientFactory HttpClientFactory { get; }

        protected PlatformVersionInfo GetAvailableVersionsFromStorage(
            string platformName,
            string versionMetadataElementName)
        {
            // TODO: configure this to account for the different debian flavors once the Version metadata has
            // been generated for each package
            this.logger.LogDebug("Getting list of available versions for platform {platformName}.", platformName);
            var httpClient = this.HttpClientFactory.CreateClient("general");

            var sdkStorageBaseUrl = this.GetPlatformBinariesStorageBaseUrl();
            var url = string.Format(SdkStorageConstants.ContainerMetadataUrlFormat, sdkStorageBaseUrl, platformName);
            var blobList = httpClient.GetStringAsync(url).Result;
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

            var defaultVersion = this.GetDefaultVersion(platformName, sdkStorageBaseUrl);
            return PlatformVersionInfo.CreateAvailableOnWebVersionInfo(supportedVersions, defaultVersion);
        }

        protected string GetDefaultVersion(string platformName, string sdkStorageBaseUrl)
        {
            var httpClient = this.HttpClientFactory.CreateClient("general");

            var defaultVersionUrl = this.commonOptions.DebianFlavor == OsTypes.DebianStretch
                ? $"{sdkStorageBaseUrl}/{platformName}/{SdkStorageConstants.DefaultVersionFileName}"
                : $"{sdkStorageBaseUrl}/{platformName}/{SdkStorageConstants.DefaultVersionFilePrefix}.{this.commonOptions.DebianFlavor}.{SdkStorageConstants.DefaultVersionFilePrefix}";

            this.logger.LogDebug("Getting the default version from url {defaultVersionUrl}.", defaultVersionUrl);

            // get default version
            var defaultVersionContent = httpClient
                .GetStringAsync(defaultVersionUrl)
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

            this.logger.LogDebug(
                "Got the default version for {platformName} as {defaultVersion}.",
                platformName,
                defaultVersion);

            if (string.IsNullOrEmpty(defaultVersion))
            {
                throw new InvalidOperationException("Default version cannot be empty.");
            }

            return defaultVersion;
        }

        protected string GetPlatformBinariesStorageBaseUrl()
        {
            var platformBinariesStorageBaseUrl = this.commonOptions.OryxSdkStorageBaseUrl;

            this.logger.LogDebug("Using the Sdk storage url {sdkStorageUrl}.", platformBinariesStorageBaseUrl);

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
