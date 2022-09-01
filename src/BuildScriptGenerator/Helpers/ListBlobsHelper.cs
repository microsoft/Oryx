// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Linq;
using System.Xml.Linq;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class ListBlobsHelper
    {
        public static XDocument GetAllBlobs(string sdkStorageBaseUrl, string platform, System.Net.Http.HttpClient httpClient)
        {
            var url = string.Format(SdkStorageConstants.ContainerMetadataUrlFormat, sdkStorageBaseUrl, platform, string.Empty);
            var blobList = httpClient
                .GetStringAsync(url)
                .Result;
            var xdoc = XDocument.Parse(blobList);
            var marker = xdoc.Root.Element("NextMarker").Value;

            // if <NextMarker> element's value is not empty, we iterate through every page by appending marker value to the url
            // and consolidate blobs from all the pages.
            do
            {
                url = string.Format(SdkStorageConstants.ContainerMetadataUrlFormat, sdkStorageBaseUrl, DotNetCoreConstants.PlatformName, marker);
                var blobListFromNextMarker = httpClient.GetStringAsync(url).Result;
                var xdocFromNextMarker = XDocument.Parse(blobListFromNextMarker);
                marker = xdocFromNextMarker.Root.Element("NextMarker").Value;
                xdoc.Descendants("Blobs").LastOrDefault().AddAfterSelf(xdocFromNextMarker.Descendants("Blobs"));
            }
            while (!string.IsNullOrEmpty(marker));
            return xdoc;
        }
    }
}