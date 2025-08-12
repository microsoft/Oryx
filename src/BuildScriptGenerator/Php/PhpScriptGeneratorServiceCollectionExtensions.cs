// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.BuildScriptGenerator.Php;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class PhpScriptGeneratorServiceCollectionExtensions
    {
        public static IServiceCollection AddPhpScriptGeneratorServices(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IProgrammingPlatform, PhpPlatform>());
            services.AddSingleton<IPhpVersionProvider, PhpVersionProvider>();
            services.AddSingleton<IPhpComposerVersionProvider, PhpComposerVersionProvider>();
            services.AddSingleton<PhpPlatformInstaller>();
            services.AddSingleton<PhpComposerInstaller>();
            services.AddSingleton<PhpOnDiskVersionProvider>();
            services.AddSingleton<PhpComposerOnDiskVersionProvider>();
            services.AddSingleton<PhpSdkStorageVersionProvider>();
            services.AddSingleton<PhpExternalVersionProvider>();
            services.AddSingleton<PhpComposerSdkStorageVersionProvider>();
            services.AddSingleton<PhpComposerExternalVersionProvider>();
            return services;
        }
    }
}