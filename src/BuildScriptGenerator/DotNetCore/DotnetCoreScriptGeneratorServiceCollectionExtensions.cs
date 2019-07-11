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
    internal static class DotNetCoreScriptGeneratorServiceCollectionExtensions
    {
        public static IServiceCollection AddDotNetCoreScriptGeneratorServices(this IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILanguageDetector, DotNetCoreLanguageDetector>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProgrammingPlatform, DotNetCorePlatform>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IConfigureOptions<DotNetCoreScriptGeneratorOptions>, DotNetCoreScriptGeneratorOptionsSetup>());
            services.AddSingleton<IDotNetCoreVersionProvider, DotNetCoreVersionProvider>();
            services.AddScoped<DotNetCoreLanguageDetector>();
            services.AddSingleton<IProjectFileProvider, AspNetCoreWebAppProjectFileProvider>();
            return services;
        }
    }
}