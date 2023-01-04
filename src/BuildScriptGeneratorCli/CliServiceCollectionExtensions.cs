// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGeneratorCli.Options;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal static class CliServiceCollectionExtensions
    {
        public static IServiceCollection AddCliServices(this IServiceCollection services, IConsole console = null)
        {
            _ = services.AddOptionsServices();
            _ = services.AddSingleton<IConsole, PhysicalConsole>();
            _ = services.AddSingleton<CliEnvironmentSettings>();
            return console == null ?
                services.AddSingleton<IStandardOutputWriter, DefaultStandardOutputWriter>() :
                services.AddSingleton<IStandardOutputWriter>(new DefaultStandardOutputWriter(
                                                (message) => { _ = console.Write(message); },
                                                (message) => { _ = console.WriteLine(message); }));
        }
    }
}