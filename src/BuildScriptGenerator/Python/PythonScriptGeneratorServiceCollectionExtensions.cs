// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.BuildScriptGenerator.Python;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class PythonScriptGeneratorServiceCollectionExtensions
    {
        public static IServiceCollection AddPythonScriptGeneratorServices(this IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProgrammingPlatform, PythonPlatform>());
            _ = services.AddSingleton<IPythonVersionProvider, PythonVersionProvider>();
            _ = services.AddSingleton<PythonPlatformInstaller>();
            _ = services.AddSingleton<PythonOnDiskVersionProvider>();
            _ = services.AddSingleton<PythonSdkStorageVersionProvider>();
            return services;
        }
    }
}