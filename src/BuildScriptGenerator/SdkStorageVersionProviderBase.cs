// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
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

        protected PlatformVersionInfo GetAvailableVersionsFromStorage(string platformName)
        {
            this.logger.LogDebug("Getting list of available versions for platform {platformName}.", platformName);
            var httpClient = this.HttpClientFactory.CreateClient("general");

            var sdkStorageBaseUrl = this.GetPlatformBinariesStorageBaseUrl();
            var url = string.Format(SdkStorageConstants.ContainerMetadataUrlFormat, sdkStorageBaseUrl, platformName);
            var blobList = httpClient.GetStringAsync(url).Result;
            var xdoc = XDocument.Parse(blobList);
            var supportedVersions = new List<string>();

            var isStretch = string.Equals(this.commonOptions.DebianFlavor, OsTypes.DebianStretch, StringComparison.OrdinalIgnoreCase);

            var sdkVersionMetadataName = isStretch
                ? SdkStorageConstants.LegacySdkVersionMetadataName
                : SdkStorageConstants.SdkVersionMetadataName;

            foreach (var metadataElement in xdoc.XPathSelectElements($"//Blobs/Blob/Metadata"))
            {
                var childElements = metadataElement.Elements();
                var versionElement = childElements
                    .Where(e => string.Equals(sdkVersionMetadataName, e.Name.LocalName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();

                var osTypeElement = childElements
                    .Where(e => string.Equals(SdkStorageConstants.OsTypeMetadataName, e.Name.LocalName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();

                // if the os type is stretch and we find a blob with the correct version metadata, we add as a supported version
                // otherwise, we check the blob for the correct version metadata and ensure that its os type/debian flavor matches
                if (versionElement != null &&
                    (isStretch || (osTypeElement != null && string.Equals(this.commonOptions.DebianFlavor, osTypeElement.Value, StringComparison.OrdinalIgnoreCase))))
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

            var defaultFile = string.Equals(this.commonOptions.DebianFlavor, OsTypes.DebianStretch, StringComparison.OrdinalIgnoreCase)
                ? SdkStorageConstants.DefaultVersionFileName
                : $"{SdkStorageConstants.DefaultVersionFilePrefix}.{this.commonOptions.DebianFlavor}.{SdkStorageConstants.DefaultVersionFileType}";
            var defaultVersionUrl = $"{sdkStorageBaseUrl}/{platformName}/{defaultFile}";

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
