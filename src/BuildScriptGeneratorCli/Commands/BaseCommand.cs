// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Common.Utilities;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal abstract class BaseCommand
    {
        private IServiceProvider _serviceProvider = null;

        public int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.CancelKeyPress += Console_CancelKeyPress;

            try
            {
                _serviceProvider = GetServiceProvider();
                if (!IsValidInput(_serviceProvider, console))
                {
                    return Constants.ExitFailure;
                }

                return Execute(_serviceProvider, console);
            }
            finally
            {
                DisposeServiceProvider();
            }
        }

        internal abstract int Execute(IServiceProvider serviceProvider, IConsole console);

        internal virtual void ConfigureBuildScriptGeneratorOptions(BuildScriptGeneratorOptions options)
        {
        }

        internal virtual bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            return true;
        }

        internal virtual IServiceProvider GetServiceProvider()
        {
            var serviceProviderBuilder = new ServiceProviderBuilder()
                .ConfigureScriptGenerationOptions(o =>
                {
                    ConfigureBuildScriptGeneratorOptions(o);
                });
            return serviceProviderBuilder.Build();
        }

        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            DisposeServiceProvider();
        }

        private void DisposeServiceProvider()
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            // Sends queued messages to Application Insights
            NLog.LogManager.Flush(LoggingConstants.FlushTimeout);
            NLog.LogManager.Shutdown();
        }
    }
}