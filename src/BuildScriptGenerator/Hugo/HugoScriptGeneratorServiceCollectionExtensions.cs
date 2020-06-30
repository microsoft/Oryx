// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.BuildScriptGenerator.Hugo;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class HugoScriptGeneratorServiceCollectionExtensions
    {
        public static IServiceCollection AddHugoScriptGeneratorServices(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IProgrammingPlatform, HugoPlatform>());
            services.AddSingleton<HugoPlatformInstaller>();
            return services;
        }
    }
}