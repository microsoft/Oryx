// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

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
                _serviceProvider = TryGetServiceProvider(console);
                if (_serviceProvider == null)
                {
                    return ProcessConstants.ExitFailure;
                }

                logger = _serviceProvider?.GetRequiredService<ILogger<CommandBase>>();
                logger?.LogInformation("Oryx command line: {cmdLine}", Environment.CommandLine);

                var envSettings = _serviceProvider?.GetRequiredService<CliEnvironmentSettings>();
                if (envSettings != null && envSettings.GitHubActions)
                {
                    logger?.LogInformation("The current Oryx command is being run from within a GitHub Action.");

                    DateTime startTime, endTime;
                    if (envSettings.GitHubActionsBuildImagePullStartTime != null
                        && envSettings.GitHubActionsBuildImagePullEndTime != null
                        && DateTime.TryParse(envSettings.GitHubActionsBuildImagePullStartTime, out startTime)
                        && DateTime.TryParse(envSettings.GitHubActionsBuildImagePullEndTime, out endTime))
                    {
                        TimeSpan interval = endTime - startTime;
                        var gitHubActionBuildImagePullDurationSeconds = interval.TotalSeconds.ToString();
                        var buildEventProps = new Dictionary<string, string>()
                        {
                            { "gitHubActionBuildImagePullDurationSeconds", gitHubActionBuildImagePullDurationSeconds },
                        };

                        logger.LogEvent("GitHubActionsBuildImagePullDurationLog", buildEventProps);
                    }
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

        internal virtual IServiceProvider TryGetServiceProvider(IConsole console)
        {
            // Don't use the IConsole instance in this method -- override this method in the command
            // and pass IConsole through to ServiceProviderBuilder to write to the output.
            var serviceProviderBuilder = new ServiceProviderBuilder(LogFilePath)
                .ConfigureServices(services =>
                {
                    // Add an empty and default configuration to prevent some commands from breaking since options
                    // setup expect this from DI.
                    var configuration = new ConfigurationBuilder().Build();
                    services.AddSingleton<IConfiguration>(configuration);
                })
                .ConfigureScriptGenerationOptions(opts => ConfigureBuildScriptGeneratorOptions(opts));
            return serviceProviderBuilder.Build();
        }

        protected string GetBeginningCommandOutputLog()
        {
            var output = new StringBuilder();
            output.AppendLine("Operation performed by Microsoft Oryx, https://github.com/Microsoft/Oryx");
            output.AppendLine("You can report issues at https://github.com/Microsoft/Oryx/issues");
            var buildInfo = new DefinitionListFormatter();
            var oryxVersion = Program.GetVersion();
            var oryxCommitId = Program.GetMetadataValue(Program.GitCommit);
            var oryxReleaseTagName = Program.GetMetadataValue(Program.ReleaseTagName);
            buildInfo.AddDefinition(
                "Oryx Version",
                $"{oryxVersion}, " +
                $"Commit: {oryxCommitId}, " +
                $"ReleaseTagName: {oryxReleaseTagName}");
            output.AppendLine();
            output.Append(buildInfo.ToString());
            return output.ToString();
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