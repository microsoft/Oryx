// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.Detector.Java;

namespace Microsoft.Oryx.Detector
{
    internal static class JavaServiceCollectionExtensions
    {
        public static IServiceCollection AddJavaServices(this IServiceCollection services)
        {
            services.AddSingleton<JavaDetector>();

            // Factory to make sure same detector instance is returned when same implementation type is resolved via
            // multiple inteface types.
            Func<IServiceProvider, JavaDetector> factory = (sp) => sp.GetRequiredService<JavaDetector>();
            services.AddSingleton<IJavaPlatformDetector, JavaDetector>(factory);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPlatformDetector, JavaDetector>(factory));
            return services;
        }
    }
}
