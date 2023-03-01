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
        private Option<string> eventName;
        private Option<double> processingTime;
        private Option<string[]> properties;

        public TelemetryCommandBinder(
            Option<string> eventName,
            Option<double> processingTime,
            Option<string[]> properties,
            Option<string> logPath,
            Option<bool> debugMode)
            : base(logPath, debugMode)
        {
            this.eventName = eventName;
            this.processingTime = processingTime;
            this.properties = properties;
        }

        protected override TelemetryCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new TelemetryCommandProperty
            {
                EventName = bindingContext.ParseResult.GetValueForOption(this.eventName),
                ProcessingTime = bindingContext.ParseResult.GetValueForOption(this.processingTime),
                Properties = bindingContext.ParseResult.GetValueForOption(this.properties),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(this.LogPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.DebugMode),
            };
    }
}
