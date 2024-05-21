// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.BuildScriptGenerator.Node;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class NodeScriptGeneratorServiceCollectionExtensions
    {
        public static IServiceCollection AddNodeScriptGeneratorServices(this IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProgrammingPlatform, NodePlatform>());
            services.AddSingleton<INodeVersionProvider, NodeVersionProvider>();
            services.AddSingleton<NodePlatformInstaller>();
            services.AddSingleton<NodeOnDiskVersionProvider>();
            services.AddSingleton<NodeSdkStorageVersionProvider>();
            return services;
        }
    }
}