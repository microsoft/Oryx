// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.Detector.Go;

namespace Microsoft.Oryx.Detector
{
    internal static class GoServiceCollectionExtensions
    {
        public static IServiceCollection AddGoServices(this IServiceCollection services)
        {
            services.AddSingleton<GoDetector>();
            // Factory to make sure same detector instance is returned when same implementation type is resolved via
            // multiple inteface types.
            Func<IServiceProvider, GoDetector> factory = (sp) => sp.GetRequiredService<GoDetector>();
            services.AddSingleton<IGoPlatformDetector, GoDetector>(factory);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPlatformDetector, GoDetector>(factory));
            return services;
        }
    }
}
