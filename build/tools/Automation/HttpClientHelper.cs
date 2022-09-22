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
            Console.WriteLine($"url: {url}");

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
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return string.Empty;
            }
        }
    }
}