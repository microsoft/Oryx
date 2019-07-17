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

            // Note that the order of these project file providers is important. For example, if a user explicitly
            // specifies a project file using either the 'PROJECT' environment or the 'project' build property, we want
            // to use that. In that case we want the ExplicitProjectFileProvider to return the project file and not
            // probe for files.
            services.AddSingleton<DefaultProjectFileProvider>();
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProjectFileProvider, ExplicitProjectFileProvider>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProjectFileProvider, RootDirectoryProjectFileProvider>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProjectFileProvider, ProbeAndFindProjectFileProvider>());
            return services;
        }
    }
}