// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal static class CliServiceCollectionExtensions
    {
        public static IServiceCollection AddCliServices(this IServiceCollection services, IConsole console = null)
        {
            services.AddSingleton<IConsole, PhysicalConsole>();
            services.AddSingleton<CliEnvironmentSettings>();
            return console == null ?
                services.AddSingleton<IStandardOutputWriter, DefaultStandardOutputWriter>() :
                services.AddSingleton<IStandardOutputWriter>(new DefaultStandardOutputWriter(
                                                (message) => { console.Write(message); },
                                                (message) => { console.WriteLine(message); }));
        }
    }
}