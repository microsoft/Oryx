// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Node;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class NodeScriptGeneratorServiceCollectionExtensions
    {
        public static IServiceCollection AddNodeScriptGeneratorServices(this IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILanguageDetector, NodeLanguageDetector>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProgrammingPlatform, NodePlatform>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IConfigureOptions<NodeScriptGeneratorOptions>, NodeScriptGeneratorOptionsSetup>());
            services.AddSingleton<INodeVersionProvider, NodeVersionProvider>();
            services.AddScoped<NodeLanguageDetector>();
            services.AddScoped<NodePlatformInstaller>();
            return services;
        }
    }
}