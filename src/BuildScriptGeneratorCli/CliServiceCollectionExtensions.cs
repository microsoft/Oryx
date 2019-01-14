// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal static class CliServiceCollectionExtensions
    {
        public static IServiceCollection AddCliServices(this IServiceCollection services)
        {
            services.AddSingleton<IConsole, PhysicalConsole>();
            return services;
        }
    }
}