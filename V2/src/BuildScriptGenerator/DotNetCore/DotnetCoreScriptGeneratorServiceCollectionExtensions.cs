// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.BuildScriptGenerator.DotNetCore;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class DotNetCoreScriptGeneratorServiceCollectionExtensions
    {
        public static IServiceCollection AddDotNetCoreScriptGeneratorServices(this IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProgrammingPlatform, DotNetCorePlatform>());
            services.AddSingleton<IDotNetCoreVersionProvider, DotNetCoreVersionProvider>();
            services.AddSingleton<DotNetCoreOnDiskVersionProvider>();
            services.AddSingleton<DotNetCoreSdkStorageVersionProvider>();
            services.AddSingleton<DotNetCorePlatformInstaller>();
            services.AddSingleton<GlobalJsonSdkResolver>();
            return services;
        }
    }
}