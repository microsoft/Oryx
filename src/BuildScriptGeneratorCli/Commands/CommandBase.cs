// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal abstract class CommandBase
    {
        private IServiceProvider _serviceProvider = null;

        [Option(
            "--log-file <file>",
            CommandOptionType.SingleValue,
            Description = "The file to which the log will be written.")]
        public string LogFilePath { get; set; }

        [Option("--debug", Description = "Print stack traces for exceptions.")]
        public bool ShowStackTrace { get; set; }

        public int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.CancelKeyPress += Console_CancelKeyPress;

            try
            {
                _serviceProvider = GetServiceProvider();
                _serviceProvider?.GetRequiredService<ILogger<CommandBase>>()?.LogInformation("Oryx command line: {cmdLine}", Environment.CommandLine);
                if (!IsValidInput(_serviceProvider, console))
                {
                    return ProcessConstants.ExitFailure;
                }

                if (ShowStackTrace)
                {
                    console.WriteLine("Debug mode enabled");
                }

                return Execute(_serviceProvider, console);
            }
            catch (InvalidUsageException e)
            {
                console.Error.WriteLine(e.Message);
                return ProcessConstants.ExitFailure;
            }
            catch (Exception exc)
            {
                _serviceProvider?.GetRequiredService<ILogger<CommandBase>>()?.LogError(exc, "Exception caught");

                console.Error.WriteLine(Constants.GenericErrorMessage);
                if (ShowStackTrace)
                {
                    console.Error.WriteLine("Exception.ToString():");
                    console.Error.WriteLine(exc.ToString());
                }

                return ProcessConstants.ExitFailure;
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
                .ConfigureScriptGenerationOptions(opts => ConfigureBuildScriptGeneratorOptions(opts));
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