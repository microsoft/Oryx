// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.ApplicationInsights;
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
                _serviceProvider = GetServiceProvider(console);

                logger = _serviceProvider?.GetRequiredService<ILogger<CommandBase>>();
                logger?.LogInformation("Oryx command line: {cmdLine}", Environment.CommandLine);

                var envSettings = _serviceProvider?.GetRequiredService<CliEnvironmentSettings>();
                if (envSettings != null && envSettings.GitHubActions)
                {
                    logger?.LogInformation("The current Oryx command is being run from within a GitHub Action.");

                    // Format: "2020-02-15T02:51:50.000Z"
                    var gitHubActionBuildContainerStartTime = envSettings.GitHubActionsBuildContainerStartTime;
                    var gitHubActionBuildContainerEndTime = envSettings.GitHubActionsBuildContainerEndTime;

                    var dateStart = gitHubActionBuildContainerStartTime.Split('T')[0];
                    var timeStart = gitHubActionBuildContainerStartTime.Split('T')[1];

                    var dateEnd = gitHubActionBuildContainerEndTime.Split('T')[0];
                    var timeEnd = gitHubActionBuildContainerEndTime.Split('T')[1];

                    DateTime date1 = new DateTime(int.Parse(dateStart.Substring(0, 4)), int.Parse(dateStart.Substring(5, 2)), int.Parse(dateStart.Substring(8, 2)),
                                                  int.Parse(timeStart.Substring(0, 2)), int.Parse(timeStart.Substring(3, 2)), int.Parse(timeStart.Substring(6, 2)));
                    DateTime date2 = new DateTime(int.Parse(dateEnd.Substring(0, 4)), int.Parse(dateEnd.Substring(5, 2)), int.Parse(dateEnd.Substring(8, 2)),
                                                  int.Parse(timeEnd.Substring(0, 2)), int.Parse(timeEnd.Substring(3, 2)), int.Parse(timeEnd.Substring(6, 2)));
                    TimeSpan interval = date2 - date1;
                    var gitHubActionBuildContainerDurationSeconds = interval.ToString();
                    var buildEventProps = new Dictionary<string, string>()
                    {
                        { "gitHubActionBuildContainerStartTime", gitHubActionBuildContainerStartTime },
                        { "gitHubActionBuildContainerEndTime", gitHubActionBuildContainerEndTime },
                        { "gitHubActionBuildContainerDurationSeconds", gitHubActionBuildContainerDurationSeconds },
                    };
                    logger.LogEvent("GitHubActionsBuildContainerTimeLog", buildEventProps);
                }

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

        internal virtual IServiceProvider GetServiceProvider(IConsole console)
        {
            // Don't use the IConsole instance in this method -- override this method in the command
            // and pass IConsole through to ServiceProviderBuilder to write to the output.
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