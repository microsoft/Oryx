// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Oryx.BuildScriptGenerator.SourceRepo;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public static class BuildScriptGeneratorServiceCollectionExtensions
    {
        public static IServiceCollection AddBuildScriptGeneratorServices(this IServiceCollection services)
        {
            services.AddNodeScriptGeneratorServices();

            services.AddSingleton<IScriptGeneratorProvider, DefaultScriptGeneratorProvider>();
            services.AddSingleton<IEnvironment, DefaultEnvironment>();
            services.AddSingleton<ISourceRepo, LocalSourceRepo>();

            return services;
        }
    }
}
