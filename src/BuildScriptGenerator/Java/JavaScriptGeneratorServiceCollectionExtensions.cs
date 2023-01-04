// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.BuildScriptGenerator.Java;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class JavaScriptGeneratorServiceCollectionExtensions
    {
        public static IServiceCollection AddScriptGeneratorServicesForJava(this IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IProgrammingPlatform, JavaPlatform>());
            _ = services.AddSingleton<MavenInstaller>();
            _ = services.AddSingleton<JavaPlatformInstaller>();
            _ = services.AddSingleton<IJavaVersionProvider, JavaVersionProvider>();
            _ = services.AddSingleton<JavaOnDiskVersionProvider>();
            _ = services.AddSingleton<JavaSdkStorageVersionProvider>();
            _ = services.AddSingleton<IMavenVersionProvider, MavenVersionProvider>();
            _ = services.AddSingleton<MavenOnDiskVersionProvider>();
            _ = services.AddSingleton<MavenSdkStorageVersionProvider>();
            return services;
        }
    }
}