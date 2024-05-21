// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.Detector.Ruby;

namespace Microsoft.Oryx.Detector
{
    internal static class RubyServiceCollectionExtensions
    {
        public static IServiceCollection AddRubyServices(this IServiceCollection services)
        {
            services.AddSingleton<RubyDetector>();

            // Factory to make sure same detector instance is returned when same implementation type is resolved via
            // multiple inteface types.
            Func<IServiceProvider, RubyDetector> factory = (sp) => sp.GetRequiredService<RubyDetector>();
            services.AddSingleton<IRubyPlatformDetector, RubyDetector>(factory);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPlatformDetector, RubyDetector>(factory));
            return services;
        }
    }
}
