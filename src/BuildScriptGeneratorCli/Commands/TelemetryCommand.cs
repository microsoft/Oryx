// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGeneratorCli.Commands;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class TelemetryCommand : CommandBase
    {
        public const string Name = "telemetry";

        public TelemetryCommand()
        {
        }

        public TelemetryCommand(TelemetryCommandProperty input)
        {
            this.EventName = input.EventName;
            this.ProcessingTime = input.ProcessingTime;
            this.Properties = input.Properties;
            this.LogFilePath = input.LogFilePath;
            this.DebugMode = input.DebugMode;
        }

        public string EventName { get; set; }

        public double ProcessingTime { get; set; }

        public string[] Properties { get; set; }

        public static Command Export(IConsole console)
        {
            var eventNameOption = new Option<string>(name: OptionTemplates.EventName);
            var processingTimeOption = new Option<double>(name: OptionTemplates.ProcessingTime);
            var propertyOption = new Option<string[]>(aliases: OptionTemplates.Property);
            var logFile = new Option<string>(name: OptionTemplates.Log);
            var debugOption = new Option<bool>(name: OptionTemplates.Debug);

            var command = new Command("telemetry", "[INTERNAL ONLY COMMAND]")
            {
                eventNameOption,
                processingTimeOption,
                propertyOption,
                logFile,
                debugOption,
            };
            command.IsHidden = true;

            command.SetHandler(
                (prop) =>
                {
                    var telemetryCommand = new TelemetryCommand(prop);
                    return Task.FromResult(telemetryCommand.OnExecute(console));
                },
                new TelemetryCommandBinder(
                    eventNameOption,
                    processingTimeOption,
                    propertyOption,
                    logFile,
                    debugOption));
            return command;
        }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<TelemetryCommand>>();

            var eventProps = new Dictionary<string, string>();
            if (this.Properties?.Length > 0)
            {
                foreach (var prop in this.Properties)
                {
                    var propSplit = prop.Split('=');
                    if (propSplit.Length != 2)
                    {
                        console.WriteErrorLine($"Invalid property, '{prop}', provided and will be skipped. Properties must be in the format --property KEY=VALUE");
                        continue;
                    }

                    eventProps.Add(propSplit[0], propSplit[1]);
                }
            }

            if (this.ProcessingTime > 0)
            {
                logger.LogTimedEvent(this.EventName, this.ProcessingTime, eventProps);
            }
            else
            {
                logger.LogEvent(this.EventName, eventProps);
            }

            return ProcessConstants.ExitSuccess;
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            if (string.IsNullOrEmpty(this.EventName))
            {
                console.WriteErrorLine("The 'oryx telemetry' command requires a value for --event-name.");
                return false;
            }

            return true;
        }
    }
}
