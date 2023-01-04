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
            _ = services.AddSingleton<INodeVersionProvider, NodeVersionProvider>();
            _ = services.AddSingleton<NodePlatformInstaller>();
            _ = services.AddSingleton<NodeOnDiskVersionProvider>();
            _ = services.AddSingleton<NodeSdkStorageVersionProvider>();
            return services;
        }
    }
}