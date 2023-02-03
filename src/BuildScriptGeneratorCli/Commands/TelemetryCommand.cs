// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command(Name, Description = "[INTERNAL ONLY COMMAND]", ShowInHelpText = false)]
    internal class TelemetryCommand : CommandBase
    {
        public const string Name = "telemetry";

        [Option(OptionTemplates.EventName, CommandOptionType.SingleValue, ShowInHelpText = false)]
        public string EventName { get; set; }

        [Option(OptionTemplates.ProcessingTime, CommandOptionType.SingleValue, ShowInHelpText = false)]
        public double ProcessingTime { get; set; }

        [Option(OptionTemplates.Property, CommandOptionType.MultipleValue, ShowInHelpText = false)]
        public string[] Properties { get; set; }

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
