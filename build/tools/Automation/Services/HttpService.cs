// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.Oryx.Automation.Services
{
    public class HttpService : IHttpService, IDisposable
    {
        private HttpClient httpClient;

        public HttpService(IHttpClientFactory httpClientFactory)
        {
            this.httpClient = httpClientFactory.CreateClient();
        }

        /// <summary>
        /// Sends a GET request to the specified URL.
        /// instance and returns the response content as a string.
        /// Returns null if the response status code is not successful.
        /// </summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <returns>A Task<string> representing the asynchronous operation. The result of
        /// the task is the response content as a string, or null if the response status code is not
        /// successful.
        /// </returns>
        public async Task<string> GetDataAsync(string url)
        {
            try
            {
                HttpResponseMessage response = await this.httpClient.GetAsync(url);
                Console.WriteLine($"Response received: {response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetDataAsync method: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Sends a GET request to the specified URL using the given
        /// HttpClient instance and returns a HashSet<string> containing
        /// the ORYX SDK versions retrieved from the response content.
        /// The response content is expected to be an XML document
        /// containing one or more "Version" objects, each with a single
        /// string value representing a version number.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance to use for the request.</param>
        /// <param name="url">The URL to send the request to.</param>
        /// <returns>A Task HashSet<string> representing the asynchronous operation. The result of
        /// the task is a HashSet<string> containing the ORYX SDK versions retrieved from the
        /// response content.
        /// </returns>
        public async Task<HashSet<string>> GetOryxSdkVersionsAsync(string url)
        {
            HashSet<string> versions = new HashSet<string>();
            var response = await this.GetDataAsync(url);
            if (response == null)
            {
                return versions;
            }

            XDocument xmlDoc = XDocument.Parse(response);
            var versionElements = xmlDoc.Descendants("Version");

            foreach (var versionElement in versionElements)
            {
                string version = versionElement.Value;
                versions.Add(version);
            }

            return versions;
        }

        public void Dispose()
        {
            this.httpClient.Dispose();
        }
    }
}