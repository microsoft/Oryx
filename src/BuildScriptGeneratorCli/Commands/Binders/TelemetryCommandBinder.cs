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
        private Option<string> eventNameOption;
        private Option<double> processingTimeOption;
        private Option<string[]> propertiesOption;

        public TelemetryCommandBinder(
            Option<string> eventNameOption,
            Option<double> processingTimeOption,
            Option<string[]> propertiesOption,
            Option<string> logFile,
            Option<bool> debugOption)
            : base(logFile, debugOption)
        {
            this.eventNameOption = eventNameOption;
            this.processingTimeOption = processingTimeOption;
            this.propertiesOption = propertiesOption;
        }

        protected override TelemetryCommandProperty GetBoundValue(BindingContext bindingContext) =>
            new TelemetryCommandProperty
            {
                EventName = bindingContext.ParseResult.GetValueForOption(this.eventNameOption),
                ProcessingTime = bindingContext.ParseResult.GetValueForOption(this.processingTimeOption),
                Properties = bindingContext.ParseResult.GetValueForOption(this.propertiesOption),
                LogFilePath = bindingContext.ParseResult.GetValueForOption(this.logPath),
                DebugMode = bindingContext.ParseResult.GetValueForOption(this.debugMode),
            };
    }
}
