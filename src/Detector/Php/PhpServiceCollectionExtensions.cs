// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.Detector.Php;

namespace Microsoft.Oryx.Detector
{
    internal static class PhpServiceCollectionExtensions
    {
        public static IServiceCollection AddPhpServices(this IServiceCollection services)
        {
            services.AddSingleton<PhpDetector>();

            // Factory to make sure same detector instance is returned when same implementation type is resolved via
            // multiple inteface types.
            Func<IServiceProvider, PhpDetector> factory = (sp) => sp.GetRequiredService<PhpDetector>();
            services.AddSingleton<IPhpPlatformDetector, PhpDetector>(factory);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPlatformDetector, PhpDetector>(factory));
            return services;
        }
    }
}
