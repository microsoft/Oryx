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
            _ = services.AddSingleton<IGolangVersionProvider, GolangVersionProvider>();
            _ = services.AddSingleton<GolangPlatformInstaller>();
            _ = services.AddSingleton<GolangOnDiskVersionProvider>();
            _ = services.AddSingleton<GolangSdkStorageVersionProvider>();
            return services;
        }
    }
}
