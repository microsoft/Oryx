// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Oryx.Automation.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<string> GetDataAsync(this HttpClient httpClient, string url)
        {
            try
            {
                Console.WriteLine("Making GET request to: " + url);
                HttpResponseMessage response = await httpClient.GetAsync(url);
                Console.WriteLine($"Response received.: {response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetDataAsync method: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
                return null;
            }
        }

        public static async Task<HashSet<string>> GetOryxSdkVersionsAsync(this HttpClient httpClient, string url)
        {
            HashSet<string> versions = new HashSet<string>();
            var response = await httpClient.GetDataAsync(url);

            XDocument xmlDoc = XDocument.Parse(response);
            var versionElements = xmlDoc.Descendants("Version");

            foreach (var versionElement in versionElements)
            {
                string version = versionElement.Value;
                versions.Add(version);
            }

            return versions;
        }
    }
}