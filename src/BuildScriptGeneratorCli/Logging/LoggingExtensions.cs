// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Logging
{
    internal static class LoggingExtensions
    {
        public static ILoggingBuilder AddFile(this ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
            return loggingBuilder;
        }
    }
}
