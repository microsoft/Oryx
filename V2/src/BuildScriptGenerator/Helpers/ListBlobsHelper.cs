// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class ListBlobsHelper
    {
        public static XDocument GetAllBlobs(string sdkStorageBaseUrl, string platform, HttpClient httpClient, string oryxSdkStorageAccountAccessToken)
        {
            var oryxSdkStorageAccountAccessArgs = oryxSdkStorageAccountAccessToken?.TrimStart(new char[] { '?' }) ?? string.Empty;

            var url = string.Format(SdkStorageConstants.ContainerMetadataUrlFormat, sdkStorageBaseUrl, platform, string.Empty, oryxSdkStorageAccountAccessArgs);
            string blobList;
            try
            {
                blobList = httpClient
                    .GetStringAsync(url)
                    .Result;
            }
            catch (AggregateException ae)
            {
                throw new AggregateException(
                    $"Http request to retrieve the SDKs available to download from '{sdkStorageBaseUrl}' " +
                    $"failed. {Constants.NetworkConfigurationHelpText}{Environment.NewLine}{ae}");
            }

            if (string.IsNullOrEmpty(blobList))
            {
                throw new InvalidOperationException(
                    $"Http request to retrieve the SDKs available to download from'{sdkStorageBaseUrl}' cannot return an empty result.");
            }

            var xdoc = XDocument.Parse(blobList);
            var marker = xdoc.Root.Element("NextMarker").Value;

            // if <NextMarker> element's value is not empty, we iterate through every page by appending marker value to the url
            // and consolidate blobs from all the pages.
            do
            {
                url = string.Format(SdkStorageConstants.ContainerMetadataUrlFormat, sdkStorageBaseUrl, platform, marker, oryxSdkStorageAccountAccessArgs);
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