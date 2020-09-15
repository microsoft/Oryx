// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.BuildScriptGenerator.Ruby;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class RubyScriptGeneratorServiceCollectionExtensions
    {
        public static IServiceCollection AddScriptGeneratorServicesRuby(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IProgrammingPlatform, RubyPlatform>());
            services.AddSingleton<IRubyVersionProvider, RubyVersionProvider>();
            services.AddSingleton<RubyPlatformInstaller>();
            services.AddSingleton<RubyOnDiskVersionProvider>();
            services.AddSingleton<RubySdkStorageVersionProvider>();
            return services;
        }
    }
}