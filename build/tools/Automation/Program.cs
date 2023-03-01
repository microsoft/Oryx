// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Oryx.Automation.DotNet;
using Microsoft.Oryx.Automation.Python;
using Microsoft.Oryx.Automation.Services;
using Oryx.Microsoft.Automation.Python;

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

            string oryxRootPath = args[1];
            string platform = args[0].ToLower();
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IHttpService, HttpService>()
                .AddSingleton<IFileService>(new FileService(oryxRootPath))
                .AddSingleton<IVersionService, VersionService>()
                .AddSingleton<IYamlFileService>(new YamlFileService(oryxRootPath))
                .AddScoped<DotNetAutomator>()
                .AddScoped<PythonAutomator>()
                .AddHttpClient()
                .BuildServiceProvider();

            switch (platform)
            {
                case DotNetConstants.DotNetName:
                    var dotNetAutomator = serviceProvider.GetRequiredService<DotNetAutomator>();
                    await dotNetAutomator.RunAsync();
                    break;
                case PythonConstants.PythonName:
                    var pythonAutomator = serviceProvider.GetRequiredService<PythonAutomator>();
                    await pythonAutomator.RunAsync();
                    break;
                default:
                    Console.WriteLine($"Unsupported platform: {platform}");
                    Console.WriteLine($"Supported platforms: {string.Join(", ", supportedPlatforms)}");
                    break;
            }
        }
    }
}
