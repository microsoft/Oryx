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
        public bool DebugMode { get; set; }

        public int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.CancelKeyPress += Console_CancelKeyPress;

            ILogger<CommandBase> logger = null;

            try
            {
                _serviceProvider = GetServiceProvider();

                logger = _serviceProvider?.GetRequiredService<ILogger<CommandBase>>();
                logger?.LogInformation("Oryx command line: {cmdLine}", Environment.CommandLine);

                if (!IsValidInput(_serviceProvider, console))
                {
                    return ProcessConstants.ExitFailure;
                }

                if (DebugMode)
                {
                    console.WriteLine("Debug mode enabled");
                }

                using (var timedEvent = logger?.LogTimedEvent(GetType().Name))
                {
                    var exitCode = Execute(_serviceProvider, console);
                    timedEvent?.AddProperty(nameof(exitCode), exitCode.ToString());
                    return exitCode;
                }
            }
            catch (InvalidUsageException e)
            {
                console.WriteErrorLine(e.Message);
                return ProcessConstants.ExitFailure;
            }
            catch (Exception exc)
            {
                logger?.LogError(exc, "Exception caught");

                console.WriteErrorLine(Constants.GenericErrorMessage);
                if (DebugMode)
                {
                    console.WriteErrorLine(exc.ToString());
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