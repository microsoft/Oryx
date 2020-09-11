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
            services.AddSingleton<MavenInstaller>();
            services.AddSingleton<JavaPlatformInstaller>();
            services.AddSingleton<IJavaVersionProvider, JavaVersionProvider>();
            services.AddSingleton<JavaOnDiskVersionProvider>();
            services.AddSingleton<JavaSdkStorageVersionProvider>();
            services.AddSingleton<IMavenVersionProvider, MavenVersionProvider>();
            services.AddSingleton<MavenOnDiskVersionProvider>();
            services.AddSingleton<MavenSdkStorageVersionProvider>();
            return services;
        }
    }
}