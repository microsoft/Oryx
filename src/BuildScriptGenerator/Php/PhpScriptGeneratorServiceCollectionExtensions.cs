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
            _ = services.AddSingleton<IPhpVersionProvider, PhpVersionProvider>();
            _ = services.AddSingleton<IPhpComposerVersionProvider, PhpComposerVersionProvider>();
            _ = services.AddSingleton<PhpPlatformInstaller>();
            _ = services.AddSingleton<PhpComposerInstaller>();
            _ = services.AddSingleton<PhpOnDiskVersionProvider>();
            _ = services.AddSingleton<PhpComposerOnDiskVersionProvider>();
            _ = services.AddSingleton<PhpSdkStorageVersionProvider>();
            _ = services.AddSingleton<PhpComposerSdkStorageVersionProvider>();
            return services;
        }
    }
}