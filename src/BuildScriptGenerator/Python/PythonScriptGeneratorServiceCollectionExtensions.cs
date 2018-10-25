// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Python;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    internal static class PythonScriptGeneratorServiceCollectionExtensions
    {
        public static IServiceCollection AddPythonScriptGeneratorServices(this IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILanguageDetector, PythonLanguageDetector>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILanguageScriptGenerator, PythonScriptGenerator>());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IConfigureOptions<PythonScriptGeneratorOptions>, PythonScriptGeneratorOptionsSetup>());
            services.AddSingleton<IPythonVersionProvider, PythonVersionProvider>();
            return services;
        }
    }
}
