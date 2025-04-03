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
            services.AddSingleton<IPythonVersionProvider, PythonVersionProvider>();
            services.AddSingleton<PythonPlatformInstaller>();
            services.AddSingleton<PythonOnDiskVersionProvider>();
            services.AddSingleton<PythonSdkStorageVersionProvider>();
            services.AddSingleton<PythonExternalVersionProvider>();
            return services;
        }
    }
}