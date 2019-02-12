// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Php;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class PhpScriptGeneratorServiceCollectionExtensions
    {
        public static IServiceCollection AddPhpScriptGeneratorServices(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<ILanguageDetector, PhpLanguageDetector>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IProgrammingPlatform, PhpPlatform>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<PhpScriptGeneratorOptions>, PhpScriptGeneratorOptionsSetup>());
            services.AddSingleton<IPhpVersionProvider, PhpVersionProvider>();
            services.AddScoped<PhpLanguageDetector>();
            return services;
        }
    }
}