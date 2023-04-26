// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.CommandLine;
using System.CommandLine.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGeneratorCli.Options;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal static class CliServiceCollectionExtensions
    {
        public static IServiceCollection AddCliServices(this IServiceCollection services, IConsole console = null)
        {
            services.AddOptionsServices();
            services.AddSingleton<IConsole, SystemConsole>();
            services.AddSingleton<CliEnvironmentSettings>();
            return console == null ?
                services.AddSingleton<IStandardOutputWriter, DefaultStandardOutputWriter>() :
                services.AddSingleton<IStandardOutputWriter>(new DefaultStandardOutputWriter(
                                                (message) => { console.Write(message); },
                                                (message) => { console.WriteLine(message); }));
        }
    }
}