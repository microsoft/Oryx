// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Oryx.Automation.Services;

namespace Microsoft.Oryx.Automation
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            HashSet<string> supportedPlatforms = new HashSet<string>() { "dotnet" };
            if (args.Length < 2)
            {
                Console.WriteLine($"\nPlease enter a supported platform (e.g. {string.Join(", ", supportedPlatforms)}) and the Oryx root path.");
                Environment.Exit(1);
            }

            var serviceProvider = new ServiceCollection()
                .AddHttpClient()
                .AddSingleton<IVersionService, VersionService>()
                .AddSingleton<IYamlFileService, YamlFileService>()
                .AddScoped<DotNet.DotNet>()
                .BuildServiceProvider();

            string oryxRootPath = args[1];
            string platform = args[0].ToLower();
            switch (platform)
            {
                case "dotnet":
                    var dotNet = serviceProvider.GetRequiredService<DotNet.DotNet>();
                    await dotNet.RunAsync(oryxRootPath);
                    break;

                default:
                    Console.WriteLine($"Unsupported platform: {platform}");
                    Console.WriteLine($"Supported platforms: {string.Join(", ", supportedPlatforms)}");
                    break;
            }
        }
    }
}
