// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Oryx.Automation
{
    public static class HttpClientHelper
    {
        private static readonly HttpClient Client = new HttpClient();

        /// <Summary>
        /// Performs a request for a given URL.
        /// </Summary>
        public static async Task<string> GetRequestStringAsync(string url)
        {
            Console.WriteLine($"[GetRequestStringAsync] Making GET request against provided URL: {url}");

            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                HttpResponseMessage response = await Client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                return responseBody ?? string.Empty;
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n[GetRequestStringAsync] Unable to process request to {url}");
                Console.WriteLine($"Message :{e.Message} ");
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                System.Environment.Exit(1);
            }

            return string.Empty;
        }
    }
}