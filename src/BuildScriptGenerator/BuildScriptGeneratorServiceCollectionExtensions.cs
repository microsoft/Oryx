// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public static class BuildScriptGeneratorServiceCollectionExtensions
    {
        public static IServiceCollection AddBuildScriptGeneratorServices(this IServiceCollection services)
        {
            services
                .AddNodeScriptGeneratorServices()
                .AddPythonScriptGeneratorServices()
                .AddDotNetCoreScriptGeneratorServices()
                .AddPhpScriptGeneratorServices();

            services.AddSingleton<IBuildScriptGenerator, DefaultBuildScriptGenerator>();
            services.AddSingleton<ICompatiblePlatformDetector, DefaultCompatiblePlatformDetector>();
            services.AddSingleton<IDockerfileGenerator, DefaultDockerfileGenerator>();
            services.AddSingleton<IEnvironment, DefaultEnvironment>();
            services.AddSingleton<ISourceRepoProvider, DefaultSourceRepoProvider>();
            services.AddSingleton<ITempDirectoryProvider, DefaulTempDirectoryProvider>();
            services.AddSingleton<IScriptExecutor, DefaultScriptExecutor>();
            services.AddSingleton<IEnvironmentSettingsProvider, DefaultEnvironmentSettingsProvider>();
            services.AddSingleton<IRunScriptGenerator, DefaultRunScriptGenerator>();

            // Add all checkers (platform-dependent + platform-independent)
            foreach (Type type in typeof(BuildScriptGeneratorServiceCollectionExtensions).Assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(CheckerAttribute), false).Length > 0)
                {
                    services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IChecker), type));
                }
            }

            return services;
        }
    }
}