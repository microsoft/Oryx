// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.BuildScriptGenerator.Golang;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class GolangScriptGeneratorServiceCollectionExtensions
    {
        public static IServiceCollection AddScriptGeneratorServicesGolang(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IProgrammingPlatform, GolangPlatform>());
            services.AddSingleton<IGolangVersionProvider, GolangVersionProvider>();
            services.AddSingleton<GolangPlatformInstaller>();
            services.AddSingleton<GolangOnDiskVersionProvider>();
            services.AddSingleton<GolangSdkStorageVersionProvider>();
            return services;
        }
    }
}
