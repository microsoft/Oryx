// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Oryx.Automation.Client
{
    public class HttpClientImpl : IHttpClient
    {
        private readonly HttpClient httpClient;

        public HttpClientImpl()
        {
            this.httpClient = new HttpClient();
        }

        public async Task<string> GetDataAsync(string url)
        {
            try
            {
                Console.WriteLine("Making request to: " + url);
                HttpResponseMessage response = await this.httpClient.GetAsync(url);
                Console.WriteLine($"Response received.: {response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Error in GetDataAsync method, status code: " + response.StatusCode);
                    return null;
                }

                Console.WriteLine("Reading response content.");
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response content read.");
                return responseContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetDataAsync method: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
                return null;
            }
        }

        public async Task<HashSet<string>> GetOryxSdkVersionsAsync(string url)
        {
            HashSet<string> versions = new HashSet<string>();
            HttpClientImpl httpClientImpl = new HttpClientImpl();
            var response = await httpClientImpl.GetDataAsync(url);

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
