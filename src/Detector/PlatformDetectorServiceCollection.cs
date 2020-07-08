// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.Detector
{
    public static class PlatformDetectorServiceCollection
    {
        public static IServiceCollection AddPlatformDetectorServices(this IServiceCollection services)
        {
            services.AddSingleton<IDetector, DefaultPlatformDetector>();
            services.AddSingleton<IConfigureOptions<DetectorOptions>, DetectorOptionsSetup>();

            services
                .AddDotNetCoreServices()
                .AddNodeServices()
                .AddPythonServices()
                .AddPhpServices()
                .AddHugoServices();

            return services;
        }
    }
}