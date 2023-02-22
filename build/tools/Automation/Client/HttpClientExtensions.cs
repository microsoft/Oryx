// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Oryx.Automation.Client
{
    public static class HttpClientExtensions
    {
        public static IServiceCollection AddHttpClientImpl(this IServiceCollection services)
        {
            services.AddSingleton<HttpClientImpl>();
            return services;
        }
    }

    public class HttpClientImpl : IDisposable
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
                Console.WriteLine("Making GET request to: " + url);
                HttpResponseMessage response = await this.httpClient.GetAsync(url);
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

        public async Task<HashSet<string>> GetOryxSdkVersionsAsync(string url)
        {
            HashSet<string> versions = new HashSet<string>();
            var response = await this.GetDataAsync(url);

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
            this.httpClient?.Dispose();
        }
    }
}