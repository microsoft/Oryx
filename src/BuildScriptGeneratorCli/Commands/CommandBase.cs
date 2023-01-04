// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal abstract class CommandBase
    {
        private IServiceProvider serviceProvider;

        [Option(
            "--log-file <file>",
            CommandOptionType.SingleValue,
            Description = "The file to which the log will be written.")]
        public string LogFilePath { get; set; }

        [Option("--debug", Description = "Print stack traces for exceptions.")]
        public bool DebugMode { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "All arguments are necessary for OnExecute call, even if not used.")]
        public int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.CancelKeyPress += this.Console_CancelKeyPress;

            ILogger<CommandBase> logger = null;

            try
            {
                this.serviceProvider = this.TryGetServiceProvider(console);
                if (this.serviceProvider == null)
                {
                    return ProcessConstants.ExitFailure;
                }

                logger = this.serviceProvider?.GetRequiredService<ILogger<CommandBase>>();
                logger?.LogInformation("Oryx command line: {cmdLine}", Environment.CommandLine);

                var envSettings = this.serviceProvider?.GetRequiredService<CliEnvironmentSettings>();
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

                if (!this.IsValidInput(this.serviceProvider, console))
                {
                    return ProcessConstants.ExitFailure;
                }

                if (this.DebugMode)
                {
                    _ = console.WriteLine("Debug mode enabled");
                }

                using (var timedEvent = logger?.LogTimedEvent(this.GetType().Name))
                {
                    var options = this.serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;
                    var exitCode = this.Execute(this.serviceProvider, console);
                    timedEvent?.AddProperty(nameof(exitCode), exitCode.ToString());
                    timedEvent?.AddProperty("callerId", options.CallerId);
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
                console.WriteErrorLine(exc.ToString());

                return ProcessConstants.ExitFailure;
            }
            finally
            {
                this.DisposeServiceProvider();
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
            var serviceProviderBuilder = new ServiceProviderBuilder(this.LogFilePath)
                .ConfigureServices(services =>
                {
                    // Add an empty and default configuration to prevent some commands from breaking since options
                    // setup expect this from DI.
                    var configuration = new ConfigurationBuilder().Build();
                    _ = services.AddSingleton<IConfiguration>(configuration);
                })
                .ConfigureScriptGenerationOptions(opts => this.ConfigureBuildScriptGeneratorOptions(opts));
            return serviceProviderBuilder.Build();
        }

        protected static string GetBeginningCommandOutputLog()
        {
            var output = new StringBuilder();
            _ = output.AppendLine("Operation performed by Microsoft Oryx, https://github.com/Microsoft/Oryx");
            _ = output.AppendLine("You can report issues at https://github.com/Microsoft/Oryx/issues");
            var buildInfo = new DefinitionListFormatter();
            var oryxVersion = Program.GetVersion();
            var oryxCommitId = Program.GetMetadataValue(Program.GitCommit);
            var oryxReleaseTagName = Program.GetMetadataValue(Program.ReleaseTagName);
            _ = buildInfo.AddDefinition(
                "Oryx Version",
                $"{oryxVersion}, " +
                $"Commit: {oryxCommitId}, " +
                $"ReleaseTagName: {oryxReleaseTagName}");
            _ = output.AppendLine();
            _ = output.Append(buildInfo.ToString());
            return output.ToString();
        }

        protected static string ParseOsTypeFile()
        {
            var ostypeFilePath = Path.Join("/opt", "oryx", FilePaths.OsTypeFileName);
            if (File.Exists(ostypeFilePath))
            {
                // these file contents are in the format <OS_type>|<Os_version>, e.g. DEBIAN|BULLSEYE
                // we want the Os_version part only, as all lowercase
                var fullOsTypeFileContents = File.ReadAllText(ostypeFilePath);
                return fullOsTypeFileContents.Split("|").TakeLast(1).SingleOrDefault().Trim().ToLowerInvariant();
            }

            return null;
        }

        protected static string ParseImageTypeFile()
        {
            var imagetypeFilePath = Path.Join("/opt", "oryx", FilePaths.ImageTypeFileName);
            if (File.Exists(imagetypeFilePath))
            {
                // these file contents are a single line that contains the image type
                return File.ReadAllText(imagetypeFilePath).Trim().ToLowerInvariant();
            }

            return null;
        }

        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            this.DisposeServiceProvider();
        }

        private void DisposeServiceProvider()
        {
            if (this.serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            // Sends queued messages to Application Insights
            NLog.LogManager.Flush(LoggingConstants.FlushTimeout);
            NLog.LogManager.Shutdown();
        }
    }
}