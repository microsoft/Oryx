// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Oryx.Automation.Client;
using Microsoft.Oryx.Automation.Services;
using Microsoft.Oryx.Automation.Telemetry;

namespace Microsoft.Oryx.Automation
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            string oryxRootPath = args[0];

            var serviceProvider = new ServiceCollection()
                .AddScoped<IHttpClient, HttpClientImpl>()
                .AddScoped<ILogger, LoggerImpl>()
                .AddSingleton<IVersionService, VersionService>()
                .AddSingleton<IYamlFileReaderService, YamlFileReaderService>()
                .AddScoped<DotNet.DotNet>()
                .AddLogging()
                .BuildServiceProvider();

            var dotNet = serviceProvider.GetRequiredService<DotNet.DotNet>();
            await dotNet.RunAsync(oryxRootPath);
        }
    }
}
