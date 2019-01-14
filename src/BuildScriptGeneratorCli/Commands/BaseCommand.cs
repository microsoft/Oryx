// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Common.Utilities;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal abstract class BaseCommand
    {
        private IServiceProvider _serviceProvider = null;

        [Option("--log-file <file>", CommandOptionType.SingleValue, Description = "The file to which the log will be written to.")]
        public string LogFilePath { get; set; }

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
            catch (Exception exc)
            {
                _serviceProvider?.GetRequiredService<ILogger<BaseCommand>>()?.LogError(exc, "Exception caught");
                console.Error.WriteLine(Constants.GenericErrorMessage);
                return Constants.ExitFailure;
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
            var serviceProviderBuilder = new ServiceProviderBuilder(LogFilePath)
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