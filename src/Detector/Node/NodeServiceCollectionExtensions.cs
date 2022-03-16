// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.Detector.Node;

namespace Microsoft.Oryx.Detector
{
    internal static class NodeServiceCollectionExtensions
    {
        public static IServiceCollection AddNodeServices(this IServiceCollection services)
        {
            services.AddSingleton<NodeDetector>();

            // Factory to make sure same detector instance is returned when same implementation type is resolved via
            // multiple inteface types.
            Func<IServiceProvider, NodeDetector> factory = (sp) => sp.GetRequiredService<NodeDetector>();
            services.AddSingleton<INodePlatformDetector, NodeDetector>(factory);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IPlatformDetector, NodeDetector>(factory));
            return services;
        }
    }
}
