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

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    public class DotNetCoreSdkStorageVersionProvider : SdkStorageVersionProviderBase, IDotNetCoreVersionProvider
    {
        private Dictionary<string, string> versionMap;
        private string defaultRuntimeVersion;

        public DotNetCoreSdkStorageVersionProvider(
            IOptions<BuildScriptGeneratorOptions> commonOptions,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
            : base(commonOptions, httpClientFactory, loggerFactory)
        {
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

        public void GetVersionInfo()
        {
            // TODO: PR2 configure this to account for the different debian flavors once the Version metadata has
            // been generated for each package. We also need the Runtime_Version metadata for this method
            if (this.versionMap == null)
            {
                var httpClient = this.HttpClientFactory.CreateClient("general");
                var sdkStorageBaseUrl = this.GetPlatformBinariesStorageBaseUrl();
                var blobList = httpClient
                    .GetStringAsync($"{sdkStorageBaseUrl}/dotnet?restype=container&comp=list&include=metadata")
                    .Result;

                var xdoc = XDocument.Parse(blobList);

                // keys represent runtime version, values represent sdk version
                var supportedVersions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var metadataElement in xdoc.XPathSelectElements($"//Blobs/Blob/Metadata"))
                {
                    var childElements = metadataElement.Elements();
                    var runtimeVersionElement = childElements.Where(e => string.Equals(
                            DotNetCoreConstants.StorageSdkMetadataRuntimeVersionElementName,
                            e.Name.LocalName,
                            StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();

                    if (runtimeVersionElement != null)
                    {
                        var sdkVersionElement = childElements.Where(e => string.Equals(
                                DotNetCoreConstants.StorageSdkMetadataSdkVersionElementName,
                                e.Name.LocalName,
                                StringComparison.OrdinalIgnoreCase))
                            .FirstOrDefault();

                        supportedVersions[runtimeVersionElement.Value] = sdkVersionElement.Value;
                    }
                }

                this.versionMap = supportedVersions;
                this.defaultRuntimeVersion = this.GetDefaultVersion(DotNetCoreConstants.PlatformName, sdkStorageBaseUrl);
            }
        }
    }
}