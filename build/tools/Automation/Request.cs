namespace Microsoft.Oryx.Automation
{
    /// <Summary>
    /// TODO: write summary.
    /// </Summary>
    public static class Request
    {
        private static readonly HttpClient Client = new HttpClient();

        /// <Summary>
        /// TODO: write summary.
        /// </Summary>
        public static async Task<string> RequestAsync(string url)
        {
            // TODO: clean up code by removing null return option
            Console.WriteLine($"url: {url}");

            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                HttpResponseMessage response = await Client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // var response = JsonSerializer.Deserialize

                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri

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