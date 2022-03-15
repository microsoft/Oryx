// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.Detector.Hugo;

namespace Microsoft.Oryx.Detector
{
    internal static class HugoServiceCollectionExtensions
    {
        public static IServiceCollection AddHugoServices(this IServiceCollection services)
        {
            services.AddSingleton<HugoDetector>();

            // Factory to make sure same detector instance is returned when same implementation type is resolved via
            // multiple inteface types.
            Func<IServiceProvider, HugoDetector> factory = (sp) => sp.GetRequiredService<HugoDetector>();
            services.AddSingleton<IHugoPlatformDetector, HugoDetector>(factory);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPlatformDetector, HugoDetector>(factory));
            return services;
        }
    }
}
