using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using BlazorApp.Shared;

namespace BlazorApp.Api
{
    public static class WeatherForecastFunction
    {
        private static string GetSummary(int temp)
        {
            var summary = "Mild";

            if (temp >= 32)
            {
                summary = "Hot";
            }
            else if (temp <= 16 && temp > 0)
            {
                summary = "Cold";
            }
            else if (temp <= 0)
            {
                summary = "Freezing!";
            }

            return summary;
        }

        [FunctionName("WeatherForecast")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var randomNumber = new Random();
            var temp = 0;

            var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = temp = randomNumber.Next(-20, 55),
                Summary = GetSummary(temp)
            }).ToArray();

            return new OkObjectResult(result);
        }
    }
}
