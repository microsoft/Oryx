// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    public class DotNetCoreSdkStorageVersionProvider : SdkStorageVersionProviderBase, IDotNetCoreVersionProvider
    {
        private readonly BuildScriptGeneratorOptions commonOptions;
        private Dictionary<string, string> versionMap;
        private string defaultRuntimeVersion;

        public DotNetCoreSdkStorageVersionProvider(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
            : base(commonOptions, httpClientFactory, loggerFactory)
        {
            this.commonOptions = commonOptions.Value;
        }

        public Dictionary<string, string> SupportedVersionsMap { get; }

        public string GetDefaultRuntimeVersion()
        {
            this.GetVersionInfo();
            return this.defaultRuntimeVersion;
        }

        public Dictionary<string, string> GetSupportedVersions()
        {
            this.GetVersionInfo();
            return this.versionMap;
        }

        /// <summary>
        /// Pulls all files in the dotnet storage container and determines the supported and default versions.
        /// -----------
        /// This works slightly differently than <see cref="SdkStorageVersionProviderBase.GetAvailableVersionsFromStorage"/>,
        /// as the dotnet supported versions are a mapping of runtime version -> sdk version. This means that we need to find
        /// both runtime version and sdk version metadata associated with each file.
        /// </summary>
        public void GetVersionInfo()
        {
            if (this.versionMap == null)
            {
                var httpClient = this.HttpClientFactory.CreateClient("general");
                var sdkStorageBaseUrl = this.GetPlatformBinariesStorageBaseUrl();
                var oryxSdkStorageAccountAccessToken = this.commonOptions.OryxSdkStorageAccountAccessToken;
                var xdoc = ListBlobsHelper.GetAllBlobs(sdkStorageBaseUrl, DotNetCoreConstants.PlatformName, httpClient, oryxSdkStorageAccountAccessToken);

                // keys represent runtime version, values represent sdk version
                var supportedVersions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                var sdkVersionMetadataName = SdkStorageConstants.SdkVersionMetadataName;
                var runtimeVersionMetadataName = SdkStorageConstants.DotnetRuntimeVersionMetadataName;

                if (this.commonOptions.DebianFlavor == OsTypes.DebianStretch)
                {
                    sdkVersionMetadataName = SdkStorageConstants.LegacySdkVersionMetadataName;
                    runtimeVersionMetadataName = SdkStorageConstants.LegacyDotnetRuntimeVersionMetadataName;
                }

                foreach (var metadataElement in xdoc.XPathSelectElements($"//Blobs/Blob/Metadata"))
                {
                    var childElements = metadataElement.Elements();

                    var runtimeVersionElement = childElements.Where(e => string.Equals(
                            runtimeVersionMetadataName,
                            e.Name.LocalName,
                            StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();

                    // do not add a supported version if the correct runtime metadata was not found
                    if (runtimeVersionElement != null)
                    {
                        var sdkVersionElement = childElements.Where(e => string.Equals(
                                sdkVersionMetadataName,
                                e.Name.LocalName,
                                StringComparison.OrdinalIgnoreCase))
                            .FirstOrDefault();

                        var osTypeElement = childElements.Where(e => string.Equals(
                                SdkStorageConstants.OsTypeMetadataName,
                                e.Name.LocalName,
                                StringComparison.OrdinalIgnoreCase))
                            .FirstOrDefault();

                        // add supported version for stretch if runtime version and sdk version metadata is found
                        // add supported version for other os types if runtime version, sdk version, and matching os type metadata is found
                        if (sdkVersionElement != null
                            && (this.commonOptions.DebianFlavor == OsTypes.DebianStretch || this.commonOptions.DebianFlavor == osTypeElement.Value))
                        {
                            supportedVersions[runtimeVersionElement.Value] = sdkVersionElement.Value;
                        }
                    }
                }

                this.versionMap = supportedVersions;
                this.defaultRuntimeVersion = this.GetDefaultVersion(DotNetCoreConstants.PlatformName, sdkStorageBaseUrl);
            }
        }
    }
}