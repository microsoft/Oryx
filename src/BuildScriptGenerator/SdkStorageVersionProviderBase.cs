// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

        /// <summary>
        /// Pulls all files in the <paramref name="platformName"/> storage container and determines
        /// the supported and default versions.
        /// -----------
        /// We determine what versions are available differently based on the OS type where the oryx
        /// command was run.
        /// For <see cref="OsTypes.DebianStretch"/> we use the existance of <see cref="SdkStorageConstants.LegacySdkVersionMetadataName"/>
        /// metadata as the indicator for a supported version.
        /// For other <see cref="OsTypes"/> we use both <see cref="SdkStorageConstants.SdkVersionMetadataName"/> and
        /// matching <see cref="SdkStorageConstants.OsTypeMetadataName"/> metadata to indicate a matching version.
        /// </summary>
        /// <param name="platformName">Name of the platform to get the supported versions for</param>
        /// <returns><see cref="PlatformVersionInfo"/> containing supported and default versions</returns>
        protected PlatformVersionInfo GetAvailableVersionsFromStorage(string platformName)
        {
            this.logger.LogDebug("Getting list of available versions for platform {platformName}.", platformName);
            var httpClient = this.HttpClientFactory.CreateClient("general");
            var sdkStorageBaseUrl = this.GetPlatformBinariesStorageBaseUrl();
            var xdoc = ListBlobsHelper.GetAllBlobs(sdkStorageBaseUrl, platformName, httpClient);
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

                // if a matching version element is not found, we do not add as a supported version
                // if the os type is stretch and we find a blob with a 'Version' metadata, we know it is a supported version
                // otherwise, we check the blob for 'Sdk_version' metadata AND ensure 'Os_type' metadata matches current debianFlavor
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

            var defaultFile = string.IsNullOrEmpty(this.commonOptions.DebianFlavor)
                    || string.Equals(this.commonOptions.DebianFlavor, OsTypes.DebianStretch, StringComparison.OrdinalIgnoreCase)
                ? SdkStorageConstants.DefaultVersionFileName
                : $"{SdkStorageConstants.DefaultVersionFilePrefix}.{this.commonOptions.DebianFlavor}.{SdkStorageConstants.DefaultVersionFileType}";
            var defaultVersionUrl = $"{sdkStorageBaseUrl}/{platformName}/{defaultFile}";

            this.logger.LogDebug("Getting the default version from url {defaultVersionUrl}.", defaultVersionUrl);

            // get default version
            string defaultVersionContent;
            try
            {
                defaultVersionContent = httpClient
                    .GetStringAsync($"{defaultVersionUrl}")
                    .Result;
            }
            catch (AggregateException ae)
            {
                throw new AggregateException(
                    $"Http request to retrieve the default version from '{defaultVersionUrl}' failed. " +
                    $"{Constants.NetworkConfigurationHelpText}{Environment.NewLine}{ae}");
            }

            if (string.IsNullOrEmpty(defaultVersionContent))
            {
                throw new InvalidOperationException(
                    $"Http request to retrieve the default version from '{defaultVersionUrl}' cannot return an empty result.");
            }

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

            if (string.IsNullOrEmpty(platformBinariesStorageBaseUrl))
            {
                 throw new InvalidOperationException(
                    $"Environment variable '{SdkStorageConstants.SdkStorageBaseUrlKeyName}' is required.");
            }

            this.logger.LogDebug("Using the Sdk storage url {sdkStorageUrl}.", platformBinariesStorageBaseUrl);
            platformBinariesStorageBaseUrl = platformBinariesStorageBaseUrl.TrimEnd('/');
            return platformBinariesStorageBaseUrl;
        }
    }
}
