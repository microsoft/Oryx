// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
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
                ServiceDescriptor.Singleton<ILanguageScriptGenerator, NodeScriptGenerator>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IConfigureOptions<NodeScriptGeneratorOptions>, NodeScriptGeneratorOptionsSetup>());
            services.AddSingleton<INodeVersionProvider, NodeVersionProvider>();
            return services;
        }
    }
}
