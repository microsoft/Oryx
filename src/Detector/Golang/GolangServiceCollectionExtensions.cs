// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.Detector.Golang;

namespace Microsoft.Oryx.Detector
{
    internal static class GolangServiceCollectionExtensions
    {
        public static IServiceCollection AddGolangServices(this IServiceCollection services)
        {
            services.AddSingleton<GolangDetector>();

            // Factory to make sure same detector instance is returned when same implementation type is resolved via
            // multiple inteface types.
            Func<IServiceProvider, GolangDetector> factory = (sp) => sp.GetRequiredService<GolangDetector>();
            services.AddSingleton<IGolangPlatformDetector, GolangDetector>(factory);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPlatformDetector, GolangDetector>(factory));
            return services;
        }
    }
}
