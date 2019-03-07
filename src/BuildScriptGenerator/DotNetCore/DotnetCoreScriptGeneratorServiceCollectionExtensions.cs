// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class DotnetCoreScriptGeneratorServiceCollectionExtensions
    {
        public static IServiceCollection AddDotnetCoreScriptGeneratorServices(this IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILanguageDetector, DotnetCoreLanguageDetector>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProgrammingPlatform, DotnetCorePlatform>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IConfigureOptions<DotnetCoreScriptGeneratorOptions>, DotnetCoreScriptGeneratorOptionsSetup>());
            services.AddSingleton<IDotnetCoreVersionProvider, DotnetCoreVersionProvider>();
            services.AddScoped<DotnetCoreLanguageDetector>();
            services.AddSingleton<IAspNetCoreWebAppProjectFileProvider, DefaultAspNetCoreWebAppProjectFileProvider>();
            return services;
        }
    }
}