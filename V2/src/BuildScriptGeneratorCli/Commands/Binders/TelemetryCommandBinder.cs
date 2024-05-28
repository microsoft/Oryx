// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.CommandLine;
using System.CommandLine.Binding;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Commands
{
    public class TelemetryCommandBinder : CommandBaseBinder<TelemetryCommandProperty>
    {
        public TelemetryCommandBinder(
            Option<string> eventName,
            Option<double> processingTime,
            Option<string[]> properties,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            this.EventName = eventName;
            this.ProcessingTime = processingTime;
            this.Properties = properties;
        }

        private Option<string> EventName { get; set; }

        private Option<double> ProcessingTime { get; set; }

        private Option<string[]> Properties { get; set; }

        protected override TelemetryCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new TelemetryCommandProperty
            {
                EventName = bindingContext.ParseResult.GetValueForOption(this.EventName),
                ProcessingTime = bindingContext.ParseResult.GetValueForOption(this.ProcessingTime),
                Properties = bindingContext.ParseResult.GetValueForOption(this.Properties),
                LogPath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
            };
    }
}
